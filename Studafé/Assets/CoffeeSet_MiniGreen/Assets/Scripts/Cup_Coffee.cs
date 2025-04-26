
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UdonSharpEditor;
#endif

namespace VRCCoffeeSet
{
    [AddComponentMenu("MiniGreen/Coffee Set/Cup")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    public class Cup_Coffee : UdonSharpBehaviour
    {
        private MainController M_controller;
        public MainController m_controller
        {
            get
            {
                if (!M_controller) M_controller = GetComponentInParent<MainController>();
                return M_controller;
            }
            set { M_controller = value; }
        }
        private VariableHolder M_variables;
        public VariableHolder m_variables
        {
            set { M_variables = value; }
            get
            {
                if (!M_variables) M_variables = GetComponentInChildren<VariableHolder>();
                return M_variables;
            }
        }

        public TypesCup m_type;
        public int m_index;
        public int m_skinIndex;
        /// <summary>
        /// 0_Content, 1_Top <para></para>
        /// Glass - 3_Grapefruit, 4_Grape, 5_Strawberry, 6_Mint, 7-8_Rosemary, 9-10_Thyme1
        /// </summary>
        public byte[] indexMat = null;

        public Animator m_animator;
        public SkinnedMeshRenderer m_meshContent;
        public VRC_Pickup m_pickup;
        /// <summary> 0_Out, 1_Mid </summary>
        public Transform[] m_axis;

        public ParticleSystem m_particle;
        /// <summary> 0_Espresso, 1_Caramel Mix </summary>
        public ParticleSystem[] m_particlePour;

        /// <summary>
        /// 0_Content, 1_Cream <para></para>
        /// Glass : 0_Content, 1_Ice, 2_Straw, 3_Cream, 4_Ade
        /// </summary>
        public Transform[] m_object;

        [HideInInspector] public float time_Delay = 0;
        float m_content = 0;
        /// <summary> -1_Sub, 0_Calm, 1_Add </summary>
        sbyte m_contentCurrent = 0;
        public float m_contentSpeedPerDrink = 0.5f;

        Vector3 defaultPos;
        Quaternion defaultRot;

        void Start()
        {
            defaultPos = transform.position;
            defaultRot = transform.rotation;
            LocalUpdate();
        }

        private void Update()
        {
            CheckContent();
        }

        void CheckContent()
        {
            if (m_content == m_variables.m_info[0]) return;
            
            if (m_type == TypesCup.Espresso && Time.time < time_Delay) return;

            switch (m_contentCurrent)
            {
                case -1:
                    m_content -= Time.deltaTime / m_contentSpeedPerDrink;
                    if (m_content < m_variables.m_info[0])
                    {
                        m_contentCurrent = 0;
                        m_content = m_variables.m_info[0];
                    }
                    break;
                case 1:
                    m_content += Time.deltaTime / m_contentSpeedPerDrink;
                    if (m_content > m_variables.m_info[0])
                    {
                        m_contentCurrent = 0;
                        m_content = m_variables.m_info[0];
                    }
                    break;
                case 0:
                    if (m_content > m_variables.m_info[0]) m_contentCurrent = -1;
                    else if (m_content < m_variables.m_info[0]) m_contentCurrent = 1;
                    break;
            }

            LocalUpdateContent();
        }

        public override void OnPickup() => FuncPickup();
        public void FuncPickup()
        {
            if (m_type == TypesCup.Glass) return;
            if (m_variables.m_deposit[0] == 0) return;
            time_Delay = Time.time + m_controller.m_pickupdelay;

            if (m_variables.m_deposit[0] == 2)
            {
                var plate = GetPlate();
                if (plate.m_pickup.IsHeld && !Networking.IsOwner(plate.gameObject))
                {
                    Drop();
                    return;
                }
                else
                {
                    if (!Networking.IsOwner(plate.gameObject)) Networking.SetOwner(Networking.LocalPlayer, plate.gameObject);
                    plate.SendCustomNetworkEvent(NetworkEventTarget.All, "DepositOff");
                }
            }
            
            m_variables.m_deposit[0] = 0;
            m_variables.m_deposit[1] = (sbyte)-1;
            SerializeVariables();
        }

        public override void OnPickupUseDown() 
        {
            if (m_variables.m_info[0] == 0) return;
            else if (m_type == TypesCup.Coffee && m_variables.m_info[1] < 20) return;
            else if (m_type == TypesCup.Espresso && m_variables.m_info[1] < 12) return;

            if (m_type == TypesCup.Espresso && m_variables.m_info[3] == 0)
            {
                if (m_axis[0].position.y < m_axis[1].position.y)
                {
                    SendCustomNetworkEvent(NetworkEventTarget.All, "PourContent");
                    return;
                }
            }

            if (m_contentCurrent != 0) return;

            if (m_variables.m_info[3] > 1)
            {
                m_variables.m_info[3]--;
                if (m_variables.m_info[3] == 1) m_variables.m_info[2] = 0;
            }
            else
            {
                m_variables.m_info[0]--;
                if (m_variables.m_info[0] == 0) { m_variables.m_info[1] = 0; m_variables.m_info[2] = 0; }
            }
            LocalUpdate();
            SerializeVariables();
        }
       
        public void LocalUpdate()
        {
            byte[] VI = m_variables.m_info;

            LocalUpdateContent();

            // Animator
            m_animator.SetInteger("Cream", VI[3]);
            if (m_type == TypesCup.Glass) m_animator.SetBool("Ice", VI[7] == 1);

            // Mat - Content
            if (m_type == TypesCup.Glass && VI[1] > 0) m_object[0].gameObject.SetActive(true);
            sbyte intSurface = 0; // 0_Flat, 1_Convex, -1_Concave
            Material[] tempMat = m_meshContent.sharedMaterials;
            switch (VI[1])
            {
                case 1:
                    tempMat[indexMat[0]] = m_controller.mat_Syrup[0]; intSurface = 1; // Syrup_Chocolate
                    break;
                case 2:
                    tempMat[indexMat[0]] = m_controller.mat_Syrup[1]; intSurface = 1; // Syrup_Caramel
                    break;
                case 11:
                    tempMat[indexMat[0]] = m_controller.mat_Water; intSurface = -1; // Water
                    break;
                case 12: case 13: case 14:
                    byte tempByte = (byte)(VI[1] - 12); // Espresso, MochaMix, CaramelMix
                    tempMat[indexMat[0]] = m_type == TypesCup.Glass ?
                    m_controller.mat_EspressoIce[tempByte] : m_controller.mat_Espresso[tempByte]; intSurface = -1;
                    break;
                case 17:
                    tempMat[indexMat[0]] = m_controller.mat_Milk[0]; intSurface = -1; // Milk
                    break;
                case 18:
                    tempMat[indexMat[0]] = m_controller.mat_Macchiato[0]; intSurface = 1; // Milk w/ caramel
                    break;
                case 21:
                    tempMat[indexMat[0]] = m_type == TypesCup.Glass ? m_controller.mat_AmericanoIce : m_controller.mat_Americano; intSurface = -1; // Americano
                    break;
                case 31: case 32: case 33: case 34:
                    tempMat[indexMat[0]] = m_type == TypesCup.Glass ? m_controller.mat_LatteIce : m_controller.mat_Latte[VI[1] - 31]; intSurface = 1; // Latte 1~4
                    break;
                case 35: case 36:
                    tempMat[indexMat[0]] = m_controller.mat_Cappuccino[VI[1] - 35]; intSurface = 1; // Cappuccino 1~2
                    break;
                case 37:
                    tempMat[indexMat[0]] = m_type == TypesCup.Glass ? m_controller.mat_MacchiatoIce : m_controller.mat_Macchiato[1]; intSurface = 1; // Caramel Macchiato
                    break;
                case 41: case 42:
                    tempMat[indexMat[0]] = m_type == TypesCup.Glass ? m_controller.mat_MochaIce : m_controller.mat_Mocha[VI[1] - 41]; intSurface = 1; // Mocha
                    break;
                case 43: case 44:
                    tempMat[indexMat[0]] = m_controller.mat_MochaArt[VI[1] - 43]; intSurface = 1; // Mocha Art
                    break;
                case 61: case 62: case 63: case 64: case 65: case 66: case 67: case 68: // Ade
                    tempMat[indexMat[0]] = m_controller.mat_Ade[VI[1] - 61];
                    break;
            }

            // Top, Surface
            if (m_type == TypesCup.Coffee)
            {
                tempMat[indexMat[1]] = VI[3] < 2 && VI[2] > 0 ? m_controller.mat_Top[VI[2] - 1] : m_controller.mat_Empty;
                m_meshContent.SetBlendShapeWeight(0, intSurface == 1 ? 100 : 0); // Surface_Add
                m_meshContent.SetBlendShapeWeight(1, intSurface == -1 ? 100 : 0); // Surface_Sub
            }

            // Cream
            int indexCream = m_type == TypesCup.Glass ? 3 : 1;
            m_object[indexCream].gameObject.SetActive( VI[3] > 0 );
            m_object[indexCream].GetChild(0).gameObject.SetActive( m_type == TypesCup.Coffee ? VI[3] > 1 && VI[2] > 0 : VI[2] > 0 );
            // Cream Top
            Transform creamTop = m_object[indexCream].GetChild(0);
            creamTop.gameObject.SetActive(VI[3] > 1 && VI[2] > 0);
            if (VI[3] > 1 && VI[2] > 0) creamTop.GetComponent<SkinnedMeshRenderer>().material = m_controller.mat_Top[VI[2] - 1];

            // Apply materials
            m_meshContent.sharedMaterials = tempMat;

            // Straw and Garnish of Glass cup
            if (m_type == TypesCup.Glass)
            {
                // Straw
                if (VI[6] > 0)
                {
                    m_object[2].gameObject.SetActive(true);
                    m_object[2].GetComponent<Renderer>().material = m_controller.mat_Straw[VI[6] - 1];
                }
                else m_object[2].gameObject.SetActive(false);
                // DLC Ade
                if (m_object[4])
                {
                    // Fruit
                    m_object[4].GetChild(0).GetChild(2).gameObject.SetActive(VI[4] == 1); // Lemon
                    m_object[4].GetChild(0).GetChild(1).gameObject.SetActive(VI[4] == 2); // Grapefruit
                    m_object[4].GetChild(0).GetChild(0).gameObject.SetActive(VI[4] == 3); // Grape
                    m_object[4].GetChild(0).GetChild(3).gameObject.SetActive(VI[4] == 4); // Strawberry
                    // Herb
                    m_object[4].GetChild(1).GetChild(0).gameObject.SetActive(VI[5] == 1); // Mint
                    m_object[4].GetChild(1).GetChild(1).GetChild(0).gameObject.SetActive(VI[5] == 2); // Rosemary 1
                    m_object[4].GetChild(1).GetChild(1).GetChild(1).gameObject.SetActive(VI[5] == 3); // Rosemary 2
                    m_object[4].GetChild(1).GetChild(2).GetChild(0).gameObject.SetActive(VI[5] == 4); // Thyme 1
                    m_object[4].GetChild(1).GetChild(2).GetChild(1).gameObject.SetActive(VI[5] == 5); // Thyme 2
                    // Ice React
                    SkinnedMeshRenderer meshIce = m_object[1].GetComponent<SkinnedMeshRenderer>();
                    meshIce.SetBlendShapeWeight(3, VI[7] == 1 && VI[4] == 2 ? 100 : 0); // Grapefruit
                    meshIce.SetBlendShapeWeight(4, VI[7] == 1 && VI[4] == 4 ? 100 : 0); // Strawberry
                    meshIce.SetBlendShapeWeight(5, VI[7] == 1 && VI[4] == 3 ? 100 : 0); // Grape
                    meshIce.SetBlendShapeWeight(6, VI[7] == 1 && VI[5] > 1 ? 100 : 0); // Herb
                }
            }

            // Particle - Steam or Bubble
            if (m_type == TypesCup.Glass)
            {
                if (m_particle) switch (VI[1])
                {
                    case 62: case 64: case 66: case 68:
                        if (!m_particle.isPlaying) m_particle.Play();
                        break;
                    default:
                        if (m_particle.isPlaying) m_particle.Stop();
                        break;
                }
            }
            else
            {
                if (!m_particle.isPlaying && VI[0] != 0 && VI[1] > 10 && VI[3] < 2) m_particle.Play();
                else if ((m_particle.isPlaying && VI[0] == 0) || VI[3] > 1) m_particle.Stop();
            }
        }

        public void LocalUpdateContent()
        {
            m_object[0].gameObject.SetActive(m_content > 0 ? true : false);
            m_animator.SetFloat("Content", MathRemap(m_content, 0, 5, 1, 0));
        }

        void OnParticleCollision(GameObject other)
        {
            if (!Networking.IsOwner(gameObject)) return;

            string[] nameContact_Splited = other.name.Split(new char[] { '_' });
            if (nameContact_Splited[0] != "Particle") return;

            byte[] tempInfo = new byte[m_variables.m_info.Length];
            for (int i = 0; i < tempInfo.Length; i++) tempInfo[i] = m_variables.m_info[i];

            if (m_type == TypesCup.Espresso) CheckParticle_Espresso(nameContact_Splited[1]);
            else if (m_type == TypesCup.Coffee) CheckParticle_Coffee(nameContact_Splited[1]);
            else if (m_type == TypesCup.Glass) CheckParticle_Glass(nameContact_Splited[1]);

            bool tempBool = true;
            for (int i = 0; i < tempInfo.Length; i++) if (tempInfo[i] != m_variables.m_info[i]) { tempBool = false; break; }

            if (tempBool) return;

            SerializeVariables();
            LocalUpdate();
        }

    #region Espresso

        void CheckParticle_Espresso(string name) {
            byte[] VI = m_variables.m_info;
            switch (name) {
                case "PowderChoco":
                    if (VI[3] == 3 && VI[2] == 0) VI[2] = 1;
                    break;
                case "PowderCinamon":
                    if (VI[3] == 3 && VI[2] == 0) VI[2] = 2;
                    break;
                case "SyrupChoco":
                    if (VI[3] == 3 && VI[2] == 0) VI[2] = 3;
                    break;
                case "SyrupCaramel":
                    if (VI[3] == 3 && VI[2] == 0) VI[2] = 4;
                    else if (VI[1] == 12 && VI[0] == 5 && VI[3] == 0) VI[1] = 14;
                    break;
                case "Cream":
                    if (VI[0] == 5 && VI[3] < 2) VI[3] = 3;
                    break;
                case "WaterWash":
                    VI[0] = 0; VI[1] = 0; VI[2] = 0; VI[3] = 0;
                    break;
                default: break;
            }

        }

        public void PourContent()
        {
            m_particlePour[m_variables.m_info[1] == 12 ? 0 : 1].Play();
            m_variables.m_info[0] = 0;
            m_variables.m_info[1] = 0;
            LocalUpdate();
        }

    #endregion

    #region Coffee

        void CheckParticle_Coffee(string name) {
            byte[] VI = m_variables.m_info;
            switch (name) {
                case "WaterHot":
                    if (VI[1] == 0 || (VI[1] == 11 && VI[0] != 4)) { VI[0] = 4; VI[1] = 11; } // Empty > Water
                    else if (VI[1] == 12 && VI[0] == 2) { VI[0] = 5; VI[1] = 21; } // Espresso > Americano
                    break;
                case "Espresso":
                    if (VI[1] == 0) { VI[0] = 2; VI[1] = 12; } // Empty > Espresso
                    else if (VI[1] == 1) { VI[0] = 2; VI[1] = 13; } // SyrupChoco > EspressoMocha
                    else if (VI[1] == 11 && VI[0] == 4) { VI[0] = 5; VI[1] = 21; } // Water > Americano
                    else if (VI[1] == 18 && VI[0] == 4) { VI[0] = 5; VI[1] = 37; } // MilkCaramel > Macchiato
                    break;
                case "MilkFrothed 1":
                    if (VI[1] == 12 && VI[0] == 2) { VI[0] = 5; VI[1] = (byte)(30 + UnityEngine.Random.Range(1, 5)); } // Epsresso > Latte
                    else if (VI[1] == 13 && VI[0] == 2) {
                        if (VI[2] == 1) { VI[0] = 5; VI[2] = 0; VI[1] = (byte)(40 + UnityEngine.Random.Range(3, 5)); } // Espresso, PowderChoco > MochaArt
                        else { VI[0] = 5; VI[1] = (byte)(40 + UnityEngine.Random.Range(1, 3)); } // Espresso > Latte
                    } else if (VI[1] == 2) { VI[0] = 4; VI[1] = 18; } // SyrupCaramel > MilkCaramel
                    break;
                case "MilkFrothed 2":
                    if (VI[1] == 12 && VI[0] == 2) { VI[0] = 5; VI[1] = (byte)(30 + UnityEngine.Random.Range(5, 7)); } // Espresso > Cappuccino
                    break;
                case "PowderChoco":
                    if (VI[2] == 0 && (VI[3] == 3 || VI[1] == 13 || VI[1] == 41 || VI[1] == 42)) VI[2] = 1;
                    break;
                case "PowderCinamon":
                    if (VI[2] == 0 && (VI[3] == 3 || VI[1] == 35 || VI[1] == 36)) VI[2] = 2;
                    break;
                case "SyrupChoco":
                    if (VI[1] == 0 && VI[0] == 0) { VI[0] = 1; VI[1] = 1; } // Empty > SyrupChoco
                    else if (VI[2] == 0 && (VI[3] == 3 || VI[1] == 41 || VI[1] == 42)) VI[2] = 3;
                    break;
                case "SyrupCaramel":
                    if (VI[0] == 0 && VI[1] == 0) { VI[0] = 1; VI[1] = 2; } // Empty > SyrupCaramel
                    if (VI[2] == 0 && (VI[3] == 3 || VI[1] == 37)) VI[2] = 4;
                    break;
                case "Cream":
                    if (VI[0] == 5 && VI[3] < 2 && VI[2] == 0) VI[3] = 3;
                    break;
                case "WaterWash":
                    VI[0] = 0; VI[2] = 0; VI[3] = 0; VI[1] = 0;
                    break;
                default: break;
            }
        }

    #endregion

    #region Glass

        void CheckParticle_Glass(string name)
        {
            byte[] VI = m_variables.m_info;
            switch (name)
            {
                case "Ice":
                    if (VI[7] == 0) VI[7] = 1;
                    return;
                case "WaterCold":
                    if (VI[7] == 0) return;
                    if (VI[1] == 0 || (VI[1] == 11 && VI[0] != 4)) { VI[0] = 4; VI[1] = 11; } // Empty > Water
                    else if (VI[1] == 12) { VI[0] = 5; VI[1] = 21; } // Espresso > Americano
                    return;
                case "Milk":
                    if (VI[7] == 0) return;
                    if (VI[1] == 0 || (VI[1] == 17 && VI[0] != 4)) { VI[0] = 4; VI[1] = 17; } // Empty > Milk
                    else if (VI[1] == 13) { VI[0] = 5; VI[1] = 41; } // EspressoChoco > Mocha
                    return;
                case "Espresso":
                    if (VI[7] == 0) {
                        if (VI[1] == 0) { VI[0] = 1; VI[1] = 12; } // Empty > Espresso
                    } else {
                        if (VI[1] == 11 && VI[0] == 4) { VI[0] = 5; VI[1] = 21; } // Water > Americano
                        else if (VI[1] == 17 && VI[0] == 4) { VI[0] = 5; VI[1] = 31; } // Espresso > Latte
                    }
                    return;
                case "EspressoCaramel":
                    if (VI[1] == 17 && VI[0] == 4) { VI[0] = 5; VI[1] = 37; } // Milk > Macchiato
                    return;
                case "PowderChoco":
                    if (VI[3] == 3 && VI[2] == 0) VI[2] = 1;
                    return;
                case "PowderCinamon":
                    if (VI[3] == 3 && VI[2] == 0) VI[2] = 2;
                    return;
                case "SyrupChoco":
                    if (VI[3] == 3 && VI[2] == 0) VI[2] = 3;
                    else if (VI[7] == 0 && VI[1] == 12) { VI[1] = 13; }
                    return;
                case "SyrupCaramel":
                    if (VI[3] == 3 && VI[2] == 0) VI[2] = 4;
                    return;
                case "Cream":
                    if (VI[1] > 20 && VI[3] < 2 && VI[0] == 5) VI[3] = 3;
                    return;
                case "WaterWash":
                    VI[0] = 0; VI[1] = 0; VI[2] = 0; VI[3] = 0; VI[4] = 0; VI[5] = 0; VI[6] = 0; VI[7] = 0;
                    return;
                default: break;
            }
            if (m_controller.m_init[1]) switch (name)
            {
                case "JuiceLemon":
                    if (VI[1] == 0) { VI[0] = 1; VI[1] = 61; }
                    return;
                case "JuiceGrape":
                    if (VI[1] == 0) { VI[0] = 1; VI[1] = 63; }
                    return;
                case "JuiceGrapefruit":
                    if (VI[1] == 0) { VI[0] = 1; VI[1] = 65; }
                    return;
                case "JuiceStrawberry":
                    if (VI[1] == 0) { VI[0] = 1; VI[1] = 67; }
                    return;
                case "SparklingWater":
                    if (VI[7] == 0) return;
                    if (VI[1] == 61) { VI[0] = 5; VI[1] = 62; } else if (VI[1] == 63) { VI[0] = 5; VI[1] = 64; } else if (VI[1] == 65) { VI[0] = 5; VI[1] = 66; } else if (VI[1] == 67) { VI[0] = 5; VI[1] = 68; }
                    return;
                case "LemonSlice":
                    if (VI[7] == 1 && VI[4] == 0) VI[4] = 1;
                    return;
                case "GrapefruitSlice":
                    if (VI[7] == 1 && VI[4] == 0) VI[4] = 2;
                    return;
                case "GreenGrape":
                    if (VI[7] == 1 && VI[4] == 0) VI[4] = 3;
                    return;
                case "Strawberry":
                    if (VI[7] == 1 && VI[4] == 0) VI[4] = 4;
                    return;
                case "Mint":
                    if (VI[7] == 1 && VI[5] == 0) VI[5] = 1;
                    return;
                case "Rosemary":
                    if (VI[7] == 1 && VI[5] == 0) VI[5] = (byte)(UnityEngine.Random.Range(2, 4));
                    return;
                case "Thyme":
                    if (VI[7] == 1 && VI[5] == 0) VI[5] = (byte)(UnityEngine.Random.Range(4, 6));
                    return;
                default: break;
            }
        }

    #endregion

        public void Drop() { m_pickup.Drop(); }
        public void SetPickup(bool value) { m_pickup.pickupable = value; }

        public void Call_Reset()
        {
            if (m_pickup.IsHeld || !m_pickup.pickupable) return;
            if (m_type != TypesCup.Glass) {
                if (m_variables.m_deposit[0] == 1) 
                    if (((VRC_Pickup)GetPlate().GetComponent(typeof(VRC_Pickup))).IsHeld) return;
                if (m_variables.m_deposit[0] > 0) SendCustomEvent("FuncPickup");
            }
            if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
            transform.SetPositionAndRotation(defaultPos, defaultRot);
            RequestSerialization();
        }

        Cup_Plate GetPlate()
        {
            if (m_type == TypesCup.Espresso) return m_controller.dish_espressoPlate[ m_variables.m_deposit[1] ];
            else if (m_type == TypesCup.Coffee) return m_controller.dish_coffeePlate[ m_variables.m_deposit[1] ];
            else return null;
        }
        public void SerializeVariables()
        {
            if (!Networking.IsOwner(m_variables.gameObject)) Networking.SetOwner(Networking.LocalPlayer, m_variables.gameObject);
            m_variables.RequestSerialization();
        }
        float MathRemap(float val, float inMin, float inMax, float outMin, float outMax) { return outMin + (val - inMin) * (outMax - outMin) / (inMax - inMin); }
    }

    #if !COMPILER_UDONSHARP && UNITY_EDITOR
    [CustomEditor(typeof(VRCCoffeeSet.Cup_Coffee))]
    public class Cup_Coffee_Editor : Editor
    {
        Cup_Coffee script;
        VariableHolder scriptV;
        TypesCup _type;

        SkinsCupEspresso skinEspresso;
        SkinsCupCoffee skinCoffee;
        SkinsCupGlass skinGlass;

        Renderer M_renderer;
        Renderer m_renderer
        {
            get
            {
                if (!M_renderer) M_renderer = script.GetComponentInChildren<Renderer>();
                return M_renderer;
            }
            set { M_renderer = value; }
        }
        Color[] skinColor = new Color[0];

        bool toggleVariable = false;
        bool toggleBase = false;

        GUIStyle paddingLeft = new GUIStyle();

        private void Awake()
        {
            script = target as Cup_Coffee;

            paddingLeft.padding.left = 10;
        }

        void Initialize()
        {
            _type = script.m_type;

            Transform thisObj = Selection.activeTransform;

            script.m_controller = FindObjectOfType<MainController>();
            scriptV = thisObj.GetComponentInChildren<VariableHolder>();
            scriptV.scriptCup = script;

            // Variables
            if (script.m_type != TypesCup.Glass)
            {
                scriptV.m_deposit = new sbyte[] { 0, -1 };
                scriptV.m_info = new byte[] { 0, 0, 0, 0 };
            }
            else
            {
                scriptV.m_deposit = null;
                scriptV.m_info = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            }

            // Object
            if (script.m_type == TypesCup.Glass) Array.Resize<Transform>(ref script.m_object, 5);
            else Array.Resize<Transform>(ref script.m_object, 2);

            // Components
            if (script.m_animator == null) script.m_animator = thisObj.GetComponentInChildren<Animator>();
            if (script.m_pickup == null) script.m_pickup = (VRC_Pickup)thisObj.GetComponent(typeof(VRC_Pickup));
            if (script.m_meshContent == null)
            {
                if (script.m_object[0]) script.m_meshContent = script.m_object[0].GetComponent<SkinnedMeshRenderer>();
            }

            // Axis and Particle Pour
            if (script.m_type == TypesCup.Espresso)
            {
                Array.Resize<Transform>(ref script.m_axis, 2);
                Array.Resize<ParticleSystem>(ref script.m_particlePour, 2);
            }
            else
            {
                script.m_axis = null;
                script.m_particlePour = null;
            }

            // Material and Skin
            switch (script.m_type)
            {
                case TypesCup.Espresso:
                    script.indexMat = new byte[] { 0 };
                    break;
                case TypesCup.Coffee:
                    script.indexMat = new byte[] { 0, 1 };
                    skinCoffee = SkinManager.GetSkinCupCoffee(script);
                    skinColor = SkinManager.GetColorCup(script, m_renderer);
                    break;
                case TypesCup.Glass:
                    script.indexMat = new byte[] { 0 };
                    break;
            }
        }

        public override void OnInspectorGUI()
        {
            if (_type != script.m_type) Initialize();

        #region Skin

            switch (script.m_type)
            {
                case TypesCup.Espresso:
                    // Skin
                    SkinsCupEspresso tempSkinE = skinEspresso;
                    EditorGUI.BeginChangeCheck();
                    skinEspresso = (SkinsCupEspresso)EditorGUILayout.EnumPopup("Skin Select", skinEspresso);
                    if ( EditorGUI.EndChangeCheck() )
                    {
                        string tempStr = SkinManager.ChangeCup(script, (int)skinEspresso);
                        if (tempStr == null) return;
                        else
                        {
                            skinEspresso = tempSkinE;
                            Debug.LogWarning(tempStr);
                        }
                    }

                    // Color
                    EditorGUILayout.BeginVertical(paddingLeft);
                    switch (skinEspresso)
                    {
                        default: break;
                    }
                    EditorGUILayout.EndVertical();
                    break;
                
                case TypesCup.Coffee:
                    // Skin
                    SkinsCupCoffee tempSkinC = skinCoffee;
                    EditorGUI.BeginChangeCheck();
                    skinCoffee = (SkinsCupCoffee)EditorGUILayout.EnumPopup("Skin Select", skinCoffee);
                    if ( EditorGUI.EndChangeCheck() )
                    {
                        string tempStr = SkinManager.ChangeCup(script, (int)skinCoffee);
                        if (tempStr == null) return;
                        else
                        {
                            skinCoffee = tempSkinC;
                            Debug.LogWarning(tempStr);
                        }
                    }

                    // Color
                    EditorGUILayout.BeginVertical(paddingLeft);
                    switch (skinCoffee)
                    {
                        case SkinsCupCoffee.Mug:
                            EditorGUI.BeginChangeCheck();
                            skinColor[0] = EditorGUILayout.ColorField("Cup Color", skinColor[0]);
                            if ( EditorGUI.EndChangeCheck() ) SkinManager.SetColor(m_renderer, 0, skinColor[0]);
                            break;
                    }
                    EditorGUILayout.EndVertical();
                    break;

                case TypesCup.Glass:
                    // Skin
                    SkinsCupGlass tempSkinG = skinGlass;
                    EditorGUI.BeginChangeCheck();
                    skinGlass = (SkinsCupGlass)EditorGUILayout.EnumPopup("Skin Select", skinGlass);
                    if ( EditorGUI.EndChangeCheck() )
                    {
                        string tempStr = SkinManager.ChangeCup(script, (int)skinGlass);
                        if (tempStr == null) return;
                        else
                        {
                            skinGlass = tempSkinG;
                            Debug.LogWarning(tempStr);
                        }
                    }

                    // Color
                    EditorGUILayout.BeginVertical(paddingLeft);
                    switch (skinGlass)
                    {
                        default: break;
                    }
                    EditorGUILayout.EndVertical();
                    break;
            }

        #endregion

        #region Variable

            GUILayout.Space(20);
            toggleVariable = EditorGUILayout.Foldout(toggleVariable, "Do not change any variables");
            if (!toggleVariable) return;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_type"), new GUIContent("Cup Type"));
            serializedObject.ApplyModifiedProperties();

            if (_type != script.m_type) Initialize();
            
            switch (script.m_type)
            {
                case TypesCup.Espresso:
                    GUILayout.Label("Axis");
                    script.m_axis[0] = EditorGUILayout.ObjectField("_Out", script.m_axis[0], typeof(Transform), true) as Transform;
                    script.m_axis[1] = EditorGUILayout.ObjectField("_Middle", script.m_axis[1], typeof(Transform), true) as Transform;
                    EditorGUILayout.Space();
                    GUILayout.Label("Particle");
                    script.m_particlePour[0] = EditorGUILayout.ObjectField("_Pour", script.m_particlePour[0], typeof(ParticleSystem), true) as ParticleSystem;
                    script.m_particlePour[1] = EditorGUILayout.ObjectField("_Pour Caramel Mix", script.m_particlePour[1], typeof(ParticleSystem), true) as ParticleSystem;
                    script.m_particle = EditorGUILayout.ObjectField("_Steam", script.m_particle, typeof(ParticleSystem), true) as ParticleSystem;
                    EditorGUILayout.Space();
                    GUILayout.Label("Object");
                    EditorGUI.BeginChangeCheck();
                    script.m_object[0] = EditorGUILayout.ObjectField("Content", script.m_object[0], typeof(Transform), true) as Transform;
                    if (EditorGUI.EndChangeCheck()) Initialize();
                    script.m_object[1] = EditorGUILayout.ObjectField("Cream", script.m_object[1], typeof(Transform), true) as Transform;
                    break;
                case TypesCup.Coffee:
                    script.m_particle = EditorGUILayout.ObjectField("Particle Steam", script.m_particle, typeof(ParticleSystem), true) as ParticleSystem;
                    EditorGUILayout.Space();
                    GUILayout.Label("Object");
                    EditorGUI.BeginChangeCheck();
                    script.m_object[0] = EditorGUILayout.ObjectField("Content", script.m_object[0], typeof(Transform), true) as Transform;
                    if (EditorGUI.EndChangeCheck()) Initialize();
                    script.m_object[1] = EditorGUILayout.ObjectField("Cream", script.m_object[1], typeof(Transform), true) as Transform;
                    break;
                case TypesCup.Glass:
                    script.m_particle = EditorGUILayout.ObjectField("Particle Bubble", script.m_particle, typeof(ParticleSystem), true) as ParticleSystem;
                    EditorGUILayout.Space();
                    GUILayout.Label("Object");
                    EditorGUI.BeginChangeCheck();
                    script.m_object[0] = EditorGUILayout.ObjectField("Content", script.m_object[0], typeof(Transform), true) as Transform;
                    if ( EditorGUI.EndChangeCheck() ) Initialize();
                    script.m_object[1] = EditorGUILayout.ObjectField("Ice", script.m_object[1], typeof(Transform), true) as Transform;
                    script.m_object[2] = EditorGUILayout.ObjectField("Straw", script.m_object[2], typeof(Transform), true) as Transform;
                    script.m_object[3] = EditorGUILayout.ObjectField("Cream", script.m_object[3], typeof(Transform), true) as Transform;
                    script.m_object[4] = EditorGUILayout.ObjectField("Ade", script.m_object[4], typeof(Transform), true) as Transform;
                    break;
            }

            EditorGUILayout.Space();
            toggleBase = EditorGUILayout.BeginFoldoutHeaderGroup(toggleBase, "Udon Variables");
            if (toggleBase) {
                UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target);
                base.OnInspectorGUI();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion
        }
    }
    #endif
}