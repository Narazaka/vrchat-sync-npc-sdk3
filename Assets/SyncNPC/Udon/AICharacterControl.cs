
using UdonSharp;
using UnityEngine;
using UnityEngine.AI;
using VRC.SDKBase;
using VRC.Udon;

namespace SyncNPC
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class AICharacterControl : UdonSharpBehaviour
    {
        [Header("ターゲットオブジェクト（オプショナル）")]
        public Transform[] TargetTransforms;
        [Header("ターゲット取ってないときにさまよい歩く")]
        public bool CanWandering = true;
        [Header("さまよう間隔最大[秒]")]
        public float MaxWanderIdleTime = 7f;
        [Header("さまよう間隔最小[秒]")]
        public float MinWanderIdleTime = 2f;
        [Header("さまよう移動距離最大[m]")]
        public float MaxWanderDistance = 10f;
        [Header("走るスピード")]
        public float RunSpeed = 1f;
        [Header("歩くスピード")]
        public float WalkSpeed = 0.47f;
        [Header("さまよう時走りになる距離")]
        public float RunDistanceWhenWandering = 4f;
        [Header("ターゲットに近付く時歩きになる距離")]
        public float RunDistanceWhenApproachingTarget = 2f;
        [Header("ターゲットから離される時走りになる距離")]
        public float RunDistanceWhenSeparatingTarget = 3.5f;

        [UdonSynced, FieldChangeCallback(nameof(TargetPlayerId)), SerializeField, HideInInspector]
        int _TargetPlayerId = -1;
        public int TargetPlayerId
        {
            get => _TargetPlayerId;
            private set
            {
                _TargetPlayerId = value;
                TargetTransform = null;
                TargetPlayer = VRCPlayerApi.GetPlayerById(_TargetPlayerId);
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(TargetTransformIndex)), SerializeField, HideInInspector]
        int _TargetTransformIndex = -1;
        public int TargetTransformIndex
        {
            get => _TargetTransformIndex;
            private set
            {
                _TargetTransformIndex = value;
                TargetPlayer = null;
                TargetTransform = _TargetTransformIndex < TargetTransforms.Length ? TargetTransforms[_TargetTransformIndex] : null;
            }
        }

        [UdonSynced, HideInInspector]
        public Vector3 WanderingTargetPosition;
        bool IsMovingToNext;
        bool HasSetNextPosition;
        float InternalWaitTime;
        float PreviousDistance;

        VRCPlayerApi TargetPlayer;
        Transform TargetTransform;
        UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter Character;
        NavMeshAgent Agent;

        void OnEnable()
        {
            Agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
            Character = GetComponent<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter>();

            Agent.updateRotation = false;
            Agent.updatePosition = true;
        }

        void Update()
        {
            if (Agent == null) return;
            if (TargetTransform != null)
            {
                Agent.destination = TargetTransform.position;
            }
            else if (TargetPlayer != null)
            {
                Agent.destination = TargetPlayer.GetPosition();
            }
            else
            {
                if (CanWandering)
                {
                    HandleWandering();
                }
                else
                {
                    Agent.destination = transform.position;
                }
            }

            if (Character == null) return;
            var currentDistance = Agent.remainingDistance - Agent.stoppingDistance;
            if (currentDistance > 0f)
            {
                if (CanWandering && TargetTransform == null && TargetPlayer == null)
                {
                    // wandering
                }
                else if (currentDistance > PreviousDistance) // プレイヤーが遠ざかっている
                {
                    Agent.speed = Agent.remainingDistance > 3.5f ? RunSpeed : WalkSpeed;
                }
                else
                {
                    Agent.speed = Agent.remainingDistance > 2f ? RunSpeed : WalkSpeed;
                }
                Character.Move(Agent.desiredVelocity, false, false);
                PreviousDistance = currentDistance;
            }
            else
            {
                Character.Move(Vector3.zero, false, false);
            }
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (player == TargetPlayer)
            {
                TargetPlayer = null; // local
                SetTargetPlayerId(-1); // owner
            }
        }

        public void SetTargetPlayerId(int id)
        {
            if (!Networking.IsOwner(gameObject)) return;
            TargetPlayerId = id;
            RequestSerialization();
        }

        public void SetTargetTransformId(int index)
        {
            if (!Networking.IsOwner(gameObject)) return;
            TargetTransformIndex = index;
            RequestSerialization();
        }

        // cf. https://github.com/Centauri2442/SimpleAI
        void HandleWandering()
        {
            if (Networking.IsOwner(gameObject))
            {
                if (!IsMovingToNext) // Runs while the AI is idle
                {
                    HasSetNextPosition = false;

                    InternalWaitTime -= Time.deltaTime;

                    if (InternalWaitTime < 0f)
                    {
                        IsMovingToNext = true;

                        InternalWaitTime = Random.Range(MinWanderIdleTime, MaxWanderIdleTime);

                        Agent.destination = WanderingTargetPosition = CalculateRandomPosition(MaxWanderDistance);
                        Agent.speed = Agent.destination.magnitude > 4f ? RunSpeed : WalkSpeed;
                        HasSetNextPosition = true;

                        //SendCustomEventDelayedSeconds(nameof(StartWandering), WanderIdleTime); Alternative to an internal timer
                    }
                }

                if (Agent.remainingDistance <= Agent.stoppingDistance // | Remaining distance must always be greater than the stopping distance!
                    && HasSetNextPosition) //Makes sure a new position has been set!
                {
                    IsMovingToNext = false;

                    InternalWaitTime = Random.Range(MinWanderIdleTime, MaxWanderIdleTime);
                }
            }
            else
            {
                Agent.destination = WanderingTargetPosition;
            }
        }

        // 
        Vector3 CalculateRandomPosition(float dist) // Calculates a navmesh position within specific distance constraints!
        {
            var randDir = transform.position + Random.insideUnitSphere * dist; // Finds a point within a 3D space around the AI!

            NavMeshHit hit;

            NavMesh.SamplePosition(randDir, out hit, dist, NavMesh.AllAreas); // Uses the calculated point to find the nearest navmesh position!

            return hit.position;
        }
    }
}
