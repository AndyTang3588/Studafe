#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VRCCoffeeSet
{
    [AddComponentMenu("VRCCoffeeSet/CoffeeSet - Position Save Target")]
    public class PositionSaveTarget : MonoBehaviour
    {
        
    }
    
    [CustomEditor(typeof(PositionSaveTarget))]
    public class PositionSaveTarget_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();
        }
    }
}
#endif