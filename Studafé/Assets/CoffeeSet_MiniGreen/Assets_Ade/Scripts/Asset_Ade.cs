#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VRCCoffeeSet
{
    static public class Asset_Ade
    {
        const string PATHOBJ = "/Assets_Ade/Prefabs/Objects_Ade.prefab";
        const string PATHCUPGLASS = "/Assets_Ade/Prefabs/CupGlass_Ade.prefab";

        static public void Initialize(MainController_Editor editor)
        {
            Transform transCtrl = editor.controller.transform;
            
            for (int i = 0; i < transCtrl.childCount; i++)
            {
                Transform tempTrans = transCtrl.GetChild(i);

                if (tempTrans.name == "Objects_Ade") break;
                else if (i == transCtrl.childCount - 1)
                {
                    UnityEngine.Object objDLC = AssetDatabase.LoadAssetAtPath(editor.pathRoot + PATHOBJ, typeof(UnityEngine.Object));
                    if (!objDLC)
                    {
                        Debug.LogError(
                            "Initializing error of Coffee Set : " +
                            "Detected DLC folder but there is no 'Objects_Ade' prefab. " +
                            "Re-import the DLC Prefab would be solve this problem."
                        );
                        return;
                    }
                    GameObject go = PrefabUtility.InstantiatePrefab(objDLC, transCtrl) as GameObject;
                    Tool[] tools = go.GetComponentsInChildren<Tool>();
                    editor.AddToolInList(tools);
                }
            }

            string pathMat = $"{editor.pathRoot}/Assets_Ade/Materials/";
            editor.controller.mat_Ade = new Material[8];
            editor.controller.mat_Ade[0] = AssetDatabase.LoadAssetAtPath($"{pathMat}Juice_Lemon.mat", typeof(Material)) as Material;
            editor.controller.mat_Ade[2] = AssetDatabase.LoadAssetAtPath($"{pathMat}Juice_Grape.mat", typeof(Material)) as Material;
            editor.controller.mat_Ade[4] = AssetDatabase.LoadAssetAtPath($"{pathMat}Juice_Grapefruit.mat", typeof(Material)) as Material;
            editor.controller.mat_Ade[6] = AssetDatabase.LoadAssetAtPath($"{pathMat}Juice_Strawberry.mat", typeof(Material)) as Material;
            editor.controller.mat_Ade[1] = AssetDatabase.LoadAssetAtPath($"{pathMat}Ade_Lemon.mat", typeof(Material)) as Material;
            editor.controller.mat_Ade[3] = AssetDatabase.LoadAssetAtPath($"{pathMat}Ade_Grape.mat", typeof(Material)) as Material;
            editor.controller.mat_Ade[5] = AssetDatabase.LoadAssetAtPath($"{pathMat}Ade_Grapefruit.mat", typeof(Material)) as Material;
            editor.controller.mat_Ade[7] = AssetDatabase.LoadAssetAtPath($"{pathMat}Ade_Strawberry.mat", typeof(Material)) as Material;

            foreach (Material mat in editor.controller.mat_Ade) if (!mat)
            {
                Debug.LogError(
                    "Initializing error of Coffee Set : " +
                    "Couldn't find material. " +
                    "Re-import the DLC Prefab would be solve this problem."
                );
                return;
            }

            foreach (Cup_Coffee udon in editor.controller.dish_Glass)
            {
                if (udon.m_object[4]) continue;
                GameObject go = PrefabUtility.InstantiatePrefab(
                    AssetDatabase.LoadAssetAtPath<GameObject>(editor.pathRoot + PATHCUPGLASS), udon.transform
                ) as GameObject;
                go.name = "CupGlass_Ade";
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                udon.m_object[4] = go.transform;
                udon.m_particle = go.GetComponentInChildren<ParticleSystem>();
            }

            editor.controller.m_init[1] = true;
        }
    
        static public void Reset(MainController_Editor editor, GameObject prefab, List<Cup_Coffee> cups)
        {
            GameObject obj = prefab.transform.Find("Objects_Ade")?.gameObject;
            if (!obj) return;

            Object.DestroyImmediate(obj);
            foreach (Cup_Coffee item in cups)
            {
                if (item.m_type != TypesCup.Glass) continue;
                Object.DestroyImmediate( item.transform.Find(item.m_object[4].name).gameObject );
            }
        }
    }
}
#endif