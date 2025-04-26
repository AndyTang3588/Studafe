
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System;
using UnityEditor;
// using UdonSharpEditor;
#endif

namespace VRCCoffeeSet
{
    [AddComponentMenu("MiniGreen/Coffee Set/Machine Contacter")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Machine_Component : UdonSharpBehaviour
    {
        MainController m_controller { get { return GetComponentInParent<MainController>(); } }
        Machine m_machine { get { return GetComponentInParent<Machine>(); } }
        public Machine_Display m_display { get { return m_machine.m_display; } }
        
        public TypesMachineC m_type;

        /// <summary>
        /// E_DP : 0_Mode, 1_Steam
        /// </summary>
        public Machine_Button[] m_button;

        /// <summary>
        /// E_DP : 0_Steam, 1_Froth
        /// </summary>
        public AudioSource[] m_audio;

        /// <summary>
        /// E_DF : 0_L, 1_R <para></para>
        /// E_DP : 0_Steam, 1_Froth
        /// </summary>
        public ParticleSystem[] m_particle;

        /// <summary>
        /// E_DF : 0_L, 1_R
        /// </summary>
        public Machine_Component[] m_deposit;
        
        /// <summary>
        /// -1 only for E_DF when no filter
        /// </summary>
        [UdonSynced] public sbyte m_indexTarget;

        /// <summary> 
        /// G_D : 0_Manual, 1_Auto
        /// E_DC, E_DF : 0~_Index among machines   <para></para>
        /// E_DP : 1_Latte, 2_Cappuccino     <para></para>
        /// E_W  : 0_Off, 1_On
        /// </summary>
        [UdonSynced] public byte m_mode = 0;
        [UdonSynced] bool m_isWorking = false;

        void Start()
        {
            for (int i = 0; i < m_button.Length; i++)
            {
                m_button[i].m_target = this;
                m_button[i].m_nameEvent = $"InputButton{i}";
            }

            switch (m_type)
            {
                case TypesMachineC.E_DF:
                    m_indexTarget = -1;
                    break;
                case TypesMachineC.E_DP:
                    m_indexTarget = -1;
                    m_mode = 1;
                    break;
            }

            LocalUpdate();
        }

        public override void OnDeserialization() => LocalUpdate();

        public void LocalUpdate()
        {
            switch (m_type)
            {
                case TypesMachineC.E_DF:
                    Tool udonFilter = m_indexTarget > -1 ? GetFilter(m_indexTarget) : null;

                    int nozzleCount = udonFilter ? udonFilter.m_variables.m_info[1] : 2;

                    // Set cup deposits
                    for (int i=0; i<nozzleCount; i++) m_deposit[i].gameObject.SetActive(udonFilter ? true : false);

                    // Set color of button
                    m_button[0].SetColor(udonFilter ? (m_isWorking ? 2 : 1) : 0);
                    m_button[0].SetInteract(udonFilter && !m_isWorking ? true : false);
                    break;
                case TypesMachineC.E_DP:
                    m_button[0].SetColor(m_indexTarget > -1 ? m_mode : 0);
                    m_button[0].SetInteract(m_indexTarget > -1 ? true : false);
                    break;
                case TypesMachineC.E_W:
                    if (m_mode == 1)
                    {
                        m_button[0].SetColor(2);
                        m_particle[0].Play();
                    }
                    else
                    {
                        m_button[0].SetColor(1);
                        m_particle[0].Stop();
                    }
                    break;
            }
        }

        public void DepositOff()
        {
            if (!Networking.IsOwner(gameObject) || m_indexTarget == -1) return;

            switch (m_type)
            {
                case TypesMachineC.E_DF:
                    Tool udonFilter = GetFilter(m_indexTarget);
                    if (m_isWorking) {
                        if (!Networking.IsOwner(udonFilter.gameObject)) Networking.SetOwner(Networking.LocalPlayer, udonFilter.gameObject);
                        udonFilter.m_variables.m_deposit[0] = 2;
                        udonFilter.m_variables.m_deposit[1] = (sbyte)m_machine.m_index;
                        udonFilter.SerializeVariables();
                        udonFilter.Drop();
                        udonFilter.transform.SetPositionAndRotation(transform.position, transform.rotation);
                    } else {
                        m_indexTarget = (sbyte)-1;
                        RequestSerialization();
                        LocalUpdate();
                    }
                    break;
                case TypesMachineC.E_DP:
                    Tool udonPitcher = GetPitcher(m_indexTarget);
                    if (m_isWorking) {
                        if (!Networking.IsOwner(udonPitcher.gameObject)) Networking.SetOwner(Networking.LocalPlayer, udonPitcher.gameObject);
                        udonPitcher.m_variables.m_deposit[0] = 1;
                        udonPitcher.m_variables.m_deposit[1] = (sbyte)m_machine.m_index;
                        udonPitcher.SerializeVariables();
                        udonPitcher.Drop();
                        udonPitcher.transform.SetPositionAndRotation(transform.position, transform.rotation);
                    } else {
                        m_indexTarget = (sbyte)-1;
                        RequestSerialization();
                        LocalUpdate();
                    }
                    break;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Check obj owner
            if (!Networking.IsOwner(other.gameObject)) return;

            switch (m_type)
            {
                case TypesMachineC.G_D: TrigEnter_Grinder(other);
                    break;
                case TypesMachineC.E_DF: TrigEnter_MachineE_DF(other);
                    break;
                case TypesMachineC.E_DC: TrigEnter_MachineE_DC(other);
                    break;
                case TypesMachineC.E_DP: TrigEnter_MachineE_DP(other);
                    break;
                case TypesMachineC.ICE: TrigEnter_MachineIce(other);
                    break;
            }
        }
        public void OnTriggerEnter_Delayed()
        {
            switch (m_type)
            {
                case TypesMachineC.E_DF:
                    Tool udonFilter = GetFilter(m_indexTarget);
                    udonFilter.SetActive(true);
                    udonFilter.SetPickup(true);
                    break;
            }
        }

        public void InputButton0()
        {
            switch (m_type)
            {
                case TypesMachineC.G_D: Input_Grinder();
                    break;
                case TypesMachineC.E_DF: Input_MachineE_DF();
                    break;
                case TypesMachineC.E_DP: Input_MachineE_DP1();
                    break;
                case TypesMachineC.E_W: Input_MachineW();
                    break;
            }
        }
        public void InputButton1()
        {
            switch (m_type)
            {
                case TypesMachineC.E_DP: Input_MachineE_DP2();
                    break;
            }
        }

    #region Grinder

        void TrigEnter_Grinder(Collider other)
        {
            // Check last obj is on deposit
            Tool udonFilter = GetFilter(m_indexTarget);
            if (udonFilter.m_variables.m_deposit[0] == 1 && udonFilter.m_variables.m_deposit[1] == m_machine.m_index) return;

            udonFilter = other.GetComponent<Tool>();

            // Check contact obj is matching with target
            if (!udonFilter) return;
            if (udonFilter.m_type != TypesTool.Filter) return;

            // Check is same skin
            if (m_machine.m_skinIndex != udonFilter.m_skinIndex) return;

            // Check pickup delay time and states of obj
            if (udonFilter.timeDelay > Time.time || udonFilter.m_variables.m_deposit[0] != 0 || udonFilter.m_variables.m_info[0] != 0) return;

            // Setup
            SetOwnerThis();
            m_indexTarget = (sbyte)udonFilter.m_index;
            RequestSerialization();

            // Filter
            udonFilter.m_variables.m_deposit[0] = 1;
            udonFilter.m_variables.m_deposit[1] = (sbyte)m_machine.m_index;
            udonFilter.SerializeVariables();
            udonFilter.Drop();
            udonFilter.transform.SetPositionAndRotation(transform.position, transform.rotation);
        }

        void Input_Grinder()
        {
            Tool udonFilter = GetFilter(m_indexTarget);
            if (udonFilter.m_variables.m_deposit[0] != 1) Debug.Log("Grinder : No filter");
            else if (udonFilter.m_variables.m_info[0] != 0) Debug.Log("Grinder : Need empty filter");
            else SendCustomNetworkEvent(NetworkEventTarget.All, "StartGrind");
        }

        public void StartGrind()
        {
            // Play Particle, Sound
            m_audio[0].Play();
            m_particle[0].Play();

            // Button
            m_button[0].SetInteract(false);

            // Filter
            Tool udonFilter = GetFilter(m_indexTarget);
            udonFilter.Drop();
            udonFilter.SetPickup(false);
            udonFilter.transform.SetPositionAndRotation(transform.position, transform.rotation);
            udonFilter.GetComponent<Animator>().SetTrigger("Grind");
            udonFilter.m_variables.m_info[0] = 1;
            udonFilter.SendCustomEventDelayedSeconds("LocalUpdate", m_audio[0].clip.length/2);

            SendCustomEventDelayedSeconds("StartGrind_Delayed", m_audio[0].clip.length);
        }
        public void StartGrind_Delayed()
        {
            Tool udonFilter = GetFilter(m_indexTarget);
            udonFilter.SetPickup(true);
            m_button[0].SetInteract(true);
        }

    #endregion
    
    #region Machine - Filter

        void TrigEnter_MachineE_DF(Collider other)
        {
            // Check last obj is on deposit
            Tool udonFilter;
            if (m_indexTarget > -1) 
            {
                udonFilter = GetFilter(m_indexTarget);
                if (udonFilter.m_variables.m_deposit[0] == 2 && udonFilter.m_variables.m_deposit[1] == m_machine.m_index) return;
            }

            udonFilter = other.GetComponent<Tool>();

            // Check contact obj is matching with target
            if (!udonFilter) return;
            if (udonFilter.m_type != TypesTool.Filter) return;

            // Check is same skin
            if (m_machine.m_skinIndex != udonFilter.m_skinIndex) return;

            // Check pickup delay time of obj
            if (udonFilter.timeDelay > Time.time) return;

            // Check states of obj
            if (udonFilter.m_variables.m_deposit[0] != 0 || udonFilter.m_variables.m_info[0] != 2)
            {
                m_display.SetText("Need Tamped" + "\n" + "Porta Filter", 5);
                return;
            }

            // Filter
            udonFilter.m_variables.m_deposit[0] = 2;
            udonFilter.m_variables.m_deposit[1] = (sbyte)m_machine.m_index;
            udonFilter.SerializeVariables();
            udonFilter.Drop();
            udonFilter.SetPickup(false);
            udonFilter.SetActive(false);
            udonFilter.transform.SetPositionAndRotation(transform.position, transform.rotation);

            // Setup
            SetOwnerThis();
            m_indexTarget = (sbyte)udonFilter.m_index;
            RequestSerialization();
            GetComponent<Animator>().SetTrigger("Deposit_" + udonFilter.m_variables.m_info[1]);
            // udon_Display.SetText("Filter Placed", 5);
            LocalUpdate();

            SendCustomEventDelayedSeconds("OnTriggerEnter_Delayed", 1);
        }

        void Input_MachineE_DF()
        {
            // Check filter deposit
            if (m_indexTarget == -1)
            {
                m_display.SetText("Need Filter", 5);
                return;
            }

            // Check states of filter
            Tool udonFilter = GetFilter(m_indexTarget);
            if (udonFilter.m_variables.m_deposit[0] != 2)
            {
                m_display.SetText("Need Filter", 5);
                return;
            }
            else if (udonFilter.m_variables.m_info[0] != 2)
            {
                m_display.SetText("Need to Change Filter", 5);
                return;
            }

            // Check cup deposits
            for (int i=0; i < udonFilter.m_variables.m_info[1]; i++)
            {
                Cup_Coffee udonCup = GetEspressoCup(m_deposit[i].m_indexTarget);
                // Not empty
                if (udonCup.m_variables.m_info[0] != 0)
                {
                    m_display.SetText("Need Empty"+"\n"+"Espresso Cup", 5);
                    return;
                }
                // Not deposited
                if (udonCup.m_variables.m_deposit[0] != 1 || udonCup.m_variables.m_deposit[1] != m_deposit[i].m_mode)
                { 
                    m_display.SetText("Need Espresso Cup", 5);
                    return;
                } 
            }

            // Initialize extracting
            SetOwnerThis();
            LocalUpdate();
            m_display.SetText("Extracting Espresso", m_audio[0].clip.length);
            SendCustomNetworkEvent(NetworkEventTarget.All,"StartExtract");
        }

        public void StartExtract() // Networked
        {
            // Machine
            m_isWorking = true;
            m_audio[0].Play();
            m_button[0].SetInteract(false);

            // Filter
            if ( !(m_indexTarget > -1) ) {
                Debug.LogError("Espresso machine: Error occured while extracting." +"\n"+ "There is no filter deposited.");
                return;
            }
            Tool udonFilter = GetFilter(m_indexTarget);
            if (udonFilter.m_variables.m_deposit[0] != 2) {
                udonFilter.m_variables.m_deposit[0] = 2;
                udonFilter.m_variables.m_deposit[1] = (sbyte)m_machine.m_index;
                LocalUpdate();
            }
            udonFilter.Drop();
            udonFilter.SetPickup(false);
            udonFilter.transform.SetPositionAndRotation(transform.position, transform.rotation);
            udonFilter.m_variables.m_info[0] = 3;
            udonFilter.LocalUpdate();
            
            // Cup(s)
            for (int i=0; i<udonFilter.m_variables.m_info[1]; i++)
            {
                m_particle[i].Play();
                Cup_Coffee udonCup = GetEspressoCup(m_deposit[i].m_indexTarget);
                udonCup.Drop();
                udonCup.SetPickup(false);
                udonCup.transform.SetPositionAndRotation(m_deposit[i].transform.position, m_deposit[i].transform.rotation);
                udonCup.m_variables.m_deposit[0] = 1;
                udonCup.m_variables.m_deposit[1] = (sbyte)m_deposit[i].m_mode;
                udonCup.m_contentSpeedPerDrink = (m_audio[0].clip.length - m_controller.m_extractDelay) / 5;
                udonCup.time_Delay = Time.time + m_controller.m_extractDelay;
                udonCup.m_variables.m_info[0] = 5;
            }

            SendCustomEventDelayedSeconds("StartExtract_Delay", m_controller.m_extractDelay);
            SendCustomEventDelayedSeconds("StartExtract_End", m_audio[0].clip.length);
        }
        public void StartExtract_Delay()
        {
            int nozzleCount = GetFilter(m_indexTarget).m_variables.m_info[1];
            for (int i=0; i<nozzleCount; i++)
            {
                Cup_Coffee udonCup = GetEspressoCup(m_deposit[i].m_indexTarget);
                udonCup.m_variables.m_info[1] = 12;
                udonCup.LocalUpdate();
            }
        }
        public void StartExtract_End()
        {
            m_button[0].SetInteract(true);
            Tool udonFilter = GetFilter(m_indexTarget);
            udonFilter.SetPickup(true);
            for (int i=0; i<udonFilter.m_variables.m_info[1]; i++)
            {
                Cup_Coffee udonCup = GetEspressoCup(m_deposit[i].m_indexTarget);
                udonCup.SetPickup(true);
                udonCup.m_contentSpeedPerDrink = 0.5f;
            }

            // Player who started extracting
            if (!Networking.IsOwner(gameObject)) return;
            m_isWorking = false;
            RequestSerialization();
            LocalUpdate();
        }

    #endregion

    #region Machine - Cup

        void TrigEnter_MachineE_DC(Collider other)
        {
            // Check last obj is on deposit
            Cup_Coffee udonCup = m_controller.dish_espressoCup[m_indexTarget];
            if (udonCup.m_variables.m_deposit[0] == 1 && udonCup.m_variables.m_deposit[1] == m_mode) return;

            udonCup = other.GetComponent<Cup_Coffee>();

            // Check contact obj is matching with target
            if (!udonCup) return;
            if (udonCup.m_type != TypesCup.Espresso) return;

            // Check pickup delay time of obj
            if (udonCup.time_Delay > Time.time) return;

            // Check states of obj
            if (udonCup.m_variables.m_deposit[0] != 0
                || udonCup.m_variables.m_info[0] != 0
                || udonCup.m_variables.m_info[3] != 0) return;
            
            // Setup
            SetOwnerThis();
            m_indexTarget = (sbyte)udonCup.m_index;
            RequestSerialization();

            // Cup
            udonCup.Drop();
            other.transform.SetPositionAndRotation(transform.position, transform.rotation);
            udonCup.m_variables.m_deposit[0] = 1;
            udonCup.m_variables.m_deposit[1] = (sbyte)m_mode;
            udonCup.SerializeVariables();
        }

    #endregion

    #region Machine - Pitcher

        void TrigEnter_MachineE_DP(Collider other)
        {
            // Check last obj is on deposit
            Tool udonPitcher;
            if (m_indexTarget > -1) 
            {
                udonPitcher = GetPitcher(m_indexTarget);
                if (udonPitcher.m_variables.m_deposit[0] == 1 && udonPitcher.m_variables.m_deposit[1] == m_machine.m_index) return;
            }

            udonPitcher = other.GetComponent<Tool>();

            // Check contact obj is matching with target
            if (!udonPitcher) return;
            if (udonPitcher.m_type != TypesTool.Pitcher) return;

            // Check pickup delay time of obj
            if (udonPitcher.timeDelay > Time.time) return;
            
            // Check states of obj
            if (udonPitcher.m_variables.m_deposit[0] != 0)
            {
                Debug.LogError("Espresso machine : Error occured while depositing pitcher. Deposit state of steam pitcher not matching.");
                return;
            }
            if (udonPitcher.m_variables.m_info[0] != 1)
            {
                m_display.SetText("Need Milk-filled" + "\n" + "Steam Pitcher", 5);
                return;
            }

            // Setup
            SetOwnerThis();
            m_indexTarget = (sbyte)udonPitcher.m_index;
            RequestSerialization();
            LocalUpdate();
            m_display.SetText("Steam Pitcher" + "\n" + "Placed", 5);

            // Steam Pitcher
            udonPitcher.m_variables.m_deposit[0] = 1;
            udonPitcher.m_variables.m_deposit[1] = (sbyte)m_machine.m_index;
            udonPitcher.Drop();
            udonPitcher.transform.SetPositionAndRotation(transform.position, transform.rotation);
            udonPitcher.SerializeVariables();
        }

        void Input_MachineE_DP1()
        {
            if (m_indexTarget == -1) return;

            Tool udonPitcher = GetPitcher(m_indexTarget);

            if (udonPitcher.m_variables.m_deposit[0] != 1) {
                ResyncPitcher(udonPitcher);
                return;
            }

            SetOwnerThis();
            m_mode = m_mode == (byte)1 ? (byte)2 : (byte)1;
            RequestSerialization();
            LocalUpdate();
            m_display.SetText("Froth Mode" +"\n"+ (m_mode == 1 ? "Latte" : "Cappuccino"), 5);
        }
        void Input_MachineE_DP2()
        {
            if (m_indexTarget == -1) {
                SendCustomNetworkEvent(NetworkEventTarget.All,"StartSteam");
                return;
            }

            Tool udonPitcher = GetPitcher(m_indexTarget);

            if (udonPitcher.m_variables.m_deposit[0] != 1) {
                ResyncPitcher(udonPitcher);
                return;
            }

            if (udonPitcher.m_variables.m_info[0] != 1) {
                m_display.SetText("Need to Change Jug", 5); // Aleart need to change jug
                return;
            }

            SetOwnerThis();
            m_isWorking = true;
            RequestSerialization();
            m_display.SetText("Frothing Milk...", m_audio[0].clip.length);

            SendCustomNetworkEvent(NetworkEventTarget.All,"StartFroth");
        }

        void ResyncPitcher(Tool udonPitcher)
        {
            SetOwnerThis();
            m_display.SetText("Sync Error" + "\n" + "Resyncing...", 5);
            m_indexTarget = (sbyte)-1;
            RequestSerialization();
            LocalUpdate();
            Debug.LogError(
                "Espresso machine : Error occured while depositing pitcher." +"\n"+ 
                "Syncing error between pitcher and this." +"\n"+ 
                "If you found this message, Please report through discord!"
            );
        }

        public void StartSteam() // Networked
        {
            m_audio[0].Play();
            m_particle[0].Play();
            m_button[1].SetInteract(false);
            m_button[1].SetWorking(true);
            
            SendCustomEventDelayedSeconds("StartSteam_Delayed", m_audio[0].clip.length - 0.1f);
        }
        public void StartSteam_Delayed()
        {
            m_button[1].SetInteract(true);
            m_button[1].SetWorking(false);
        }
        public void StartFroth() // Networked
        {
            // Machine
            m_audio[1].Play();
            m_particle[1].Play();
            m_button[0].SetInteract(false);
            m_button[1].SetInteract(false);
            m_button[1].SetWorking(true);

            // Check sync with pitcher
            if (m_indexTarget == -1) {
                Debug.LogError("MachineE_D_Pitcher: Error occured while Frothing, no indexPitcher");
                return;
            }

            //Pitcher
            Tool udonPitcher = GetPitcher(m_indexTarget);
            udonPitcher.Drop();
            udonPitcher.SetPickup(false);
            udonPitcher.transform.SetPositionAndRotation(transform.position, transform.rotation);
            udonPitcher.m_variables.m_info[0] = (byte)(m_mode + 1);
            udonPitcher.m_variables.m_deposit[0] = 1;
            udonPitcher.LocalUpdate();

            SendCustomEventDelayedSeconds("StartFroth_Delayed", m_audio[1].clip.length);
        }
        public void StartFroth_Delayed()
        {
            Tool udonPitcher = GetPitcher(m_indexTarget);
            m_button[0].SetInteract(true);
            m_button[1].SetInteract(true);
            m_button[1].SetWorking(false);
            udonPitcher.SetPickup(true);

            if (!Networking.IsOwner(gameObject)) return;
            m_isWorking = false;
            RequestSerialization();
        }

    #endregion

    #region Machine - Water

        void Input_MachineW()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            m_mode = m_mode == (byte)0 ? (byte)1 : (byte)0;
            RequestSerialization();
            LocalUpdate();
            m_display.SetText(m_mode == (byte)0 ? "Hot Water Off" : "Hot Water On", 5);
        }

    #endregion

    #region Machine - Ice

        void TrigEnter_MachineIce(Collider other)
        {
            if (m_isWorking) return;

            // Check is obj has valid script
            Cup_Coffee udonCup = other.GetComponent<Cup_Coffee>();
            if (!udonCup) return;
            if (udonCup.m_type != TypesCup.Glass || udonCup.m_variables.m_info[7] != 0) return;

            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "StartIce");
        }

        public void StartIce()
        {
            m_isWorking = true;
            m_particle[0].Play();
            m_audio[0].Play();
            SendCustomEventDelayedSeconds("StartIce_Delayed", m_audio[0].clip.length);
        }
        public void StartIce_Delayed() => m_isWorking = false;

    #endregion

    #region Function

        void SetOwnerThis() { if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject); }
        Tool GetFilter(int index) { return m_controller.tool_filter[index]; }
        Tool GetPitcher(int index) { return m_controller.tool_pitcher[index]; }
        Cup_Coffee GetEspressoCup(int index) { return m_controller.dish_espressoCup[index]; }

    #endregion
    }

    
    #if !COMPILER_UDONSHARP && UNITY_EDITOR
    [CustomEditor(typeof(Machine_Component))]
    public class Machine_ContacterEditor : Editor
    {
        Machine_Component script;
        TypesMachineC _type;

        bool toggle_Base;

        GUIStyle paddingLeft = new GUIStyle{};

        private void Awake()
        {
            script = target as Machine_Component;
            paddingLeft.padding.left = 15;
        }

        void Initialize()
        {
            _type = script.m_type;

            switch (script.m_type)
            {
                case TypesMachineC.G_D:
                    Array.Resize<Machine_Button>(ref script.m_button, 1);
                    Array.Resize<AudioSource>(ref script.m_audio, 1);
                    Array.Resize<ParticleSystem>(ref script.m_particle, 1);
                    script.m_deposit = null;
                    break;
                case TypesMachineC.E_DF:
                    Array.Resize<Machine_Button>(ref script.m_button, 1);
                    Array.Resize<AudioSource>(ref script.m_audio, 1);
                    Array.Resize<ParticleSystem>(ref script.m_particle, 2);
                    Array.Resize<Machine_Component>(ref script.m_deposit, 2);
                    break;
                case TypesMachineC.E_DC:
                    script.m_button = null;
                    script.m_audio = null;
                    Array.Resize<ParticleSystem>(ref script.m_particle, 1);
                    script.m_deposit = null;
                    break;
                case TypesMachineC.E_DP:
                    Array.Resize<Machine_Button>(ref script.m_button, 2);
                    Array.Resize<AudioSource>(ref script.m_audio, 2);
                    Array.Resize<ParticleSystem>(ref script.m_particle, 2);
                    script.m_deposit = null;
                    break;
                case TypesMachineC.E_W:
                    Array.Resize<Machine_Button>(ref script.m_button, 1);
                    script.m_audio = null;
                    Array.Resize<ParticleSystem>(ref script.m_particle, 1);
                    script.m_deposit = null;
                    break;
                case TypesMachineC.ICE:
                    script.m_button = null;
                    Array.Resize<AudioSource>(ref script.m_audio, 1);
                    Array.Resize<ParticleSystem>(ref script.m_particle, 1);
                    script.m_deposit = null;
                    break;
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_type"));
            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(10);

            if (_type != script.m_type) Initialize();

            switch (script.m_type)
            {
                case TypesMachineC.G_D:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_button").GetArrayElementAtIndex(0), new GUIContent("Button"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_audio").GetArrayElementAtIndex(0), new GUIContent("Audio"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_particle").GetArrayElementAtIndex(0), new GUIContent("Particle"));
                    break;
                case TypesMachineC.E_DF:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_button").GetArrayElementAtIndex(0), new GUIContent("Button"));
                    EditorGUILayout.Space();
                    GUILayout.Label("Cup Deposit");
                    EditorGUILayout.BeginVertical(paddingLeft);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_deposit").GetArrayElementAtIndex(0), new GUIContent("L"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_deposit").GetArrayElementAtIndex(1), new GUIContent("R"));
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_audio").GetArrayElementAtIndex(0), new GUIContent("Audio"));
                    EditorGUILayout.Space();
                    GUILayout.Label("Particle");
                    EditorGUILayout.BeginVertical(paddingLeft);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_particle").GetArrayElementAtIndex(0), new GUIContent("L"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_particle").GetArrayElementAtIndex(1), new GUIContent("R"));
                    EditorGUILayout.EndVertical();
                    break;
                case TypesMachineC.E_DC:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_particle").GetArrayElementAtIndex(0), new GUIContent("Particle Shadow"));
                    break;
                case TypesMachineC.E_DP:
                    GUILayout.Label("Button");
                    EditorGUILayout.BeginVertical(paddingLeft);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_button").GetArrayElementAtIndex(0), new GUIContent("Mode"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_button").GetArrayElementAtIndex(1), new GUIContent("Steam"));
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(10);
                    GUILayout.Label("Audio");
                    EditorGUILayout.BeginVertical(paddingLeft);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_audio").GetArrayElementAtIndex(0), new GUIContent("Steam"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_audio").GetArrayElementAtIndex(1), new GUIContent("Froth"));
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(10);
                    GUILayout.Label("Particle");
                    EditorGUILayout.BeginVertical(paddingLeft);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_particle").GetArrayElementAtIndex(0), new GUIContent("Steam"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_particle").GetArrayElementAtIndex(1), new GUIContent("Froth"));
                    EditorGUILayout.EndVertical();
                    break;
                case TypesMachineC.E_W:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_button").GetArrayElementAtIndex(0), new GUIContent("Button"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_particle").GetArrayElementAtIndex(0), new GUIContent("Particle"));
                    break;
                case TypesMachineC.ICE:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_particle").GetArrayElementAtIndex(0), new GUIContent("Particle"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_audio").GetArrayElementAtIndex(0), new GUIContent("Audio"));
                    break;
            }

            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(20);

            toggle_Base = EditorGUILayout.Foldout(toggle_Base, "Udon");
            if (toggle_Base)
            {
                // if ( UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target) ) return;
                base.OnInspectorGUI();
            }
        }
    }
    #endif
}