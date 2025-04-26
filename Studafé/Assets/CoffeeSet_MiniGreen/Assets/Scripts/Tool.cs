
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System;
using UnityEditor;
using UdonSharpEditor;
#endif

namespace VRCCoffeeSet
{
    [AddComponentMenu("MiniGreen/Coffee Set/Tool")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    public class Tool : UdonSharpBehaviour
    {
        public TypesTool m_type;
        public int m_index = -1;
        public int m_skinIndex;
        private MainController M_controller;
        public MainController m_controller
        {
            set { M_controller = value; }
            get
            {
                if (!M_controller) M_controller = GetComponentInParent<MainController>();
                return M_controller;
            }
        }

        private VariableHolder M_variables;
        public VariableHolder m_variables
        {
            set { M_variables = value; }
            get
            {
                if (!M_variables) M_variables = GetComponentInChildren<VariableHolder>();
                return M_variables;
            }
        }

        public VRC_Pickup m_pickup;
        [SerializeField] public Animator m_animator;
        public ParticleSystem[] m_particle;
        public AudioSource m_audio;
        /// <summary> 0_Out, 1_Mid </summary>
        public Transform[] m_axis;

        public bool isCap;
        bool stateCap = true;
        public bool isAnim;
        public float lengthAnim;
        public string eventAnim;
        public bool isTilt;
        public bool isTrig;
        public bool isHold;

        [HideInInspector] public float timeDelay = 0;
        [HideInInspector] public Vector3 defaultPos;
        [HideInInspector] public Quaternion defaultRot;

        void Start()
        {
            defaultPos = transform.position;
            defaultRot = transform.rotation;
            LocalUpdate();
        }

        void LateUpdate()
        {
            CheckTilt();
        }

        void CheckTilt()
        {
            if (m_type != TypesTool.Ingredient) return;
            if (!isTilt || isTrig) return;

            if (isCap && stateCap)
            {
                if (m_particle[0].isPlaying) ParticleStop();
                return;
            }
            if (m_axis[0].position.y < m_axis[1].position.y && m_pickup.IsHeld && !m_particle[0].isPlaying && Time.time > timeDelay) ParticlePlay();
            else if ( (m_axis[0].position.y > m_axis[1].position.y || !m_pickup.IsHeld) && m_particle[0].isPlaying) ParticleStop();
        }

        public void LocalUpdate()
        {
            if (m_type != TypesTool.Filter && m_type != TypesTool.Pitcher) return;

            Material[] tempMat;

            switch (m_type)
            {
                case TypesTool.Filter:
                    m_animator.SetInteger("Content", m_variables.m_info[0]);
                    SkinnedMeshRenderer meshFilter = GetComponent<SkinnedMeshRenderer>();
                    meshFilter.SetBlendShapeWeight(2, m_variables.m_info[1] == 2 ? 100 : 0); // Nozzle S_Hide
                    meshFilter.SetBlendShapeWeight(3, m_variables.m_info[1] == 1 ? 100 : 0); // Nozzle D_Hide
                    break;
                case TypesTool.Pitcher:
                    Renderer meshPitcher = GetComponent<Renderer>();
                    tempMat = meshPitcher.materials;
                    byte stateContent = m_variables.m_info[0];
                    tempMat[1] = stateContent == 0 ? m_controller.mat_Empty : m_controller.mat_Milk[stateContent];
                    meshPitcher.materials = tempMat;
                    break;
            }
        }

        void OnTriggerEnter(Collider other) 
        {
            if (!Networking.IsOwner(other.gameObject)) return;

            switch (m_type)
            {
                case TypesTool.Straw:
                    // Check obj has valid script
                    Cup_Coffee udonCup = other.GetComponent<Cup_Coffee>();
                    if (!udonCup) return;
                    if (udonCup.m_type != TypesCup.Glass) return;
                    // Check there is a straw
                    if (udonCup.m_variables.m_info[6] > 0) return;
                    // Setup
                    udonCup.m_variables.m_info[6] = (byte)(m_skinIndex + 1);
                    udonCup.SerializeVariables();
                    udonCup.LocalUpdate();
                    // Reset Position
                    if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
                    Drop();
                    transform.SetPositionAndRotation(defaultPos, defaultRot);
                    break;
                case TypesTool.Tamper:
                    // Not tampering if tamper is not held
                    if (!m_pickup.IsHeld) return;
                    // Check obj has valid script
                    Tool udonFiter = other.GetComponent<Tool>();
                    if (!udonFiter) return;
                    if (udonFiter.m_type != TypesTool.Filter) return;
                    // Check State
                    if (udonFiter.m_skinIndex != m_skinIndex) return;
                    if (udonFiter.m_variables.m_info[0] != 1) return;
                    // If filter is held, Variable will be set on owner's side
                    if (udonFiter.m_pickup.IsHeld && !Networking.IsOwner(other.gameObject)) return;
                    // Setup
                    udonFiter.m_variables.m_info[0] = 2;
                    udonFiter.SerializeVariables();
                    udonFiter.LocalUpdate();
                    break;
                case TypesTool.Squeezer:
                    string[] nameContact = other.name.Split(new char[] {'_'});
                    if (nameContact[0] != "FruitHalf") return;
                    switch (nameContact[1]) {
                        case "Lemon":
                            SendCustomNetworkEvent(NetworkEventTarget.All, "ParticlePlay");
                            break;
                        case "Grapefruit":
                            SendCustomNetworkEvent(NetworkEventTarget.All, "ParticlePlay_1");
                            break;
                    }
                    break;
                case TypesTool.KnockBox:
                    // Check obj has valid script
                    Tool udonFilter = other.GetComponent<Tool>();
                    if (!udonFilter) return;
                    if (udonFilter.m_type != TypesTool.Filter) return;
                    // Check state
                    if (udonFilter.m_variables.m_info[0] == 0) return;
                    // Setup
                    udonFilter.m_variables.m_info[0] = 0;
                    udonFilter.SerializeVariables();
                    udonFilter.LocalUpdate();
                    if (m_audio) SendCustomNetworkEvent(NetworkEventTarget.All, "AudioPlay");
                    break;
            }
        }

        public override void OnPickup() => FuncPickup();
        public void FuncPickup()
        {
            if (!m_variables) return;
            if (m_variables.m_deposit[0] == 0) return;
            timeDelay = Time.time + m_controller.m_pickupdelay;

            switch (m_type)
            {
                case TypesTool.Tamper:
                    m_variables.m_deposit[0] = 0;
                break;
                case TypesTool.Filter:
                    if (m_variables.m_deposit[0] == 2) m_controller.m_machineF[m_variables.m_deposit[1]].SendCustomNetworkEvent(NetworkEventTarget.All, "DepositOff");
                    m_variables.m_deposit[0] = 0;
                    SerializeVariables();
                break;
                case TypesTool.Pitcher:
                    if (m_variables.m_deposit[0] == 1) m_controller.m_machineP[m_variables.m_deposit[1]].SendCustomNetworkEvent(NetworkEventTarget.All, "DepositOff");
                    m_variables.m_deposit[0] = 0;
                    SerializeVariables();
                break;
            }
        }
        
        public override void OnPickupUseDown()
        {
            switch (m_type)
            {
                case TypesTool.Ingredient:
                    if (isCap && stateCap)
                    {
                        SendCustomNetworkEvent(NetworkEventTarget.All, "CapOpen");
                        timeDelay = Time.time + lengthAnim;
                    }
                    
                    if (lengthAnim > 0 && Time.time < timeDelay) return;

                    if (!isTrig) return;

                    if (isAnim && !isCap) SendCustomNetworkEvent(NetworkEventTarget.All, isHold ? "AnimStart" : "AnimTrig");

                    if (isTilt) if (m_axis[0].position.y > m_axis[1].position.y) return;

                    SendCustomNetworkEvent(NetworkEventTarget.All, "ParticlePlay");

                    if (m_audio) SendCustomNetworkEvent(NetworkEventTarget.All, "AudioPlay");
                    break;
                case TypesTool.Filter:
                    m_variables.m_info[1] = m_variables.m_info[1] == 1 ? (byte)2 : (byte)1;
                    LocalUpdate();
                    SerializeVariables();
                    break;
                case TypesTool.Pitcher:
                    byte stateContent = m_variables.m_info[0];
                    if (m_axis[0].position.y < m_axis[1].position.y && stateContent > 1)
                    {
                        m_variables.m_info[0] = 0;
                        LocalUpdate();
                        SerializeVariables();
                        m_particle[stateContent - 2].Play();
                    }
                break;
            }
        }
        public override void OnPickupUseUp()
        {
            if (isTrig && isHold)
            {
                SendCustomNetworkEvent(NetworkEventTarget.All, "ParticleStop");
                if (isHold) SendCustomNetworkEvent(NetworkEventTarget.All, "AnimStop");
            }
            if (m_audio) SendCustomNetworkEvent(NetworkEventTarget.All, "AudioStop");
        }

        public override void OnDrop()
        {
            if (isCap) SendCustomNetworkEvent(NetworkEventTarget.All, "CapClose");
        }

        void OnParticleCollision(GameObject other)
        {
            if (!Networking.IsOwner(gameObject) || m_type != TypesTool.Pitcher) return;

            string[] nameContact_Splited = other.name.Split(new char[] {'_'});
            if (nameContact_Splited[0] != "Particle") return;

            byte stateContent = m_variables.m_info[0];
            switch (nameContact_Splited[1])
            {
                case "Milk":
                    if (stateContent == 0) m_variables.m_info[0] = 1;
                break;
                case "WaterWash":
                    if (stateContent != 0) m_variables.m_info[0] = 0;
                break;
                default: return;
            }
            if (stateContent == m_variables.m_info[0]) return;
            SerializeVariables();
            LocalUpdate();
        }

        public void SetPickup(bool value) { m_pickup.pickupable = value; }
        public void SetActive(bool value) { GetComponent<Renderer>().enabled = value; }
        public void Drop() { m_pickup.Drop(); }
        public void ParticlePlay() { m_particle[0].Play(); }
        public void ParticlePlay_1() { m_particle[1].Play(); }
        public void ParticleStop() { m_particle[0].Stop(); }
        public void AnimTrig() { m_animator.SetTrigger(eventAnim); }
        public void AnimStart() { m_animator.SetBool(eventAnim, true); }
        public void AnimStop() { m_animator.SetBool(eventAnim, false); }
        public void AudioPlay() { m_audio.Play(); }
        public void AudioStop() { m_audio.Stop(); }
        public void CapOpen()
        {
            timeDelay = Time.time + lengthAnim;
            stateCap = false;
            if (isAnim) m_animator.SetBool(eventAnim, false);
        }
        public void CapClose()
        {
            stateCap = true;
            if (isAnim) m_animator.SetBool(eventAnim, true);
        }

        /// <summary> Local, by MainController </summary>
        public void Call_Reset()
        {
            if (m_pickup.IsHeld || !m_pickup.pickupable) return;

            switch (m_type)
            {
                case TypesTool.Filter:
                case TypesTool.Pitcher:
                    if (m_variables.m_deposit[0] > 0) FuncPickup();
                    break;
            }

            if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);

            transform.SetPositionAndRotation(defaultPos,defaultRot);
        }
        public void SerializeVariables()
        {
            if (!Networking.IsOwner(m_variables.gameObject)) Networking.SetOwner(Networking.LocalPlayer, m_variables.gameObject);
            m_variables.RequestSerialization();
        }
    }

    #if !COMPILER_UDONSHARP && UNITY_EDITOR
    [CustomEditor(typeof(Tool))]
    public class Tool_Editor : Editor 
    {
        Tool script;
        VariableHolder scriptV;
        TypesTool _type;

        int indexSkin;
        
        bool toggleVariable = false;
        bool toggleBase = false;

        GUIStyle paddingLeft = new GUIStyle();

        private void Awake()
        {
            script = target as Tool;
            paddingLeft.padding.left = 15;
        }

        void Initialize()
        {
            _type = script.m_type;
            Transform thisObj = Selection.activeTransform;

            // Skin
            indexSkin = SkinManager.GetSkinTool(script);

            // Variable Holder
            switch (_type)
            {
                case TypesTool.Filter:
                    scriptV = thisObj.GetComponentInChildren<VariableHolder>();
                    scriptV.scriptTool = script;
                    scriptV.m_deposit = new sbyte[] {0, 0};
                    scriptV.m_info = new byte[] {0, 2};
                    break;
                case TypesTool.Pitcher:
                    scriptV = thisObj.GetComponentInChildren<VariableHolder>();
                    scriptV.scriptTool = script;
                    scriptV.m_deposit = new sbyte[] {0, 0};
                    scriptV.m_info = new byte[] {0};
                    break;
                default:
                    script.m_variables = null;
                    break;
            }

            // Pickup
            switch (_type)
            {
                case TypesTool.KnockBox:
                    script.m_pickup = null;
                    break;
                default:
                    script.m_pickup = (VRC_Pickup)thisObj.GetComponentInChildren(typeof(VRC_Pickup));
                    break;
            }

            // Animator
            switch (_type)
            {
                case TypesTool.Ingredient:
                case TypesTool.Filter:
                    if (!script.m_animator) script.m_animator = Selection.activeGameObject.GetComponentInChildren<Animator>();
                    break;
                default:
                    script.m_animator = null;
                    break;
            }

            // Particle
            switch (_type)
            {
                case TypesTool.Squeezer:
                case TypesTool.Ingredient:
                case TypesTool.Pitcher:
                    Array.Resize<ParticleSystem>(ref script.m_particle, 2);
                    break;
                default: 
                    script.m_particle = null;
                    break;
            }

            // Axis
            switch (_type)
            {
                case TypesTool.Ingredient: break;
                case TypesTool.Pitcher:
                    Array.Resize<Transform>(ref script.m_axis, 2);
                    break;
                default:
                    script.m_axis = null;
                    break;
            }

            // Audio
            if (_type == TypesTool.NULL) script.m_audio = null;

            // Ingredient Variable
            if (_type != TypesTool.Ingredient)
            {
                script.isCap = false;
                script.isAnim = false;
                script.isTilt = false;
                script.isTrig = false;
                script.isHold = false;
                script.lengthAnim = 0;
                script.eventAnim = null;
            }

            serializedObject.Update();
        }

        public override void OnInspectorGUI()
        {
            if (_type != script.m_type) Initialize();

        #region Skin

            int tempIndexSkin = indexSkin;
            switch (_type)
            {
                case TypesTool.KnockBox:
                case TypesTool.Filter:
                case TypesTool.Tamper:
                    EditorGUI.BeginChangeCheck();
                    indexSkin = (int)(SkinsMachine)EditorGUILayout.EnumPopup("Skin Select", (SkinsMachine)indexSkin);
                    if ( EditorGUI.EndChangeCheck() )
                    {
                        string tempStr = SkinManager.ChangeTool(script, indexSkin);
                        if (tempStr == null) return;
                        else
                        {
                            indexSkin = tempIndexSkin;
                            Debug.LogWarning(tempStr);
                        }
                    }
                    break;
                default:
                    GUILayout.Label("Skin Select unavailable");
                    break;
            }

        #endregion

            GUILayout.Space(20);
            toggleVariable = EditorGUILayout.Foldout(toggleVariable, "Do not change any variables");
            if (!toggleVariable) return;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_type"), new GUIContent("Tool Type"));
            serializedObject.ApplyModifiedProperties();

            if (_type != script.m_type) Initialize();

            switch (_type)
            {
                case TypesTool.Squeezer:
                    GUILayout.Label("Particle");
                    EditorGUILayout.BeginVertical(paddingLeft);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_particle").GetArrayElementAtIndex(0), new GUIContent("Lemon"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_particle").GetArrayElementAtIndex(1), new GUIContent("GrapeFruit"));
                    EditorGUILayout.EndVertical();
                    break;
                case TypesTool.Ingredient:
                    script.isCap = EditorGUILayout.Toggle("Cap", script.isCap);

                    script.isAnim = EditorGUILayout.Toggle("Animated", script.isAnim);
                    if (script.isAnim)
                    {
                        EditorGUILayout.BeginVertical(paddingLeft);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_animator"), new GUIContent("Animator"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("lengthAnim"), new GUIContent("Length"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("eventAnim"), new GUIContent("Event Name"));
                        EditorGUILayout.EndVertical();
                        serializedObject.ApplyModifiedProperties();
                        GUILayout.Space(10);
                    }
                    else
                    {
                        script.m_animator = null;
                        script.lengthAnim = 0;
                        script.eventAnim = null;
                    }

                    script.isTrig = EditorGUILayout.Toggle("Need to Click", script.isTrig);
                    if (script.isTrig)
                    {
                        EditorGUILayout.BeginVertical(paddingLeft);
                        script.isHold = EditorGUILayout.Toggle("Need to Hold", script.isHold);
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(10);
                    }

                    script.isTilt = EditorGUILayout.Toggle("Need to Tilt", script.isTilt);
                    if (script.isTilt)
                    {
                        Array.Resize<Transform>(ref script.m_axis, 2);
                        serializedObject.Update();
                        GUILayout.Label("Axis");
                        EditorGUILayout.BeginVertical(paddingLeft);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_axis").GetArrayElementAtIndex(0), new GUIContent("Out"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_axis").GetArrayElementAtIndex(1), new GUIContent("Base"));
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(10);
                    }
                    else script.m_axis = null;

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_particle").GetArrayElementAtIndex(0), new GUIContent("Particle"));
                    break;
                case TypesTool.Pitcher:
                    // GUILayout.Space(10);
                    GUILayout.Label("Axis");
                    EditorGUILayout.BeginVertical(paddingLeft);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_axis").GetArrayElementAtIndex(0), new GUIContent("Out"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_axis").GetArrayElementAtIndex(1), new GUIContent("Base"));
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(10);
                    GUILayout.Label("Particle");
                    EditorGUILayout.BeginVertical(paddingLeft);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_particle").GetArrayElementAtIndex(0), new GUIContent("Latte"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_particle").GetArrayElementAtIndex(1), new GUIContent("Cappuccino"));
                    EditorGUILayout.EndVertical();
                    break;
            }

            if (_type != TypesTool.NULL) EditorGUILayout.PropertyField(serializedObject.FindProperty("m_audio"), new GUIContent("Audio"));

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            toggleBase = EditorGUILayout.BeginFoldoutHeaderGroup(toggleBase, "Udon Variables");
            if (toggleBase) {
                UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target);
                base.OnInspectorGUI();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
    #endif
}