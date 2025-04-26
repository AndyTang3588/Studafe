
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace VRCCoffeeSet
{
    [AddComponentMenu("MiniGreen/Coffee Set/Machine Button")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Machine_Button : UdonSharpBehaviour
    {
        [HideInInspector] public Machine_Component m_target;
        [HideInInspector] public string m_nameEvent = "InputButton";
        Vector3 posDefault;
        Quaternion rotDefault;

        void Start()
        {
            posDefault = transform.position;
            rotDefault = transform.rotation;
            if (VRCPlayerApi.GetPlayerCount() == 0) return;
            if (!Networking.LocalPlayer.IsUserInVR()) ((VRC_Pickup)GetComponent(typeof(VRC_Pickup))).pickupable = false;
        }

        public override void OnPickup()
        {
            ((VRC_Pickup)GetComponent(typeof(VRC_Pickup))).Drop();
            transform.SetPositionAndRotation(posDefault, rotDefault);
        }
        public override void Interact() {
            Debug.Log($"{m_target.name} / {m_nameEvent}");
            m_target.SendCustomEvent(m_nameEvent);
        }

        public void SetInteract(bool value)
        {
            if (VRCPlayerApi.GetPlayerCount() == 0) return;
            if (Networking.LocalPlayer.IsUserInVR()) ((VRC_Pickup)GetComponent(typeof(VRC_Pickup))).pickupable = value;
            DisableInteractive = !value;
        }

        /// <summary> Set color of button </summary>
        /// <param name="state">0_Off, 1_Blue, 2_Green</param>
        public void SetColor(int colorIndex) { GetComponent<Animator>().SetInteger("State", colorIndex); }
        public void SetWorking(bool value) { GetComponent<Animator>().SetBool("State", value); }
    }
}