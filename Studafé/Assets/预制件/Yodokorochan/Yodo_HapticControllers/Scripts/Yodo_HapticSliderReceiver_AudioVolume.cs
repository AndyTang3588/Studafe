
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Yodo_HapticSliderReceiver_AudioVolume : UdonSharpBehaviour
{
    public bool Yodo_isReceiveSliderValueChangeEvent = true;
    public float Yodo_audioVolume = 0.0f;

    public void Yodo_OnSliderValueChanged()
    {
        AudioSource audio = (AudioSource)this.GetComponent(typeof(AudioSource));
        if(audio != null)
        {
            audio.volume = Yodo_audioVolume;
        }
    }
}
