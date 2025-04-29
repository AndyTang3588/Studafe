
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Yodokorochan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Yodo_HapticHandCollider : UdonSharpBehaviour
    {
        [HideInInspector]
        public bool Yodo_isHapticCollider = true;

        public bool Yodo_VibrateLeftHand = false;
        public bool Yodo_VibrateRightHand = false;
    }
}
