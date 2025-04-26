
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace VRCCoffeeSet
{
    [AddComponentMenu("MiniGreen/Coffee Set/Sink")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Table_Sink : UdonSharpBehaviour
    {
        public Animator animTable;
        public ParticleSystem particleWater;
        [UdonSynced] bool stateWater = false;

        void Start() {
            SetWater();
            if (VRCPlayerApi.GetPlayerCount() > 0) if (!Networking.LocalPlayer.IsUserInVR())
                ((VRC_Pickup)GetComponent(typeof(VRC_Pickup))).pickupable = false;
        }

        public override void OnDeserialization() {
            SetWater();
        }

        public override void Interact() {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            stateWater = !stateWater;
            RequestSerialization();
            SetWater();
        }

        void SetWater() {
            animTable.SetBool("State", stateWater);
            if (stateWater) particleWater.Play();
            else particleWater.Stop();
        }

        public override void OnPickup() {
            ((VRC_Pickup)GetComponent(typeof(VRC_Pickup))).Drop();
        }
    }
}