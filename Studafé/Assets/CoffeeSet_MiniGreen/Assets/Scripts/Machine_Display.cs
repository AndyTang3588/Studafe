
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace VRCCoffeeSet
{
    [AddComponentMenu("MiniGreen/Coffee Set/Machine Display")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Machine_Display : UdonSharpBehaviour
    {
        VRCPlayerApi localPlayer;

        Text text_Display;
        public float time_Delay = 5;
        float time_Next = 0;

        [UdonSynced] string strDisplay = "";

        void Start()
        {
            localPlayer = Networking.LocalPlayer;
            text_Display = transform.GetChild(0).GetComponent<Text>();
        }
        void LateUpdate() {
            if (strDisplay == "") return;
            
            if (!Networking.IsOwner(localPlayer, gameObject)) return;

            if (Time.time < time_Next) return;

            strDisplay = "";
            RequestSerialization();
            LocalUpdate();
        }

        public override void OnDeserialization() => LocalUpdate();
        void LocalUpdate()
        {
            text_Display.text = strDisplay;
        }

        public void SetText(string value, float time)
        {
            Networking.SetOwner(localPlayer, gameObject);
            strDisplay = value;
            time_Next = Time.time + time;
            RequestSerialization();
            LocalUpdate();
        }
    }
}