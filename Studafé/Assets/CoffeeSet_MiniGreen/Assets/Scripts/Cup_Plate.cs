
using UdonSharp;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;

namespace VRCCoffeeSet
{
    [AddComponentMenu("MiniGreen/Coffee Set/Plate")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    public class Cup_Plate : UdonSharpBehaviour
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

        [Header("DO NOT Change variables")]
        public Transform AxisDeposit;
        public TypesCup m_type;
        public int m_index;
        public int m_skinIndex = 0;
        public int m_matIndex = 0;

        public VRC_Pickup m_pickup;

        Vector3 defaultPos;
        Quaternion defaultRot;

        /// <summary> -1_No cup, 0~_Index of cup </summary>
        [UdonSynced] [HideInInspector] public int indexCup = -1;
        /// <summary> -1_None, 0~_Index of Ring </summary>
        [UdonSynced] [HideInInspector] public int stateRing = -1;
        GameObject objCup;

        void Start()
        {
            defaultPos = transform.position;
            defaultRot = transform.rotation;

            m_pickup = (VRC_Pickup)GetComponent(typeof(VRC_Pickup));
            SetRing();
        }

        public override void OnDeserialization()
        {
            // Set objCup
            if (indexCup > -1 && !objCup)
            {
                objCup = GetCupUdon(indexCup).gameObject;
                SetPositionCup(true);
            }
            else if (indexCup == -1 && objCup)
            {
                SetPositionCup(false);
                objCup = null;
            }
            SetRing();
        }

        public void DepositOff() // Networked _Cup
        {
            SetPositionCup(false);
            indexCup = -1;
            objCup = null;
        }
        
        public override void OnPickup()
        {
            if (indexCup == -1) return;

            // Check state of deposit of cup for safe if not synced
            Cup_Coffee udonCup = GetCupUdon(indexCup);
            if (udonCup.m_variables.m_deposit[0] == 2)
            {
                if (!objCup) objCup = udonCup.gameObject;
                if (!Networking.IsOwner(objCup)) Networking.SetOwner(Networking.LocalPlayer, objCup);
            }
            else
            {
                indexCup = -1;
                objCup = null;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            // Only when cup not deposited
            if (indexCup > -1) return;

            // Check contact obj is matching with target
            Cup_Coffee udonContact = other.GetComponent<Cup_Coffee>();
            if (!udonContact) return;

            if (m_pickup.IsHeld && !Networking.IsOwner(gameObject)) return;
            if (udonContact.m_pickup.IsHeld && !Networking.IsOwner(udonContact.gameObject)) return;

            if (udonContact.m_type != m_type) return;
            if (udonContact.m_skinIndex != m_skinIndex) return;

            // Check delay time of pickup
            if (udonContact.time_Delay > Time.time) return;
            // Check states of deposit 
            if (udonContact.m_variables.m_deposit[0] != 0) return;

            udonContact.Drop();

            // Setup
            objCup = other.gameObject;
            indexCup = udonContact.m_index;

            // Cup
            udonContact.m_variables.m_deposit[0] = 2;
            udonContact.m_variables.m_deposit[1] = (sbyte)m_index;
            SetPositionCup(true);

            // Plate - Ring
            if (stateRing > -1) return;

            // Return if content is full or empty
            if (udonContact.m_variables.m_info[0] == 0 || udonContact.m_variables.m_info[0] == 5) return;

            // Check if content of "Coffee Cup" is not coffee
            if (udonContact.m_type == TypesCup.Coffee) if (udonContact.m_variables.m_info[1] < 20) return;

            stateRing = Random.Range(0,2);
            SetRing();
        }

        void OnParticleCollision(GameObject other)
        {
            if (!Networking.IsOwner(gameObject)) return;

            string[] nameContact_Splited = other.name.Split(new char[] {'_'});
            if (nameContact_Splited[0] != "Particle") return;

            switch (nameContact_Splited[1]) {
                case "WaterWash":
                    if (stateRing > -1) stateRing = -1; SetRing();
                break;
                default: return;
            }
        }

        void SetRing()
        {
            var meshSelf = GetComponent<Renderer>();
            Material[] tempMat = meshSelf.materials;
            if (stateRing > 0) tempMat[1] = m_controller.mat_Ring[stateRing];
            else tempMat[1] = m_controller.mat_Empty;
            meshSelf.materials = tempMat;
        }

        void SetPositionCup(bool value)
        {
            if (value)
            {
                ConstraintSource sourceSelf = new ConstraintSource();
                sourceSelf.sourceTransform = AxisDeposit;
                sourceSelf.weight = 1;
                ParentConstraint cupConstraint = objCup.GetComponent<ParentConstraint>();
                cupConstraint.SetSource(0, sourceSelf);
                cupConstraint.constraintActive = true;
            }
            else if (objCup) objCup.GetComponent<ParentConstraint>().constraintActive = false;
        }
        Cup_Coffee GetCupUdon(int index)
        {
            Cup_Coffee udon = null;
            if (m_type == TypesCup.Espresso) udon = m_controller.dish_espressoCup[index];
            else if (m_type == TypesCup.Coffee) udon = m_controller.dish_coffeeCup[index];
            return udon;
        }

        public void Call_Reset()
        {
            if (m_pickup.IsHeld) return;
            if (!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
            if (indexCup > -1) {
                Cup_Coffee udonCup = GetCupUdon(indexCup);
                if (!Networking.IsOwner(udonCup.gameObject)) Networking.SetOwner(Networking.LocalPlayer, udonCup.gameObject);
                udonCup.FuncPickup();
            }
            transform.SetPositionAndRotation(defaultPos, defaultRot);
        }
    }
}