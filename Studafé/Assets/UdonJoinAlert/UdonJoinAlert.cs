
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace NanaseWorks
{
    public class UdonJoinAlert : UdonSharpBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            audioSource.Play();
        }
    }
}