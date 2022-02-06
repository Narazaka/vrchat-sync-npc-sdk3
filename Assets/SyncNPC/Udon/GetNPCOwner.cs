
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace SyncNPC
{
    public class GetNPCOwner : UdonSharpBehaviour
    {
        [Header("ついてくるのをやめさせられる")]
        public bool AllowRelease;
        AICharacterControl Character;

        void OnEnable()
        {
            Character = GetComponent<AICharacterControl>();
        }

        public override void Interact()
        {
            var player = Networking.LocalPlayer;
            Networking.SetOwner(player, gameObject);
            if (AllowRelease && Character.TargetPlayerId == player.playerId)
            {
                Character.SetTargetPlayerId(-1);
            }
            else
            {
                Character.SetTargetPlayerId(player.playerId);
            }
        }

        void Update()
        {
            DisableInteractive = Character == null || (!AllowRelease && Networking.LocalPlayer.playerId == Character.TargetPlayerId);
        }
    }
}
