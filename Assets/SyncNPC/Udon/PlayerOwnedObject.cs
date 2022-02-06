
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace SyncNPC
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerOwnedObject : UdonSharpBehaviour
    {
        [HideInInspector]
        public VRCPlayerApi Owner;

        public void _OnOwnerSet()
        {
            if (Owner.isLocal)
            {
                for (var i = 0; i < transform.childCount; ++i)
                {
                    Networking.SetOwner(Owner, transform.GetChild(i).gameObject);
                }
            }
            for (var i = 0; i < transform.childCount; ++i)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
        }

        public void _OnCleanup()
        {
            for (var i = 0; i < transform.childCount; ++i)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}
