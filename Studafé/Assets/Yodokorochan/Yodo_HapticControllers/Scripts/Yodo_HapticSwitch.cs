using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Yodokorochan
{
    public enum HapticSwitchMode
    {
        [InspectorName("本地")] Local,
        [InspectorName("本地 & 保存")] LocalAndPersistence,
        [InspectorName("全局")] Global,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Yodo_HapticSwitch : UdonSharpBehaviour
    {
        [UdonSynced]
        [HideInInspector]
        public bool currentStatus = false;

        [Header("震动强度")]
        [Range(0.0f, 1.0f)] public float Yodo_HapticStrength = 1.0f;

        [Header("震动持续时间 [秒]")]
        [Range(0.0f, 1.0f)] public float Yodo_HapticDuration = 0.2f;

        [Header("表示 OFF 状态的物体")]
        public GameObject Yodo_SwitchOffObject = null;

        [Header("表示 ON 状态的物体")]
        public GameObject Yodo_SwitchOnObject = null;

        [Header("需要 Toggle 的物体")]
        public GameObject[] Yodo_ToggleTargetObject = null;

        [Header("开关的初始状态")]
        public bool Yodo_DefaultStatus = false;

        [Header("运行模式")]
        [SerializeField]
        private HapticSwitchMode Yodo_SwitchMode = HapticSwitchMode.Local;

        [Header("在 VR 中也启用普通的 Interact")]
        [SerializeField]
        private bool Yodo_InteractiveInVR = false;

        [Header("外部 Udon 联动（进阶用）")]
        public GameObject[] Yodo_SendCustomEventTarget;

        [Header("外部方法（复制用）")]
        [SerializeField][TextArea]
        public string CustomEventNames =
            "public void Yodo_HapticSwitchTriggered(){}\n" +
            "public void Yodo_HapticSwitchOn(){}\n" +
            "public void Yodo_HapticSwitchOff(){}";

        private bool[] _defaultStatuses;
        private VRCPlayerApi _localPlayer;
        private AudioSource _audio;

        // 震动振幅系数（因为范围不是 0~1，所以进行修正，将来可能因 VRC 更新而变化）
        private const float vib_amplitude_coefficient = 0.0636f;
        private HapticSwitchPersistenceManager _persistenceManager = null;

        void Start()
        {
            currentStatus = Yodo_DefaultStatus;
            _defaultStatuses = new bool[Yodo_ToggleTargetObject.Length];
            for(int cur = 0; cur < Yodo_ToggleTargetObject.Length;cur++)
            {
                if(Yodo_ToggleTargetObject[cur] != null)
                {
                    _defaultStatuses[cur] = Yodo_ToggleTargetObject[cur].activeSelf;
                }
            }
            _audio = GetComponent<AudioSource>();
            _localPlayer = Networking.LocalPlayer;
            UpdateObjects();
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (player == null) { return; }
            if (!player.IsValid()) { return; }

            if(player.isLocal)
            {
                if(player.IsUserInVR())
                {
                    if(!Yodo_InteractiveInVR)
                    {
                        this.DisableInteractive = true;
                    }
                }
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other == null) { return; }
            UdonBehaviour ub = (UdonBehaviour)other.GetComponent(typeof(UdonBehaviour));
            if (ub == null) { return; }

            if (ub.GetProgramVariableType("Yodo_isHapticCollider") == typeof(bool))
            {
                if ((bool)ub.GetProgramVariable("Yodo_isHapticCollider"))
                {
                    // スイッチをToggle(デスクトップモードと共通)
                    ToggleSwitch();

                    // 振動フィードバック
                    if ((bool)ub.GetProgramVariable("Yodo_VibrateLeftHand"))
                    {
                        _localPlayer.PlayHapticEventInHand(VRC_Pickup.PickupHand.Left, Yodo_HapticDuration, Yodo_HapticStrength * vib_amplitude_coefficient, 320.0f);
                    }
                    if ((bool)ub.GetProgramVariable("Yodo_VibrateRightHand"))
                    {
                        _localPlayer.PlayHapticEventInHand(VRC_Pickup.PickupHand.Right, Yodo_HapticDuration, Yodo_HapticStrength * vib_amplitude_coefficient, 320.0f);
                    }
                }
            }
        }

        public override void Interact()
        {
            ToggleSwitch();
        }

        public override void OnDeserialization()
        {
            if (Yodo_SwitchMode != HapticSwitchMode.Global) { return; }
            UpdateObjects();
        }

        public void ToggleSwitch()
        {
            switch (Yodo_SwitchMode)
            {
                case HapticSwitchMode.Local:
                    currentStatus = !currentStatus;
                    break;
                case HapticSwitchMode.LocalAndPersistence:
                    currentStatus = !currentStatus;
                    if (_persistenceManager != null)
                    {
                        _persistenceManager.SetSavedState(currentStatus);
                    }
                    break;
                case HapticSwitchMode.Global:
                    Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
                    currentStatus = !currentStatus;
                    RequestSerialization();
                    break;
            }
            UpdateObjects();

            // 効果音があれば再生
            if (_audio != null)
            {
                _audio.Play();
            }
            UpdateExtenalUdon();
        }

        private void UpdateObjects() //オブジェクトの状態を設定
        {
            // オブジェクトのActiveを設定
            for (int cur = 0; cur < Yodo_ToggleTargetObject.Length; cur++)
            {
                GameObject go = Yodo_ToggleTargetObject[cur];
                if (go != null)
                {
                    go.SetActive((Yodo_DefaultStatus ^ currentStatus) ^ _defaultStatuses[cur]);
                }
            }
            if (Yodo_SwitchOffObject != null)
            {
                Yodo_SwitchOffObject.SetActive(!currentStatus);
            }
            if (Yodo_SwitchOnObject != null)
            {
                Yodo_SwitchOnObject.SetActive(currentStatus);
            }
        }

        private void UpdateExtenalUdon()
        {
            // 外部Udon連携
            if (Yodo_SendCustomEventTarget != null)
            {
                foreach (GameObject go in Yodo_SendCustomEventTarget)
                {
                    if (go == null) { continue; }
                    UdonBehaviour ub = (UdonBehaviour)go.GetComponent(typeof(UdonBehaviour));
                    if (ub == null) { continue; }
                    if (Yodo_SwitchMode == HapticSwitchMode.Global)
                    {
                        ub.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Yodo_HapticSwitchTriggered");
                        if (currentStatus)
                        {
                            ub.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Yodo_HapticSwitchOn");
                        }
                        else
                        {
                            ub.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Yodo_HapticSwitchOff");
                        }
                    }
                    else
                    {
                        ub.SendCustomEvent("Yodo_HapticSwitchTriggered");
                        if (currentStatus)
                        {
                            ub.SendCustomEvent("Yodo_HapticSwitchOn");
                        }
                        else
                        {
                            ub.SendCustomEvent("Yodo_HapticSwitchOff");
                        }
                    }
                }
            }
        }

#region Persistence
        public void SetStateWithoutNotify(bool newState)
        {
            if (Yodo_SwitchMode == HapticSwitchMode.LocalAndPersistence)
            {
                currentStatus = newState;
                UpdateObjects();
                UpdateExtenalUdon();
            }
        }

        public void InitPersistence(HapticSwitchPersistenceManager manager)
        {
            _persistenceManager = manager;
        }
#endregion
    }
}
