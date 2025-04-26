
using UdonSharp;
using UnityEngine;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering.PostProcessing;
using UnityEditor;
using UdonSharpEditor;
#endif

namespace VRCCoffeeSet
{
    public enum TypesMachine
    {
        NULL, Grinder, Espresso, Ice
    }

    public enum TypesMachineC
    {
       NULL, G_D, E_DF, E_DC, E_DP, E_W, ICE
    }

    public enum TypesTool
    {
        NULL, Filter, Pitcher, KnockBox, Tamper, Straw, Ingredient, Squeezer
    }

    public enum TypesCup
    {
        NULL, Coffee, Espresso, Glass
    }

    [AddComponentMenu("MiniGreen/Coffee Set/Main Controller")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class MainController : UdonSharpBehaviour
    {
        /// <summary> 0_Initialize, 1_Ade, 2_Tea, 3_Alcohol </summary>
        [HideInInspector] public bool[] m_init = new bool[4];

        public float m_pickupdelay = 0.5f;
        public float m_extractDelay = 5.5f;

        [Header("Machines")]
        public Machine_Component[] m_grinder;
        public Machine_Component[] m_machineF;
        public Machine_Component[] m_machineP;
        public Machine[] m_machineIce;

        [Header("Tools")]
        public Tool[] tool_tamper;
        public Tool[] tool_filter;
        public Tool[] tool_pitcher;
        public Tool[] tool_straw;
        public Tool[] tool_other;

        [Header("Dishes")]
        public Cup_Coffee[] dish_espressoCup;
        public Cup_Plate[] dish_espressoPlate;
        public Cup_Coffee[] dish_coffeeCup;
        public Cup_Plate[] dish_coffeePlate;
        public Cup_Coffee[] dish_Glass;

        [Header("Menu")]
        public GameObject[] menu_Ade;

        #region Material Variables
        
        [HideInInspector] public Material[] mat_Ring;
        [HideInInspector] public Material[] mat_Straw;

        [HideInInspector] public Material mat_Empty;
        [HideInInspector] public Material mat_Water;
        /// <summary> 0_Milk <para></para> Froth : 3_Milk, 2_Latte, 3_Cappuccino </summary>
        [HideInInspector] public Material[] mat_Milk;

        [HideInInspector] public Material[] mat_Top;
        [HideInInspector] public Material mat_Cream;

        /// <summary> 0_Chocolate <para></para> 1_Caramel </summary>
        [HideInInspector] public Material[] mat_Syrup;

        /// <summary> 0_Espresso <para></para> 1_ChocoMix <para></para> 2_CaramelMix </summary>
        [HideInInspector] public Material[] mat_Espresso;
        /// <summary> 0_Espresso <para></para> 1_ChocoMix </summary>
        [HideInInspector] public Material[] mat_EspressoIce;
        [HideInInspector] public Material mat_Americano;
        [HideInInspector] public Material mat_AmericanoIce;
        [HideInInspector] public Material[] mat_Latte;
        [HideInInspector] public Material mat_LatteIce;
        [HideInInspector] public Material[] mat_Cappuccino;
        [HideInInspector] public Material[] mat_Mocha;
        [HideInInspector] public Material[] mat_MochaArt;
        [HideInInspector] public Material mat_MochaIce;
        /// <summary> 0_CaramelMilk <para></para> 1_Macchiato </summary>
        [HideInInspector] public Material[] mat_Macchiato;
        [HideInInspector] public Material mat_MacchiatoIce;

        /// <summary> 0-1_Lemon, 2-3_Grape, 4-5_Grapefruit </summary>
        [HideInInspector] public Material[] mat_Ade;

        #endregion

        public AudioSource[] audios;
        uint m_soundMuteCount = 0;

        [HideInInspector] public Animator _PPDrunk;
        [HideInInspector, Range(0,100)] public float m_drunk = 0;

        

        private void Update()
        {
            #if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.F)) DrunkSet(m_drunk + 10);
            #endif
            DrunkDrain();
            CheckSoundVolume();
        }

        #region Reset Position

        public void Reset_Tool()
        {
            foreach (var item in tool_tamper) item.Call_Reset();
            foreach (var item in tool_filter) item.Call_Reset();
            foreach (var item in tool_pitcher) item.Call_Reset();
            foreach (var item in tool_straw) item.Call_Reset();
        }

        public void Reset_Object()
        {
            foreach (var item in tool_other) item.Call_Reset();
        }

        public void Reset_Cup()
        {
            foreach (var item in dish_coffeeCup) item.Call_Reset();
            foreach (var item in dish_espressoCup) item.Call_Reset();
            foreach (var item in dish_Glass) item.Call_Reset();
        }

        public void Reset_Plate()
        {
            foreach (var item in dish_coffeePlate) item.Call_Reset();
            foreach (var item in dish_espressoPlate) item.Call_Reset();
        }

        #endregion

        #region Drunk System

        void DrunkDrain()
        {
            if (!m_init[3]) return;
            if (m_drunk <= 0) return;
            DrunkSet(m_drunk -= Time.deltaTime);
        }
        public void DrunkSet(float value)
        {
            m_drunk = value;
            _PPDrunk.SetFloat("Value", MathRemap(m_drunk, 0, 100, 0, 1) );
        }

        #endregion

        #region Sound Proof

        public void SoundMute(bool value)
        {
            if (value) m_soundMuteCount++;
            else m_soundMuteCount--;
            Debug.Log(m_soundMuteCount);
        }
        void CheckSoundVolume()
        {
            if (audios == null) return;
            if (audios.Length == 0) return;
            if (m_soundMuteCount > 0 && audios[0].volume > 0)
            {
                float targetVolume = audios[0].volume - Time.deltaTime;
                if (targetVolume < 0) targetVolume = 0;
                foreach (AudioSource item in audios) item.volume = targetVolume;
            }
            else if (m_soundMuteCount == 0 && audios[0].volume < 1)
            {
                float targetVolume = audios[0].volume + Time.deltaTime;
                if (targetVolume > 1) targetVolume = 1;
                foreach (AudioSource item in audios) item.volume = targetVolume;
            }
        }

        #endregion

        #region Functions

        float MathRemap(float val, float inMin, float inMax, float outMin, float outMax) { return outMin + (val - inMin) * (outMax - outMin) / (inMax - inMin); }
        void AddArray_Tool(ref Tool[] arr1, Tool[] arr2)
        {
            Tool[] newArr = new Tool[arr1.Length + arr2.Length];
            for (int i = 0; i < arr1.Length; i++) newArr[i] = arr1[i];
            for (int i = arr1.Length; i < newArr.Length; i++) newArr[i] = arr2[i - arr1.Length];
            arr1 = newArr;
        }

        #endregion
    
        #region Editor

        [HideInInspector] public string PPLayerName = "Water";

        [HideInInspector, UnityEngine.Min(0)] public float m_soundRangeMax = 5;
        [HideInInspector] public bool gizmo_visualize;
        [HideInInspector] public Color gizmo_colorMax;
        
#if !COMPILER_UDONSHARP && UNITY_EDITOR

        [ExecuteInEditMode]
        public void OnDrawGizmosSelected()
        {
            if (gizmo_visualize)
            {
                Gizmos.color = gizmo_colorMax;
                Gizmos.DrawSphere(transform.position, m_soundRangeMax);
            }
        }
#endif

        #endregion
    }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
    [CustomEditor(typeof(MainController))]
    public class MainController_Editor : Editor
    {
        Transform transCtrl;
        Transform transSavedPos;

        public MainController controller;
        public string pathRoot;
        
        /// <summary> 0_Ade, 1_Tea, 2_Alcohol </summary>
        bool[] detectDLC = new bool[3];
        bool[] detectSkin = new bool[1];

        public bool detectPPLayer;

        bool fold_variable, fold_save, fold_soundRange;
        bool[] status_fold = new bool[5];

        bool isOnScene;
        GUIStyle styleInfo = new GUIStyle();
        GUIStyle styleInit = new GUIStyle();
        GUIStyle styleInitStatus = new GUIStyle();

        private void Awake()
        {
            styleInfo.fontSize = styleInit.fontSize = styleInitStatus.fontSize = 13;
            styleInfo.normal.textColor = styleInit.normal.textColor = Color.white;
            styleInfo.alignment = TextAnchor.UpperCenter;
            styleInit.alignment = TextAnchor.MiddleRight;
            styleInitStatus.alignment = TextAnchor.MiddleLeft;
            
            controller = target as MainController;
            isOnScene = controller.gameObject.scene.IsValid();
            if (!isOnScene) return;
            transCtrl = Selection.activeTransform;

            Array.Resize<bool>(ref controller.m_init, 4);

            GameObject tempObj = GameObject.Find("Coffee Set_Position");
            if (tempObj) transSavedPos =  tempObj.transform;
            
            pathRoot = AssetDatabase.GetAssetPath( PrefabUtility.GetCorrespondingObjectFromSource<MainController>(controller) ).Replace("/Coffee Set - MiniGreen.prefab", "");
            detectDLC[0] = AssetDatabase.IsValidFolder($"{pathRoot}/Assets_Ade");
            detectDLC[1] = AssetDatabase.IsValidFolder($"{pathRoot}/Assets_Tea");
            detectDLC[2] = AssetDatabase.IsValidFolder($"{pathRoot}/Assets_Alcohol");
            detectSkin[0] = AssetDatabase.IsValidFolder($"{pathRoot}/Assets_SkinWhiteWood");

            detectPPLayer = detectDLC[2] && Camera.main ? Camera.main.GetComponent<PostProcessLayer>() : false;

            UpdateAudios();
        }

        public override void OnInspectorGUI()
        {
            #region Information

            EditorGUILayout.Space(15);
            GUILayout.Label("! Do Not Unpack The Prefab !", styleInfo);
            EditorGUILayout.Space(15);
            GUILayout.Label(
                "Coffee Set by MiniGreen417" + "\n" +
                "Last Update : 2023-01-01" + "\n" +
                "ⓒ 2022. (MiniGreen417) all rights reserved."
                , styleInfo
            );
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Instruction Document", GUILayout.Width(200), GUILayout.Height(25)))
            {
                Application.OpenURL("https://docs.google.com/document/d/1qk2pvZLmfb3-kat5gqp-OG7Tz3IFpAKMIhr1lGNIt5U");
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (!isOnScene)
            {
                EditorGUILayout.HelpBox("Drag and drop onto hierarchy.", MessageType.Info);
                return;
            }
            else if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Can't edit while playing.", MessageType.Info);
                return;
            }

            #endregion

            #region Post-process Layer Name

            if (detectDLC[2] && detectPPLayer)
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                SerializedProperty p_PPlayerName = serializedObject.FindProperty("PPLayerName");
                EditorGUILayout.PropertyField(p_PPlayerName,
                    new GUIContent(
                        "Post-precess Layer Name", 
                        "Detected the post-process layer already exist." + "\n" + 
                        "Please write the name of layer what using for post-process layer."
                    )
                );
                if ( GUILayout.Button("Default", GUILayout.MaxWidth(60)) )
                {
                    p_PPlayerName.stringValue = "Water";
                    PrefabUtility.ApplyPropertyOverride(p_PPlayerName, $"{pathRoot}/Coffee Set - MiniGreen.prefab", InteractionMode.UserAction);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }

            #endregion

            #region Initialize

            if (GUILayout.Button("Initialize", GUILayout.Height(50))) 
            {
                for (int i = 0; i < controller.m_init.Length; i++) controller.m_init[i] = false;
                controller.audios = transCtrl.GetComponentsInChildren<AudioSource>();
                UpdateAudios();

                #region Machine

                Machine[] transMachine = transCtrl.GetComponentsInChildren<Machine>();
                List<Machine_Component> grinder = new List<Machine_Component>();
                List<Machine> machine = new List<Machine>();
                List<Machine_Component> machineF = new List<Machine_Component>();
                List<Machine_Component> machineP = new List<Machine_Component>();
                List<Machine> machineIce = new List<Machine>();

                int tempIndex = 0;
                for (int i = 0; i < transMachine.Length; i++)
                {
                    switch (transMachine[i].m_type)
                    {
                        case TypesMachine.Grinder:
                            transMachine[i].m_index = (byte)grinder.Count;
                            grinder.Add(transMachine[i].GetComponentInChildren<Machine_Component>());
                            break;
                        case TypesMachine.Espresso:
                            transMachine[i].m_index = (byte)machine.Count;
                            machine.Add(transMachine[i]);
                            Machine_Component[] deposit = transMachine[i].GetComponentsInChildren<Machine_Component>();
                            foreach (Machine_Component script in deposit)
                            {
                                if (script.m_type == TypesMachineC.E_DF) machineF.Add(script);
                                else if (script.m_type == TypesMachineC.E_DP) machineP.Add(script);
                                else if (script.m_type == TypesMachineC.E_DC)
                                {
                                    script.m_mode = (byte)tempIndex;
                                    tempIndex++;
                                }
                            }
                            break;
                        case TypesMachine.Ice:
                            machineIce.Add(transMachine[i]);
                            break;
                    }
                }
                controller.m_grinder = grinder.ToArray();
                controller.m_machineF = machineF.ToArray();
                controller.m_machineP = machineP.ToArray();
                controller.m_machineIce = machineIce.ToArray();

                #endregion

                #region Tool
                
                List<Tool> tools = controller.GetComponentsInChildren<Tool>().ToList();
                List<Tool> tampers = new List<Tool>();
                List<Tool> filters = new List<Tool>();
                List<Tool> pitchers = new List<Tool>();
                List<Tool> straws = new List<Tool>();
                List<Tool> others = new List<Tool>();
                foreach (Tool item in tools)
                {
                    switch (item.m_type)
                    {
                        case TypesTool.Tamper: tampers.Add(item);
                            break;
                        case TypesTool.Filter: filters.Add(item);
                            break;
                        case TypesTool.Pitcher: pitchers.Add(item);
                            break;
                        case TypesTool.Straw: straws.Add(item);
                            break;
                        case TypesTool.KnockBox: 
                            break;
                        default: others.Add(item);
                            break;
                    }
                }
                controller.tool_tamper = tampers.AssignIndex();
                controller.tool_filter = filters.AssignIndex();
                controller.tool_pitcher = pitchers.AssignIndex();
                controller.tool_straw = straws.AssignIndex();
                controller.tool_other = others.ToArray();
                
                #endregion

                #region Cup, Plate

                List<Cup_Coffee> cups = controller.GetComponentsInChildren<Cup_Coffee>().ToList();
                List<Cup_Plate> plates = controller.GetComponentsInChildren<Cup_Plate>().ToList();
                List<Cup_Coffee> cupEspresso = new List<Cup_Coffee>();
                List<Cup_Coffee> cupCoffee = new List<Cup_Coffee>();
                List<Cup_Coffee> cupGlass = new List<Cup_Coffee>();
                List<Cup_Plate> plateEspresso = new List<Cup_Plate>();
                List<Cup_Plate> plateCoffee = new List<Cup_Plate>();
                foreach (Cup_Coffee item in cups)
                {
                    switch (item.m_type)
                    {
                        case TypesCup.Espresso: cupEspresso.Add(item);
                            break;
                        case TypesCup.Coffee: cupCoffee.Add(item);
                            break;
                        case TypesCup.Glass: cupGlass.Add(item);
                            break;
                    }
                }
                foreach (Cup_Plate item in plates)
                {
                    switch (item.m_type)
                    {
                        case TypesCup.Espresso: plateEspresso.Add(item);
                            break;
                        case TypesCup.Coffee: plateCoffee.Add(item);
                            break;
                    }
                }
                controller.dish_espressoCup = cupEspresso.AssignIndex();
                controller.dish_coffeeCup = cupCoffee.AssignIndex();
                controller.dish_Glass = cupGlass.AssignIndex();
                controller.dish_espressoPlate = plateEspresso.AssignIndex();
                controller.dish_coffeePlate = plateCoffee.AssignIndex();

                #endregion

                #region DLC

                controller.mat_Ade = null;
                controller._PPDrunk = null;

                if (detectDLC[0]) Type.GetType("VRCCoffeeSet.Asset_Ade").GetMethod("Initialize").Invoke(null, new object[] {this} );
                if (detectDLC[1]) Type.GetType("VRCCoffeeSet.Asset_Tea").GetMethod("Initialize").Invoke(null, new object[] {this} );
                if (detectDLC[2]) Type.GetType("VRCCoffeeSet.Asset_Alcohol").GetMethod("Initialize").Invoke(null, new object[] {this} );

                // DLC Status on Menu
                controller.menu_Ade[0].SetActive(controller.m_init[1]); // EN
                controller.menu_Ade[1].SetActive(!controller.m_init[1]);
                controller.menu_Ade[2].SetActive(controller.m_init[1]); // KR
                controller.menu_Ade[3].SetActive(!controller.m_init[1]);
                controller.menu_Ade[4].SetActive(controller.m_init[1]); // JP
                controller.menu_Ade[5].SetActive(!controller.m_init[1]);

                #endregion

                // Finish
                Debug.Log(
                    $"Coffee set initialized successfully. || DLC : " +
                    (controller.m_init[1] ? "Ade" : "") +
                    (controller.m_init[2] ? ", Tea" : "") + 
                    (controller.m_init[3] ? ", Alcohol" : "")
                );

                controller.m_init[0] = true;
                PrefabUtility.ApplyPrefabInstance(Selection.activeGameObject, InteractionMode.UserAction);
            }

            #endregion

            #region Status Indicator

            GUILayout.BeginHorizontal();
            GUILayout.Label("Initialize : ", styleInit);
            styleInitStatus.normal.textColor = controller.m_init[0] ? Color.green : Color.red;
            GUILayout.Label(controller.m_init[0] ? "✓" : "✕", styleInitStatus);
            GUILayout.EndHorizontal();

            if (detectDLC[0]) DrawStatus("DLC - Ade", controller.m_init[1]);
            if (detectDLC[1]) DrawStatus("DLC - Tea", controller.m_init[2]);
            if (detectDLC[2]) DrawStatus("DLC - Alcohol", controller.m_init[3]);
            if (detectSkin[0]) DrawStatus("Skin - White Wood", detectSkin[0]);

            #endregion
            
            #region Position Save

            EditorGUILayout.Space(10);
            fold_save = EditorGUILayout.BeginFoldoutHeaderGroup(fold_save, "Save position of objects");
            if (fold_save)
            {
                EditorGUILayout.Space();
                if (!controller.m_init[0]) 
                {
                    GUILayout.Label("Initialize before saving or loading positions", styleInfo);
                    EditorGUILayout.Space();
                }

                GUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(!controller.m_init[0]);
                if (GUILayout.Button("Save", GUILayout.Height(30)))
                {
                    if (!transSavedPos)
                    {
                        transSavedPos = VRCCoffeeSet_Ext.CreateObject("Coffee Set_Position", controller.transform.position, controller.transform.eulerAngles, controller.transform.parent);
                        transSavedPos.tag = "EditorOnly";
                        VRCCoffeeSet_Ext.CreateObject(System.DateTime.Now.ToString(), Vector3.zero, Vector3.zero, transSavedPos);
                    }
                    else transSavedPos.transform.GetChild(0).name = System.DateTime.Now.ToString();

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
                }
                EditorGUI.BeginDisabledGroup(!transSavedPos);
                if (GUILayout.Button("Load", GUILayout.Height(30)))
                {
                    controller.transform.SetPositionAndRotation(transSavedPos.position, transSavedPos.rotation);

                    foreach (var item in controller.transform.GetComponentsInChildren<PositionSaveTarget>()) LoadPosition(item.transform);
                    foreach (var item in controller.transform.GetComponentsInChildren<Machine>()) LoadPosition(item.transform);
                    foreach (var item in controller.tool_tamper) 
                    {
                        LoadPosition(item.transform.parent);
                        LoadPosition(item.transform);
                    }
                    foreach (var item in controller.tool_filter) LoadPosition(item.transform);
                    foreach (var item in controller.tool_pitcher) LoadPosition(item.transform);
                    foreach (var item in controller.tool_straw) LoadPosition(item.transform);
                    foreach (var item in controller.tool_other) LoadPosition(item.transform);
                    foreach (var item in controller.dish_espressoCup) LoadPosition(item.transform);
                    foreach (var item in controller.dish_espressoPlate) LoadPosition(item.transform);
                    foreach (var item in controller.dish_coffeeCup) LoadPosition(item.transform);
                    foreach (var item in controller.dish_coffeePlate) LoadPosition(item.transform);
                    foreach (var item in controller.dish_Glass) LoadPosition(item.transform);

                    Debug.Log("Coffee Set : Position Loaded.");
                }
                EditorGUI.EndDisabledGroup();
                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();

                if (transSavedPos) GUILayout.Label("Last save : " + transSavedPos.transform.GetChild(0).name);

                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            #endregion

            #region Sound Settings

            fold_soundRange = EditorGUILayout.BeginFoldoutHeaderGroup(fold_soundRange, "Sound Settings");
            if (fold_soundRange)
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                controller.m_soundRangeMax = EditorGUILayout.FloatField(new GUIContent("Sound Range", "Default is 5"), controller.m_soundRangeMax);
                if (EditorGUI.EndChangeCheck()) UpdateAudios();
                if (GUILayout.Button("Default", GUILayout.MaxWidth(75)))
                {
                    controller.m_soundRangeMax = 5;
                    UpdateAudios();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                controller.gizmo_visualize = EditorGUILayout.Toggle("Visualize Sphere", controller.gizmo_visualize);
                controller.gizmo_colorMax = EditorGUILayout.ColorField(controller.gizmo_colorMax);
                if (EditorGUI.EndChangeCheck()) SceneView.RepaintAll();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
                if (GUILayout.Button("Soundproof Collider", GUILayout.Height(25)))
                {
                    EditorGUIUtility.PingObject( AssetDatabase.LoadAssetAtPath<UnityEngine.Object>($"{pathRoot}/Coffee Set - Soundproof.prefab") );
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            #endregion

            EditorGUILayout.Space(20);

            fold_variable = EditorGUILayout.Foldout(fold_variable, "Do not change any variables");
            if (!fold_variable) { serializedObject.ApplyModifiedProperties(); return; }

            #region Material

            status_fold[0] = EditorGUILayout.BeginFoldoutHeaderGroup(status_fold[0], "Materials");
            if (status_fold[0])
            {
                Material[] tempMat;

                if (controller.mat_Ring == null) controller.mat_Ring = new Material[0];
                tempMat = controller.mat_Ring;
                controller.mat_Ring = new Material[ EditorGUILayout.IntField("Mat_Ring", controller.mat_Ring.Length) ];
                for (int i = 0; i < controller.mat_Ring.Length; i++) {
                    if (i <= tempMat.Length-1) controller.mat_Ring[i] = tempMat[i];
                    controller.mat_Ring[i] = EditorGUILayout.ObjectField(i.ToString(), controller.mat_Ring[i], typeof(Material), true) as Material;
                }
                EditorGUILayout.Space(5);

                if (controller.mat_Straw == null) controller.mat_Straw = new Material[0];
                tempMat = controller.mat_Straw;
                controller.mat_Straw = new Material[ EditorGUILayout.IntField("Mat_Straw", controller.mat_Straw.Length) ];
                for (int i = 0; i < controller.mat_Straw.Length; i++) {
                    if (i <= tempMat.Length-1) controller.mat_Straw[i] = tempMat[i];
                    controller.mat_Straw[i] = EditorGUILayout.ObjectField(i.ToString(), controller.mat_Straw[i], typeof(Material), true) as Material;
                }
                EditorGUILayout.Space(5);

                GUILayout.Label("Top");
                if (controller.mat_Top == null) controller.mat_Top = new Material[0];
                tempMat = controller.mat_Top;
                controller.mat_Top = new Material[ EditorGUILayout.IntField("Mat_Top", controller.mat_Top.Length) ];
                for (int i = 0; i < controller.mat_Top.Length; i++) {
                    if (i <= tempMat.Length-1) controller.mat_Top[i] = tempMat[i];
                    controller.mat_Top[i] = EditorGUILayout.ObjectField(i.ToString(), controller.mat_Top[i], typeof(Material), true) as Material;
                }
                EditorGUILayout.Space(5);

                controller.mat_Cream = EditorGUILayout.ObjectField("Cream", controller.mat_Cream, typeof(Material), true) as Material;
            
                EditorGUILayout.Space(15);
                GUILayout.Label("Content");
                controller.mat_Empty = EditorGUILayout.ObjectField("Empty", controller.mat_Empty, typeof(Material), true) as Material;
                controller.mat_Water = EditorGUILayout.ObjectField("Water", controller.mat_Water, typeof(Material), true) as Material;
                if (controller.mat_Milk == null) controller.mat_Milk = new Material[4];
                else if (controller.mat_Milk.Length != 4) controller.mat_Milk = new Material[4];
                controller.mat_Milk[0] = EditorGUILayout.ObjectField("Milk", controller.mat_Milk[0], typeof(Material), true) as Material;
                controller.mat_Milk[1] = EditorGUILayout.ObjectField("_Pitcher Milk", controller.mat_Milk[1], typeof(Material), true) as Material;
                controller.mat_Milk[2] = EditorGUILayout.ObjectField("_ _Latte", controller.mat_Milk[2], typeof(Material), true) as Material;
                controller.mat_Milk[3] = EditorGUILayout.ObjectField("_ _Cappuccino", controller.mat_Milk[3], typeof(Material), true) as Material;
                
                EditorGUILayout.Space(15);
                GUILayout.Label("Coffee");

                if (controller.mat_Syrup == null) controller.mat_Syrup = new Material[0];
                tempMat = controller.mat_Syrup;
                controller.mat_Syrup = new Material[ EditorGUILayout.IntField("Syrup", controller.mat_Syrup.Length) ];
                for (int i = 0; i < controller.mat_Syrup.Length; i++) {
                    if (i <= tempMat.Length-1) controller.mat_Syrup[i] = tempMat[i];
                    controller.mat_Syrup[i] = EditorGUILayout.ObjectField(i.ToString(), controller.mat_Syrup[i], typeof(Material), true) as Material;
                }

                if (controller.mat_Espresso == null) controller.mat_Espresso = new Material[0];
                tempMat = controller.mat_Espresso;
                controller.mat_Espresso = new Material[ EditorGUILayout.IntField("Espresso", controller.mat_Espresso.Length) ];
                for (int i = 0; i < controller.mat_Espresso.Length; i++) {
                    if (i <= tempMat.Length-1) controller.mat_Espresso[i] = tempMat[i];
                    controller.mat_Espresso[i] = EditorGUILayout.ObjectField(i.ToString(), controller.mat_Espresso[i], typeof(Material), true) as Material;
                }
                EditorGUILayout.Space(5);

                if (controller.mat_EspressoIce == null) controller.mat_EspressoIce = new Material[0];
                tempMat = controller.mat_EspressoIce;
                controller.mat_EspressoIce = new Material[ EditorGUILayout.IntField("Espresso Ice", controller.mat_EspressoIce.Length) ];
                for (int i = 0; i < controller.mat_EspressoIce.Length; i++) {
                    if (i <= tempMat.Length-1) controller.mat_EspressoIce[i] = tempMat[i];
                    controller.mat_EspressoIce[i] = EditorGUILayout.ObjectField(i.ToString(), controller.mat_EspressoIce[i], typeof(Material), true) as Material;
                }
                EditorGUILayout.Space(5);

                controller.mat_Americano = EditorGUILayout.ObjectField("Americano", controller.mat_Americano, typeof(Material), true) as Material;
                controller.mat_AmericanoIce = EditorGUILayout.ObjectField("Americano Ice", controller.mat_AmericanoIce, typeof(Material), true) as Material;

                if (controller.mat_Latte == null) controller.mat_Latte = new Material[0];
                tempMat = controller.mat_Latte;
                controller.mat_Latte = new Material[ EditorGUILayout.IntField("Latte", controller.mat_Latte.Length) ];
                for (int i = 0; i < controller.mat_Latte.Length; i++) {
                    if (i <= tempMat.Length-1) controller.mat_Latte[i] = tempMat[i];
                    controller.mat_Latte[i] = EditorGUILayout.ObjectField(i.ToString(), controller.mat_Latte[i], typeof(Material), true) as Material;
                }
                EditorGUILayout.Space(5);

                controller.mat_LatteIce = EditorGUILayout.ObjectField("Latte Ice", controller.mat_LatteIce, typeof(Material), true) as Material;

                if (controller.mat_Cappuccino == null) controller.mat_Cappuccino = new Material[0];
                tempMat = controller.mat_Cappuccino;
                controller.mat_Cappuccino = new Material[ EditorGUILayout.IntField("Cappuccino", controller.mat_Cappuccino.Length) ];
                for (int i = 0; i < controller.mat_Cappuccino.Length; i++) {
                    if (i <= tempMat.Length-1) controller.mat_Cappuccino[i] = tempMat[i];
                    controller.mat_Cappuccino[i] = EditorGUILayout.ObjectField(i.ToString(), controller.mat_Cappuccino[i], typeof(Material), true) as Material;
                }
                EditorGUILayout.Space(5);

                if (controller.mat_Mocha == null) controller.mat_Mocha = new Material[0];
                tempMat = controller.mat_Mocha;
                controller.mat_Mocha = new Material[ EditorGUILayout.IntField("Mocha", controller.mat_Mocha.Length) ];
                for (int i = 0; i < controller.mat_Mocha.Length; i++) {
                    if (i <= tempMat.Length-1) controller.mat_Mocha[i] = tempMat[i];
                    controller.mat_Mocha[i] = EditorGUILayout.ObjectField(i.ToString(), controller.mat_Mocha[i], typeof(Material), true) as Material;
                }
                EditorGUILayout.Space(5);

                if (controller.mat_MochaArt == null) controller.mat_MochaArt = new Material[0];
                tempMat = controller.mat_MochaArt;
                controller.mat_MochaArt = new Material[ EditorGUILayout.IntField("Mocha Art", controller.mat_MochaArt.Length) ];
                for (int i = 0; i < controller.mat_MochaArt.Length; i++) {
                    if (i <= tempMat.Length-1) controller.mat_MochaArt[i] = tempMat[i];
                    controller.mat_MochaArt[i] = EditorGUILayout.ObjectField(i.ToString(), controller.mat_MochaArt[i], typeof(Material), true) as Material;
                }
                EditorGUILayout.Space(5);

                controller.mat_MochaIce = EditorGUILayout.ObjectField("Mocha Ice", controller.mat_MochaIce, typeof(Material), true) as Material;

                if (controller.mat_Macchiato == null) controller.mat_Macchiato = new Material[0];
                tempMat = controller.mat_Macchiato;
                controller.mat_Macchiato = new Material[ EditorGUILayout.IntField("Macchiato", controller.mat_Macchiato.Length) ];
                for (int i = 0; i < controller.mat_Macchiato.Length; i++) {
                    if (i <= tempMat.Length-1) controller.mat_Macchiato[i] = tempMat[i];
                    controller.mat_Macchiato[i] = EditorGUILayout.ObjectField(i.ToString(), controller.mat_Macchiato[i], typeof(Material), true) as Material;
                }
                EditorGUILayout.Space(5);

                controller.mat_MacchiatoIce = EditorGUILayout.ObjectField("Macchiato Ice", controller.mat_MacchiatoIce, typeof(Material), true) as Material;
                
                EditorGUILayout.Space(15);
                if (controller.m_init[1]) {
                    GUILayout.Label("Ade");
                    for (int i = 0; i < controller.mat_Ade.Length; i++) 
                        controller.mat_Ade[i] = EditorGUILayout.ObjectField(i.ToString(), controller.mat_Ade[i], typeof(Material), true) as Material;
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            #endregion

            #region Udon

            status_fold[1] = EditorGUILayout.BeginFoldoutHeaderGroup(status_fold[1], "Udon");
            if (status_fold[1]) {
                if ( UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target) ) return;
                base.OnInspectorGUI();
                GUILayout.Space(20);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            #endregion

            #region Debug

            status_fold[2] = EditorGUILayout.BeginFoldoutHeaderGroup(status_fold[2], "Debug");
            if (status_fold[2])
            {
                GUILayout.Space(10);
                if (GUILayout.Button("Reset"))
                {
                    ResetUdonvariables();

                    // DLC Status on Menu
                    controller.menu_Ade[0].SetActive(false); // EN
                    controller.menu_Ade[1].SetActive(true);
                    controller.menu_Ade[2].SetActive(false); // KR
                    controller.menu_Ade[3].SetActive(true);
                    controller.menu_Ade[4].SetActive(false); // JP
                    controller.menu_Ade[5].SetActive(true);

                    PrefabUtility.ApplyPrefabInstance(Selection.activeGameObject, InteractionMode.UserAction);

                    // Load prefab for remove objects
                    string pathPrefab = $"{pathRoot}/Coffee Set - MiniGreen.prefab";
                    GameObject objPrefab = PrefabUtility.LoadPrefabContents(pathPrefab);

                    // Machines
                    Machine[] transMachine = objPrefab.GetComponentsInChildren<Machine>();
                    List<Machine> grinder = new List<Machine>();
                    List<Machine> machine = new List<Machine>();
                    List<Machine> machineIce = new List<Machine>();
                    foreach (Machine script in transMachine)
                        if (script.m_type == TypesMachine.Grinder) grinder.Add(script);
                        else if (script.m_type == TypesMachine.Espresso) machine.Add(script);
                        else if (script.m_type == TypesMachine.Ice) machineIce.Add(script);
                        else DestroyImmediate(script.gameObject);
                    RemoveMachineDup(grinder);
                    RemoveMachineDup(machine);
                    RemoveMachineDup(machineIce);

                    // Cups
                    List<Cup_Coffee> cups = objPrefab.GetComponentsInChildren<Cup_Coffee>().ToList();

                    // DLC
                    if (detectDLC[0]) Type.GetType("VRCCoffeeSet.Asset_Ade").GetMethod("Reset").Invoke(null, new object[] {this, objPrefab, cups} );
                    if (detectDLC[1]) Type.GetType("VRCCoffeeSet.Asset_Tea").GetMethod("Reset").Invoke(null, new object[] {this, objPrefab} );
                    if (detectDLC[2]) Type.GetType("VRCCoffeeSet.Asset_Alcohol").GetMethod("Reset").Invoke(null, new object[] {this, objPrefab} );
                    
                    PrefabUtility.SaveAsPrefabAsset(objPrefab, pathPrefab);
                    PrefabUtility.UnloadPrefabContents(objPrefab);

                    AssetDatabase.Refresh();
                    Debug.Log("Coffee set has been resetted.");
                }
                if (GUILayout.Button("Find Audiosources"))
                {
                    controller.audios = transCtrl.GetComponentsInChildren<AudioSource>();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            #endregion
        
            serializedObject.ApplyModifiedProperties();
        }

        void ResetUdonvariables()
        {
            controller.PPLayerName = "Water";
            controller.m_init = new bool[4];
            controller.mat_Ade = null;
            controller.m_grinder = null;
            controller.m_machineF = null;
            controller.m_machineP = null;
            controller.m_machineIce = null;
            controller.tool_tamper = null;
            controller.tool_filter = null;
            controller.tool_pitcher = null;
            controller.tool_straw = null;
            controller.tool_other = null;
            controller.dish_espressoCup = null;
            controller.dish_espressoPlate = null;
            controller.dish_coffeeCup = null;
            controller.dish_coffeePlate = null;
            controller.dish_Glass = null;
        }
        void UpdateAudios()
        {
            if (!transCtrl) return;
            foreach(AudioSource item in transCtrl.GetComponentsInChildren<AudioSource>())
            {
                item.minDistance = 0;
                item.maxDistance = controller.m_soundRangeMax;
            }
        }

        void DrawStatus(string name, bool value)
        {
            styleInitStatus.normal.textColor = value ? Color.green : Color.red;
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{name} : ", styleInit);
            GUILayout.Label(value ? "✓" : "✕", styleInitStatus);
            GUILayout.EndHorizontal();
        }

        public void AssignMaterial(Transform trans, Material mat) { trans.GetComponent<Renderer>().material = mat; }

        public void AddToolInList(Tool[] tools)
        {
            List<Tool> listTool = controller.tool_other.ToList<Tool>();
            foreach (var item in tools) listTool.Add(item);
            controller.tool_other = listTool.ToArray();
        }

        void SavePosition(Transform obj)
        {
            Transform save = transSavedPos.Find(obj.name);
            if (!save) save = VRCCoffeeSet_Ext.CreateObject(obj.name, obj.position, obj.eulerAngles, transSavedPos);
            else save.SetPositionAndRotation(obj.position, obj.rotation);
        }
        void LoadPosition(Transform obj)
        {
            Transform save = transSavedPos.Find(obj.name);
            if (save) obj.SetPositionAndRotation(save.position, save.rotation);
        }

        void RemoveMachineDup(List<Machine> machines)
        {
            if (machines != null)
                for (int i = 1; i < machines.Count; i++) DestroyImmediate(machines[i].gameObject);
            machines = null;
        }
    }
    static class VRCCoffeeSet_Ext
    {
        static public Tool[] AssignIndex(this List<Tool> list)
        {
            for (int i = 0; i < list.Count; i++) list[i].m_index = i;
            return list.ToArray();
        }
        static public Cup_Coffee[] AssignIndex(this List<Cup_Coffee> list)
        {
            for (int i = 0; i < list.Count; i++) list[i].m_index = i;
            return list.ToArray();
        }
        static public Cup_Plate[] AssignIndex(this List<Cup_Plate> list)
        {
            for (int i = 0; i < list.Count; i++) list[i].m_index = i;
            return list.ToArray();
        }
        static public Transform CreateObject(string _name, Vector3 _pos, Vector3 _rot, Transform _parent = null)
        {
            Transform tempTrans = new GameObject().transform;
            tempTrans.name = _name;
            if (_parent) tempTrans.SetParent(_parent);
            tempTrans.position = _pos;
            tempTrans.eulerAngles = _rot;
            return tempTrans;
        }
    }
#endif
}