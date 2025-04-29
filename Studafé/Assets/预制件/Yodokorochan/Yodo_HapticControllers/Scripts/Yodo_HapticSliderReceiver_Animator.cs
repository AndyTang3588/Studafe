
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Yodokorochan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Yodo_HapticSliderReceiver_Animator : UdonSharpBehaviour
    {
        [Header("目标 Animator")]
        [SerializeField]
        private Animator Yodo_TargetAnimator;

        [Header("目标 Float 参数名")]
        [SerializeField]
        private string Yodo_TargetParameterName;

        [Header("接收滑块值的变量")]
        // [HideInInspector]
        // 如果加上这个会从 Inspector 中隐藏，因此是有效的
        public float Yodo_CurrentSliderValue = 0.0f;

        void Start()
        {
            if(!Yodo_TargetAnimator)
            {
                Debug.LogError($"[Yodo]目标 Animator 为空。[{this.name}]");
            }
            if(Yodo_TargetParameterName == "")
            {
                Debug.LogError($"[Yodo]目标 Float 参数名 为空。[{this.name}]");
            }
        }
        public void Yodo_OnSliderValueChanged()
        {
            Yodo_TargetAnimator.SetFloat(Yodo_TargetParameterName, Yodo_CurrentSliderValue);
        }
    }
}
