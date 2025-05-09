﻿using System;
using System.Collections;
using System.Collections.Generic;
using UdonSharp;
using UdonSharpEditor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace VowganVR
{
    [CustomEditor(typeof(SecurityCameraController))]
    public class SecurityCameraControllerEditor : Editor
    {
        
        private SecurityCameraController script;
        private Texture2D banner;
        private ReorderableList cameraList;
        
        private SerializedProperty propCameras;
        private SerializedProperty propRenderCamera;
        private SerializedProperty propCameraScreen;
        private SerializedProperty propFOVSlider;
        private SerializedProperty propCameraNameText;
        
        
        private void OnEnable()
        {
            script = target as SecurityCameraController;
            if (script == null) return;
            
            propCameras = serializedObject.FindProperty(nameof(SecurityCameraController.Cameras));
            propRenderCamera = serializedObject.FindProperty(nameof(SecurityCameraController.RenderCamera));
            propCameraScreen = serializedObject.FindProperty(nameof(SecurityCameraController.CameraScreen));
            propFOVSlider = serializedObject.FindProperty(nameof(SecurityCameraController.FOVSlider));
            propCameraNameText = serializedObject.FindProperty(nameof(SecurityCameraController.CameraNameText));
            
            banner = Resources.Load<Texture2D>("VV_SecurityCameraController");
            
            cameraList = new ReorderableList(serializedObject, propCameras)
            {
                drawHeaderCallback = DrawHeaderCallback,
                drawElementCallback = DrawElementCallback,
            };
        }
        
        private void DrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Cameras");
        }
        
        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.ObjectField(rect, propCameras.GetArrayElementAtIndex(index), GUIContent.none);
        }
        
        public override void OnInspectorGUI()
        {
            Repaint();
            
            UdonSharpGUI.DrawConvertToUdonBehaviourButton(target);
            
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Box(banner, GUIStyle.none);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(MonoScript.FromMonoBehaviour((UdonSharpBehaviour)target), typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();
            GUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            cameraList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();

            StartArea("References");
            EditorGUILayout.PropertyField(propRenderCamera);
            EditorGUILayout.PropertyField(propCameraScreen);
            EditorGUILayout.PropertyField(propFOVSlider);
            EditorGUILayout.PropertyField(propCameraNameText);
            EndArea();
            serializedObject.ApplyModifiedProperties();
        }
        
        private void StartArea(string label = "", bool disabled = false, bool useBox = false)
        {
            GUILayout.Label(label, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUI.BeginDisabledGroup(disabled);
            GUILayout.BeginVertical(useBox ? EditorStyles.helpBox : GUIStyle.none);
        }
        
        private void EndArea()
        {
            EditorGUI.EndDisabledGroup();
            GUILayout.EndVertical();
            EditorGUI.indentLevel--;
            GUILayout.Space(10);
        }
    }
}