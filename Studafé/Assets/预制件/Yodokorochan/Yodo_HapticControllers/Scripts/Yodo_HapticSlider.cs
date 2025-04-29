
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using VRC.SDKBase;
using VRC.Udon;

namespace Yodokorochan
{
    public enum HapticSliderMode
    {
        [InspectorName("本地")] Local,
        [InspectorName("本地＆保存")] LocalAndPersistence,
        [InspectorName("全局")] Global,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Yodo_HapticSlider : UdonSharpBehaviour
    {
        [Header("滑块的初始值")][Range(0,1)]
        [UdonSynced]
        public float Yodo_HapticSliderValue = 0.0f;

        [Header("音量调节目标")]
        [SerializeField]
        private AudioSource[] Yodo_TargetAudioSources;

        [Header("后处理效果调节目标")]
        [SerializeField]
        private PostProcessVolume[] Yodo_TargetPostProcessVolumes;

        [Header("灯光强度调节目标")]
        [SerializeField]
        private Light[] Yodo_TargetLights;

        [Header("值的最大值")]
        [SerializeField]
        private float Yodo_MaxValue = 1.0f;

        [Header("操作模式")]
        [SerializeField]
        private HapticSliderMode Yodo_SliderMode = HapticSliderMode.Local;

        [Header("抓取时的振动强度")][Range(0,1)]
        public float Yodo_SliderVibrationStrength = 0.5f;

        [Header("抓取时的振动持续时间")][Range(0,1)]
        public float Yodo_SliderVibrationDuration = 0.1f;

        [Header("外部联动 Udon（高级）")]
        public GameObject[] Yodo_TargetUdon;

        [Header("外部联动 Udon 变量名")]
        public string[] Yodo_TargetVariableName;

        [Header("外部方法（可复制粘贴）")]
        [TextArea]
        public string CustomEventName = "public void Yodo_OnSliderValueChanged(){}";

        [Header("----以下为系统使用参数----")]
        public GameObject Yodo_Knob = null;
        public float Yodo_MinKnobX = -0.19f;
        public float Yodo_MaxKnobX = 0.0f;
        public Text Yodo_IndicatorText = null;

        private bool isPicking = false;
        private float localSensitivity = 0.002f;
        private float localMovement = 0.0f;
        private float last_knob_x = 0.0f;
        private float localSensitivityCutout = 0.001f;
        private VRCPlayerApi _localPlayer;
        private const float vib_amplitude_coefficient = 0.0636f;    // 0～1じゃないので補正　そのうちVRCのアプデで変わるかも
        private float previous_slider_value;
        private HapticSliderPersistenceManager _persistenceManager = null;

        public void Start()
        {
            int udonnum = 0;
            int namenum = 0;
            previous_slider_value = Yodo_HapticSliderValue;
            _localPlayer = Networking.LocalPlayer;
            if (Yodo_TargetUdon != null)
            {
                udonnum = Yodo_TargetUdon.Length;
            }
            if (Yodo_TargetVariableName != null)
            {
                namenum = Yodo_TargetVariableName.Length;
            }
            if (udonnum != namenum)
            {   // Target UdonとTarget variableの数は同じにしてください。
                Debug.LogError("[Yodo]Target Udon and Target variable name must have same size.(" + this.name + ")");
            }
            AdjustSliderPosition();
            ResetPickupPosition();
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (player == null) { return; }
            if (player.isLocal)
            {
                SendValueToTarget();
            }
        }

        public override void OnPickup()
        {
            isPicking = true;
        }

        public override void OnDrop()
        {
            isPicking = false;
            if (!Yodo_Knob) { return; }
            ResetPickupPosition();
        }

        public void FixedUpdate()
        {
            if (!Yodo_Knob) { return; }
            if (!isPicking) { return; }

            // 新しいValueを算出
            float hand_x = this.transform.localPosition.x;
            float next_x = Mathf.Clamp(hand_x, Yodo_MinKnobX, Yodo_MaxKnobX);
            Yodo_HapticSliderValue = (1.0f - Mathf.Abs((next_x - Yodo_MinKnobX) / (Yodo_MaxKnobX - Yodo_MinKnobX)));
            previous_slider_value = Yodo_HapticSliderValue;
            AdjustSliderPosition();

            if (Yodo_SliderMode == HapticSliderMode.Global)
            {
                if (!Networking.IsOwner(this.gameObject))
                {
                    Networking.SetOwner(_localPlayer, this.gameObject);
                }
                RequestSerialization();
            }
            if (Yodo_SliderMode == HapticSliderMode.LocalAndPersistence)
            {
                if (_persistenceManager != null)
                {
                    _persistenceManager.SetSavedValue(Yodo_HapticSliderValue);
                }
            }

            // 振動フィードバック
            if (last_knob_x != next_x)
            {
                float diff = Mathf.Abs(last_knob_x - next_x);
                if (localSensitivityCutout <= diff)
                {
                    localMovement += diff;
                    last_knob_x = next_x;
                }
            }
            if (localSensitivity <= localMovement)
            {
                localMovement -= localSensitivity;
                VRC_Pickup pickup = (VRC_Pickup)this.GetComponent(typeof(VRC_Pickup));
                if (pickup == null)
                {
                    Debug.LogError("[Yodo]No VRC_PickUp component in this object.");
                    return;
                }
                VRC_Pickup.PickupHand currentHand;
#if UNITY_EDITOR
                currentHand = VRC_Pickup.PickupHand.Right;  // GotoUdon入ってるとPlayModeで死ぬので対策
#else
            currentHand = pickup.currentHand;           // つまみを持ってる方の手
#endif
                _localPlayer.PlayHapticEventInHand(currentHand, Yodo_SliderVibrationDuration, Yodo_SliderVibrationStrength * vib_amplitude_coefficient, 320.0f);
            }
        }

        public void Update()
        {
            if (!isPicking) { return; }
            SendValueToTarget();
        }

        public override void OnDeserialization()
        {
            if (Yodo_SliderMode != HapticSliderMode.Global)
            {
                Yodo_HapticSliderValue = previous_slider_value;
                return;
            }
            AdjustSliderPosition();
            SendValueToTarget();
            ResetPickupPosition();
        }

        private void AdjustSliderPosition()
        {
            float next_x = Mathf.Lerp(Yodo_MaxKnobX, Yodo_MinKnobX, Yodo_HapticSliderValue);
            Vector3 next_pos = Yodo_Knob.transform.localPosition;
            next_pos.x = next_x;
            Yodo_Knob.transform.localPosition = next_pos;
        }

        private void ResetPickupPosition()
        {
            this.transform.SetLocalPositionAndRotation(Yodo_Knob.transform.localPosition, Yodo_Knob.transform.localRotation);
        }

        private void SendValueToTarget()
        {
            for (int cur = 0; cur < Yodo_TargetUdon.Length; cur++)
            {
                if (Yodo_TargetUdon[cur] != null)
                {
                    UdonBehaviour ub = (UdonBehaviour)Yodo_TargetUdon[cur].GetComponent(typeof(UdonBehaviour));
                    ub.SendCustomEvent("Yodo_OnSliderValueChanged");
                    if (Yodo_TargetVariableName[cur] != "")
                    {
                        ub.SetProgramVariable(Yodo_TargetVariableName[cur], Yodo_HapticSliderValue);
                    }
                }
            }

            if (Yodo_IndicatorText != null)
            {
                Yodo_IndicatorText.text = (Yodo_HapticSliderValue * 100).ToString("  0");
            }

            float _value = Yodo_HapticSliderValue * Yodo_MaxValue;

            foreach (AudioSource audio in Yodo_TargetAudioSources)
            {
                if (audio)
                {
                    audio.volume = Mathf.Clamp01(_value);
                }
            }

            foreach (PostProcessVolume ppv in Yodo_TargetPostProcessVolumes)
            {
                if (ppv)
                {
                    ppv.weight = Mathf.Clamp01(_value);
                }
            }

            foreach (Light light in Yodo_TargetLights)
            {
                if (light)
                {
                    light.intensity = _value;
                }
            }
        }

#region Persistence
        public void SetValueWithoutNotify(float newValue)
        {
            if (Yodo_SliderMode == HapticSliderMode.LocalAndPersistence)
            {
                Yodo_HapticSliderValue = newValue;
                AdjustSliderPosition();
                ResetPickupPosition();
                SendValueToTarget();
            }
        }

        public void InitPersistence(HapticSliderPersistenceManager persistenceManager)
        {
            _persistenceManager = persistenceManager;
        }
#endregion
    }
}
