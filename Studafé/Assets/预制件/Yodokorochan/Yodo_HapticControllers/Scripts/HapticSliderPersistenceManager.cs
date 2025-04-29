
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Yodokorochan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class HapticSliderPersistenceManager : UdonSharpBehaviour
    {
        [UdonSynced]
        [HideInInspector]
        private float SavedValue = 0.0f;

        [SerializeField]
        private Yodo_HapticSlider hapticSlider;

        private void Start()
        {
            if (hapticSlider == null)
            {
                Debug.LogError($"[Yodo]请在 HapticSliderPersistenceManager 中设置 hapticSlider。 位置：[{GetFullPath(transform)}]");
            }
        }

        public void SetSavedValue(float value)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            SavedValue = value;
            RequestSerialization();
        }

        public override void OnPlayerRestored(VRCPlayerApi player)
        {
            if (Utilities.IsValid(player))
            {
                if (player.isLocal)
                {
                    hapticSlider.InitPersistence(this);
                    hapticSlider.SetValueWithoutNotify(SavedValue);
                }
            }
        }

        [RecursiveMethod]
        private string GetFullPath(Transform target)
        {
            if(target.parent == null)
            {
                return target.gameObject.name;
            }
            else
            {
                return $"{GetFullPath(target.parent)}/{target.gameObject.name}";
            }
        }
    }
}
