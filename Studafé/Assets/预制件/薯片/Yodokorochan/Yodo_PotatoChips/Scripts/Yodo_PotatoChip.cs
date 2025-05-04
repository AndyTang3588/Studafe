
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Yodo_PotatoChip : UdonSharpBehaviour
{
    public GameObject Yodo_EffectObjectProvider;
    public AudioClip[] Yodo_crunchClips;
    public Vector3 Yodo_spawnRandomizeRadious = new Vector3(0.1f, 0.03f, 0.1f);
    public float Yodo_UdonChipPrice = 1.0f;
    private AudioSource[] audioObjects;
    private VRC_Pickup pickup = null;
    private Animator animator = null;
    private SphereCollider local_collider = null;

    private UCS.UdonChips udonChips = null;

    private void Start()
    {
        GameObject ucgo = GameObject.Find("UdonChips");
        if (ucgo != null)
        {
            udonChips = ucgo.GetComponent<UCS.UdonChips>();
        }
        local_collider = this.GetComponent<SphereCollider>();
        pickup = (VRC_Pickup)this.GetComponent(typeof(VRC_Pickup));
    }

    public void Update()
    {
        if (udonChips != null)
        {
            if (udonChips.money < Yodo_UdonChipPrice)
            {
                if (pickup.pickupable)
                {
                    pickup.pickupable = false;
                }
            }
            else
            {
                if (!pickup.pickupable)
                {
                    pickup.pickupable = true;
                }
            }
        }
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (player.isLocal)
        {
            audioObjects = Yodo_EffectObjectProvider.GetComponentsInChildren<AudioSource>();
            pickup = (VRC_Pickup)this.GetComponent(typeof(VRC_Pickup));
            animator = this.GetComponent<Animator>();
        }
    }

    public override void OnDrop()
    {
        if (!pickup)
        {
            pickup = (VRC_Pickup)this.GetComponent(typeof(VRC_Pickup));// 何故か時々なるっぽいので一応対策
        }
        if (pickup)
        {
            pickup.PlayHaptics();
        }
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(Yodo_EatChips));
        if (udonChips)
        {
            udonChips.money -= Yodo_UdonChipPrice;
        }
    }

    public void Yodo_EatChips()
    {
        // Play eat sound
        foreach (AudioSource audio in audioObjects)
        {
            if (!audio) { continue; }
            if (!audio.isPlaying)
            {
                if (Yodo_crunchClips.Length == 0) { break; }
                audio.transform.position = this.transform.position;
                audio.PlayOneShot(Yodo_crunchClips[Random.Range(0, Yodo_crunchClips.Length)]);

                ParticleSystem particle = audio.GetComponent<ParticleSystem>();
                if (particle)
                {
                    particle.Play();
                }
                break;
            }
        }

        // Start Transition Animation
        if (animator)
        {
            animator.SetTrigger("Eat");
        }

        // Reset position
        if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
        {
            Vector3 randomizedPosition = new Vector3(
                Random.Range(-Yodo_spawnRandomizeRadious.x, Yodo_spawnRandomizeRadious.x),
                Random.Range(-Yodo_spawnRandomizeRadious.y, Yodo_spawnRandomizeRadious.y),
                Random.Range(-Yodo_spawnRandomizeRadious.z, Yodo_spawnRandomizeRadious.z)
            );

            Vector3 randomizedRotationEular = new Vector3(
                Random.Range(0.0f, 360.0f),
                Random.Range(0.0f, 360.0f),
                Random.Range(0.0f, 360.0f)
            );
            Quaternion randomizedRotation = Quaternion.Euler(randomizedRotationEular);

            this.transform.position = this.transform.parent.position + randomizedPosition;
            this.transform.rotation = randomizedRotation;
        }
    }
}

