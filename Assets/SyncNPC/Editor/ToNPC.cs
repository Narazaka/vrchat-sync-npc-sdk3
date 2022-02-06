using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UdonSharpEditor;
using System.IO;
using System.Text.RegularExpressions;

namespace SyncNPC
{
    public class ToNPC
    {
        [MenuItem("CONTEXT/Animator/ToNPC")]
        public static void AnimatorToNPC(MenuCommand menuCommand)
        {
            if (!Patched())
            {
                PatchThirdPersonCharacter();
                EditorUtility.DisplayDialog("パッチ完了", "再度同じメニューを実行してください", "OK");
                return;
            }
            var animator = menuCommand.context as Animator;
            animator.runtimeAnimatorController = Controller();
            var obj = animator.gameObject;
            GetOrAddUdonSharpComponent<AICharacterControl>(obj);
            GetOrAddUdonSharpComponent<GetNPCOwner>(obj);
            GetOrAddUdonSharpComponent(obj, typeof(UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter));

            var us = obj.GetComponents<VRC.Udon.UdonBehaviour>();
            foreach (var u in us)
            {
                u.interactText = "ついてきて！";
            }
            var collider = obj.GetOrAddComponent<CapsuleCollider>();
            collider.center = Vector3.up * 0.76f;
            collider.radius = 0.25f;
            collider.height = 1.5f;
            var rigidBody = obj.GetOrAddComponent<Rigidbody>();
            rigidBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            var pickup = obj.GetOrAddComponent<VRC.SDK3.Components.VRCPickup>();
            pickup.pickupable = false;
            var agent = obj.GetOrAddComponent<UnityEngine.AI.NavMeshAgent>();
            agent.stoppingDistance = 1.5f;
            agent.radius = 0.25f;
            agent.height = 1.5f;
        }

        static T GetOrAddUdonSharpComponent<T>(GameObject obj) where T : UdonSharp.UdonSharpBehaviour
        {
            var component = obj.GetUdonSharpComponent<T>();
            return component == null ? obj.AddUdonSharpComponent<T>() : component;
        }

        static object GetOrAddUdonSharpComponent(GameObject obj, System.Type type) 
        {
            var component = obj.GetUdonSharpComponent(type);
            return component == null ? obj.AddUdonSharpComponent(type) : component;
        }

        static UnityEditor.Animations.AnimatorController Controller()
        {
            return AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>("Assets/Standard Assets/Characters/ThirdPersonCharacter/Animator/ThirdPersonAnimatorController.controller");
        }

        static string ThirdPersonCharacterPathBase = "Assets/Standard Assets/Characters/ThirdPersonCharacter/Scripts/ThirdPersonCharacter";
        static string ThirdPersonCharacterCsPath = $"{ThirdPersonCharacterPathBase}.cs";
        static string ThirdPersonCharacterUdonSharpPath = $"{ThirdPersonCharacterPathBase}.asset";
        static Regex USharpRegex = new Regex(@"(public void Move\(Vector3 move, bool crouch, bool jump\)\s*\{[\r\n]+\t*)([\r\n])");
        static string USharpResult = "$1if (m_Animator == null) return;$2";

        static bool Patched()
        {
            return File.Exists(ThirdPersonCharacterUdonSharpPath);
        }

        static void PatchThirdPersonCharacter()
        {
            if (!Patched())
            {
                var us = ScriptableObject.CreateInstance<UdonSharp.UdonSharpProgramAsset>();
                us.sourceCsScript = AssetDatabase.LoadAssetAtPath<MonoScript>(ThirdPersonCharacterCsPath);
                AssetDatabase.CreateAsset(us, ThirdPersonCharacterUdonSharpPath);

                var code = USharpRegex.Replace(
                    File.ReadAllText(ThirdPersonCharacterCsPath),
                    USharpResult
                )
                    .Replace("MonoBehaviour", "UdonSharp.UdonSharpBehaviour")
                    .Replace("void Start()", "void OnEnable()")
                    .Replace("m_Rigidbody.constraints", "// m_Rigidbody.constraints")
                    .Replace("m_GroundCheckDistance = 0.1f", "m_GroundCheckDistance = 0.3f");
                File.WriteAllText(ThirdPersonCharacterCsPath, code);
            }
        }
    }
}
