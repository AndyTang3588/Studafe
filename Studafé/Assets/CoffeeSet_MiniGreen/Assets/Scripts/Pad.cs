
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace VRCCoffeeSet
{
    [AddComponentMenu("MiniGreen/Coffee Set/Pad")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    public class Pad : UdonSharpBehaviour
    {
        Animator animatorPad;
        [Header("DO NOT Change variables")]
        [Header("")]
        public Text textHolder;
        public GameObject canvasDisplay;
        public GameObject[] Menus;
        [Tooltip("0_EN, 1_KR")]
        public Transform btnLang;
        byte currentLang = 0;
        sbyte currentMenu = -1;
        Transform currentSection;
        Transform[] currentPage = new Transform[5];
        Transform currentContent;
        
        void Start() {
            textHolder.text = "";
            animatorPad = GetComponent<Animator>();
            canvasDisplay.SetActive(false);
            canvasDisplay.GetComponent<RectMask2D>().enabled = true;
            for (int i=0; i<Menus.Length; i++) {
                if (i == currentLang) Menus[i].SetActive(true);
                else Menus[i].SetActive(false);
                for (int i2=0; i2<Menus[i].transform.childCount; i2++) {
                    GameObject tempObj = Menus[i].transform.GetChild(i2).gameObject;
                    tempObj.SetActive(false);
                    tempObj.GetComponent<Mask>().enabled = true;
                }
            }
        }

        public override void OnPickupUseDown() {
            if (!canvasDisplay.activeSelf) {
                canvasDisplay.SetActive(true);
            }
            else {
                canvasDisplay.SetActive(false);
                if (currentMenu > -1) {
                    Menus[currentLang].GetComponent<Animator>().SetInteger("IndexMenu", -1);
                    DisableMenu();
                    DisablePages();
                }
            }
        }

        public void Btn_Lang() {
            btnLang.GetChild(currentLang).gameObject.SetActive(false);
            Menus[currentLang].SetActive(false);
            if (currentLang == btnLang.childCount-1) currentLang = 0;
            else currentLang++;
            btnLang.GetChild(currentLang).gameObject.SetActive(true);
            Menus[currentLang].SetActive(true);
        }

        public void Btn_Menu() {
            currentMenu = (sbyte)int.Parse(textHolder.text);
            GameObject tempMenu = GetMenu(currentMenu);
            tempMenu.SetActive(true);
            Menus[currentLang].GetComponent<Animator>().SetInteger("IndexMenu", currentMenu);
            currentSection = tempMenu.transform.GetChild(0);
            ResetSections(tempMenu);
        }
        public void Btn_ToMenu() {
            Menus[currentLang].GetComponent<Animator>().SetInteger("IndexMenu", -1);
            SendCustomEventDelayedSeconds("DisableMenu", 0.15f);
            currentSection = null;
        }
        public void DisableMenu() {
            GetMenu(currentMenu).SetActive(false);
            currentMenu = -1;
        }
        void ResetSections(GameObject objMenu) {
            for (int i=0; i<objMenu.transform.childCount; i++) {
                GameObject tempObj = objMenu.transform.GetChild(i).gameObject;
                if (!tempObj.name.Contains("Section")) return;

                tempObj.SetActive(true);
                if (i == 0) tempObj.GetComponent<Animator>().SetTrigger("Enable");
                else tempObj.GetComponent<Animator>().SetTrigger("Disable");
            }
        }
        GameObject GetMenu(int index) { return Menus[currentLang].transform.GetChild(index).gameObject; }

        public void Btn_Sub() {
            string[] tempText = textHolder.text.Split(new char[] {'_'});
            int numSection = int.Parse(tempText[0]);
            int numPage = int.Parse(tempText[1]);

            currentSection.GetComponent<Animator>().SetTrigger("Out_L");
            currentSection = GetMenu(currentMenu).transform.GetChild(numSection);
            currentSection.GetComponent<Animator>().SetTrigger("In_R");

            int indexPage = GetPageIndex();
            currentPage[indexPage] = currentSection.GetChild(numPage);
            currentPage[indexPage].gameObject.SetActive(true);
        }
        public void Btn_Undo() {
            int numSection = int.Parse(textHolder.text);

            currentSection.GetComponent<Animator>().SetTrigger("Out_R");
            currentSection = GetMenu(currentMenu).transform.GetChild(numSection);
            currentSection.GetComponent<Animator>().SetTrigger("In_L");

            SendCustomEventDelayedSeconds("DisablePage",0.1f);
        }
        public void DisablePage() {
            int indexPage = GetPageIndex();
            currentPage[indexPage-1].gameObject.SetActive(false);
            currentPage[indexPage-1] = null;
        }
        public void DisablePages() {
            for (int i=0; i<currentPage.Length; i++) {
                if (!currentPage[i]) return;
                currentPage[i].gameObject.SetActive(false);
            }
        }
        /// <summary>
        ///  Returns index of empty array
        /// </summary>
        int GetPageIndex() {
            int index = 0;
            for (int i=0; i<currentPage.Length; i++) {
                index = i;
                if (!currentPage[i]) break;
            }
            return index;
        }
    }
}