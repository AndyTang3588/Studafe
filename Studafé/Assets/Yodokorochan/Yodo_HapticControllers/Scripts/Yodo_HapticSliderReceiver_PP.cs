
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Yodo_HapticSliderReceiver_PP : UdonSharpBehaviour
{
    public bool Yodo_isReceiveSliderValueChangeEvent = true;
    public float Yodo_PPWeight = 0.0f;

    public void OnEnable()
    {
        Yodo_OnSliderValueChanged();
    }

    public void Yodo_OnSliderValueChanged()
    {
        Animator animator = (Animator)this.GetComponent(typeof(Animator));
        if (animator != null)
        {
            animator.SetFloat("PP_Weight", Yodo_PPWeight);
        }
    }
}
