
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
using UdonSharpEditor;
#endif

namespace VRCCoffeeSet
{
    [AddComponentMenu("VRCCoffeeSet/CoffeeSet - Sound Mute Zone")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SoundMuteZone : UdonSharpBehaviour
    {
        [SerializeField] MainController script;

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (player != Networking.LocalPlayer) return;
            if (script) script.SoundMute(true);
        }
        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (player != Networking.LocalPlayer) return;
            if (script) script.SoundMute(false);
        }
    }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
    [CustomEditor(typeof(SoundMuteZone))]
    public class SoundMuteZone_Editor : Editor
    {
        SoundMuteZone script;
        MainController controller;

        private void Awake()
        {
            if (Selection.objects.Length > 1) return;
            script = target as SoundMuteZone;
            serializedObject.FindProperty("script").objectReferenceValue = FindObjectOfType<MainController>();
            serializedObject.ApplyModifiedProperties();
        }
        public override void OnInspectorGUI()
        {
            if (Selection.objects.Length > 1) return;
            
            if (!script.gameObject.scene.IsValid())
            {
                EditorGUILayout.HelpBox("Drag and drop onto hierarchy.", MessageType.Info);
                return;
            }

            if (!serializedObject.FindProperty("script").objectReferenceValue)
            {
                EditorGUILayout.HelpBox("Main Controller Not Found!", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("Main Controller has been detected.", MessageType.Info);
            }
        }
    }
#endif
}