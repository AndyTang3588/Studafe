
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace VRCCoffeeSet
{
    [AddComponentMenu("MiniGreen/Coffee Set/Menu")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    public class Menu_Plate : UdonSharpBehaviour
    {
        [Header("DO NOT Change variables")]
        [Header("")]
        public GameObject[] Lang;
        [UdonSynced] int currentLang = 0;
        int currentLang_Old = 0;

        void Start() {
            for (int i=0; i<Lang.Length; i++) {
                if (i==0) Lang[i].SetActive(true);
                else Lang[i].SetActive(false);
            }
        }

        public override void OnDeserialization() {
            if (currentLang != currentLang_Old) ChangeLang();
        }

        public override void OnPickupUseDown() {
            if (currentLang == Lang.Length-1) currentLang = 0;
            else currentLang++;
            ChangeLang();
        }

        void ChangeLang() {
            Lang[currentLang_Old].SetActive(false);
            Lang[currentLang].SetActive(true);
            currentLang_Old = currentLang;
        }
    }
}