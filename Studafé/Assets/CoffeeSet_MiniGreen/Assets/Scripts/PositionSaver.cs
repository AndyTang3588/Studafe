#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace VRCCoffeeSet
{
    [AddComponentMenu("VRCCoffeeSet/CoffeeSet - Position Saver")]
    [DisallowMultipleComponent]
    public class PositionSaver : MonoBehaviour
    {
        public void SavePosition(Transform transCtrl)
        {
            // Find MainController
            MainController controller = transCtrl.GetComponent<MainController>();
            if (!controller)
            {
                Debug.LogError("Coffee Set_PositionSaver : Can't find MainController. Please re-import the package.");
                return;
            }

            foreach (var item in controller.transform.GetComponentsInChildren<PositionSaveTarget>()) SavePosition(item.transform);
            foreach (var item in controller.transform.GetComponentsInChildren<Machine>()) SavePosition(item.transform);
            foreach (var item in controller.tool_tamper)
            {
                SavePosition(item.transform.parent);
                SavePosition(item.transform);
            }
            foreach (var item in controller.tool_filter) SavePosition(item.transform);
            foreach (var item in controller.tool_pitcher) SavePosition(item.transform);
            foreach (var item in controller.tool_straw) SavePosition(item.transform);
            foreach (var item in controller.tool_other) SavePosition(item.transform);
            foreach (var item in controller.dish_espressoCup) SavePosition(item.transform);
            foreach (var item in controller.dish_espressoPlate) SavePosition(item.transform);
            foreach (var item in controller.dish_coffeeCup) SavePosition(item.transform);
            foreach (var item in controller.dish_coffeePlate) SavePosition(item.transform);
            foreach (var item in controller.dish_Glass) SavePosition(item.transform);

            Debug.Log("Position Saving!");
        }
        public void LoadPosition(Transform transCtrl)
        {

        }
        void SaveObj(Transform obj, string type)
        {
            string targetName = $"{type}|{obj.name}";
            Transform save = transform.Find(targetName);
            if (!save) save = VRCCoffeeSet_Ext.CreateObject(targetName, obj.position, obj.eulerAngles, transform);
            else save.SetPositionAndRotation(obj.position, obj.rotation);
        }
        void LoadObj(Transform obj)
        {
            Transform save = transform.Find(obj.name);
            if (save) obj.SetPositionAndRotation(save.position, save.rotation);
        }
    }
}
#endif