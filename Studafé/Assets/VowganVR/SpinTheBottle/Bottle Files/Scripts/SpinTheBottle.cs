using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace VowganVR
{
    public class SpinTheBottle : UdonSharpBehaviour
    {
        
        [UdonSynced, FieldChangeCallback(nameof(SpinBottleCallback))] public byte SpinBottle;
        
        private Transform root;
        private VRCPlayerApi playerLocal;
        private VRCObjectSync objectSync;
        private Animator anim;
        private int hashSpin;

        public byte SpinBottleCallback
        {
            get => SpinBottle;
            set
            {
                SpinBottle = value;
                anim.SetTrigger(hashSpin);
            }
        }
        
        private void Start()
        {
            playerLocal = Networking.LocalPlayer;
            root = transform.parent;
            objectSync = (VRCObjectSync)root.GetComponent(typeof(VRCObjectSync));
            anim = root.GetComponent<Animator>();
            hashSpin = Animator.StringToHash("Spin");
        }
        
        public override void Interact()
        {
            Networking.SetOwner(playerLocal, gameObject);
            Networking.SetOwner(playerLocal, root.gameObject);
            root.rotation = Quaternion.Euler(0, Random.Range(0f, 359f), 0);
            objectSync.FlagDiscontinuity();
            SpinBottleCallback++;
            if (SpinBottleCallback > 250) SpinBottleCallback = 0;
            RequestSerialization();
        }
    }
}