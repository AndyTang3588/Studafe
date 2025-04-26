
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Yodokorochan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class HapticSwitchPersistenceManager : UdonSharpBehaviour
    {
        [UdonSynced]
        [HideInInspector]
        private bool SavedState = false;

        [SerializeField]
        private Yodo_HapticSwitch hapticSwitch;

        private void Start()
        {
            if (hapticSwitch == null)
            {
                if (transform.parent != null)
                {
                    if (transform.parent.parent != null)
                    {
                        hapticSwitch = transform.parent.parent.GetComponent<Yodo_HapticSwitch>();
                    }
                }
            }
        }

        public void SetSavedState(bool state)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            SavedState = state;
            RequestSerialization();
        }

        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (Utilities.IsValid(player))
            {
                if (player.isLocal)
                {
                    hapticSwitch.InitPersistence(this);
                    hapticSwitch.SetStateWithoutNotify(SavedState);
                }
            }
        }
    }
}
