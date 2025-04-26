
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
#endif

namespace VRCCoffeeSet
{
    [AddComponentMenu("MiniGreen/Coffee Set/Machine")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Machine : UdonSharpBehaviour
    {
        public TypesMachine m_type;
        public int m_skinIndex;
        public Machine_Display m_display;

        [HideInInspector] public byte m_index;
    }
    
    #if !COMPILER_UDONSHARP && UNITY_EDITOR
    [CustomEditor(typeof(Machine))]
    public class MachineEditor : Editor
    {
        Machine script;
        TypesMachine _type;

        SkinsMachine skinMachine;

        bool toggleVariable = false;
        bool toggleBase;

        private void Awake()
        {
            script = target as Machine;
        }

        void Initialize()
        {
            _type = script.m_type;

            // Display
            if (_type != TypesMachine.Espresso) script.m_display = null;

            // Skin
            skinMachine = SkinManager.GetSkinMachine(script);

            serializedObject.Update();
        }

        public override void OnInspectorGUI()
        {
            if (_type != script.m_type) Initialize();

        #region Skin

            SkinsMachine tempSkinMachine = skinMachine;
            EditorGUI.BeginChangeCheck();
            skinMachine = (SkinsMachine)EditorGUILayout.EnumPopup("Skin Select", skinMachine);
            if ( EditorGUI.EndChangeCheck() )
            {
                string tempStr = SkinManager.ChangeMachine(script, skinMachine);
                if (tempStr == null) return;
                else
                {
                    skinMachine = tempSkinMachine;
                    Debug.LogWarning(tempStr);
                }
            }

        #endregion

            GUILayout.Space(20);
            toggleVariable = EditorGUILayout.Foldout(toggleVariable, "Do not change any variables");
            if (!toggleVariable) return;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_type"), new GUIContent("Machine Type"));
            serializedObject.ApplyModifiedProperties();

            if (_type != script.m_type) Initialize();

            switch (_type)
            {
                case TypesMachine.Espresso:
                    GUILayout.Space(10);
                    EditorGUILayout.PropertyField( serializedObject.FindProperty("m_display") );
                    break;
            }
            
            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(20);
            toggleBase = EditorGUILayout.Foldout(toggleBase, "Udon Script");
            if (toggleBase) base.OnInspectorGUI();
        }
    }
    #endif
}