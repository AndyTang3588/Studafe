
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace VRCCoffeeSet
{
    [AddComponentMenu("MiniGreen/Coffee Set/Menu Deposit")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Menu_Deposit : UdonSharpBehaviour
    {
        [Header("DO NOT Change variables")]
        [Header("")]
        public GameObject menuPlate;
        public Transform axisDeposit;

        public override void Interact()
        {
            VRC_Pickup pickupPlate = (VRC_Pickup)menuPlate.GetComponent(typeof(VRC_Pickup));
            if (pickupPlate.IsHeld) return;
            Networking.SetOwner(Networking.LocalPlayer, menuPlate);
            menuPlate.transform.SetPositionAndRotation(axisDeposit.position, axisDeposit.rotation);
        }
    }
}