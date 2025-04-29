
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Yodokorochan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Yodo_HapticHandProvider : UdonSharpBehaviour
    {
        [Header("允许用手触发开关")]
        [SerializeField]
        private bool SwitchWithHands = true;

        [Header("允许用脚触发开关")]
        [SerializeField]
        private bool SwitchWithFoots = false;

        [Header("允许用头撞触发开关")]
        [SerializeField]
        private bool SwitchWithHead = false;

        [Header("左手碰撞体")]
        [SerializeField]
        private GameObject LeftHandObject;

        [Header("右手碰撞体")]
        [SerializeField]
        private GameObject RightHandObject;

        [Header("左脚碰撞体")]
        [SerializeField]
        private GameObject LeftFootObject;

        [Header("右脚碰撞体")]
        [SerializeField]
        private GameObject RightFootObject;

        [Header("头部碰撞体")]
        [SerializeField]
        private GameObject HeadObject;

        private HumanBodyBones targetBoneLeftHand = HumanBodyBones.LeftIndexDistal;
        private HumanBodyBones targetBoneRightHand = HumanBodyBones.RightIndexDistal;
        private HumanBodyBones targetBoneLeftFoot = HumanBodyBones.LeftFoot;
        private HumanBodyBones targetBoneRightFoot = HumanBodyBones.RightFoot;
        private float boneResetInterval = 5.0f;
        private float boneResetCounter = 5.0f;
        private VRCPlayerApi _localPlayer;

        void Start()
        {
            if (SwitchWithHands)
            {
                if (!LeftHandObject) { Debug.LogError($"[Yodo]触觉控制器缺少左手对象 [{this.name}]"); }
                if (!RightHandObject) { Debug.LogError($"[Yodo]触觉控制器缺少右手对象 [{this.name}]"); }
            }
            if (SwitchWithFoots)
            {
                if (!LeftFootObject) { Debug.LogError($"[Yodo]触觉控制器缺少左脚对象 [{this.name}]"); }
                if (!RightFootObject) { Debug.LogError($"[Yodo]触觉控制器缺少右脚对象 [{this.name}]"); }
            }
            if (SwitchWithHead)
            {
                if (!HeadObject) { Debug.LogError($"[Yodo]触觉控制器缺少头部对象 [{this.name}]"); }
            }

            _localPlayer = Networking.LocalPlayer;
        }

        private void Update()
        {
            boneResetCounter -= Time.deltaTime;
            // 无法检测 Avatar 加载完成，所以每隔几秒强制刷新一次（虽然浪费但没办法，反正也不怎么耗性能）
            if (boneResetCounter < 0)
            {
                SetupLocalHandBones();
                boneResetCounter = boneResetInterval;
            }

            UpdateColliderPositions();
        }

        // 因为不同 Avatar 可能有或没有对应 Bone，所以查找最接近的手指 Bone。如果连手都没有就放弃。
        private void SetupLocalHandBones()
        {
            Vector3 noBone = new Vector3(0, 0, 0);  // 没有 Bone 时通常会取到原点坐标，所以检测是否为原点来判断 Bone 是否存在
            Vector3 newPos;
            HumanBodyBones newBone;

            if (SwitchWithHands)
            {
                // 左手
                newBone = HumanBodyBones.RightIndexDistal;
                newPos = Networking.LocalPlayer.GetBonePosition(newBone);
                if (newPos == noBone)
                {
                    newBone = HumanBodyBones.RightIndexIntermediate;
                    newPos = Networking.LocalPlayer.GetBonePosition(newBone);
                    if (newPos == noBone)
                    {
                        newBone = HumanBodyBones.RightIndexProximal;
                        newPos = Networking.LocalPlayer.GetBonePosition(newBone);
                        if (newPos == noBone)
                        {
                            newBone = HumanBodyBones.RightHand;
                        }
                    }
                }
                targetBoneRightHand = newBone;

                // 右手
                newBone = HumanBodyBones.LeftIndexDistal;
                newPos = Networking.LocalPlayer.GetBonePosition(newBone);
                if (newPos == noBone)
                {
                    newBone = HumanBodyBones.LeftIndexIntermediate;
                    newPos = Networking.LocalPlayer.GetBonePosition(newBone);
                    if (newPos == noBone)
                    {
                        newBone = HumanBodyBones.LeftIndexProximal;
                        newPos = Networking.LocalPlayer.GetBonePosition(newBone);
                        if (newPos == noBone)
                        {
                            newBone = HumanBodyBones.LeftHand;
                        }
                    }
                }
                targetBoneLeftHand = newBone;
            }
        }

        private void UpdateColliderPositions()
        {
            if(_localPlayer == null) { return; }
            if (!_localPlayer.IsValid()) { return; }    // ワールド退出時のエラー回避

            if(SwitchWithHands)
            {
                if(LeftHandObject)
                {
                    LeftHandObject.transform.position = _localPlayer.GetBonePosition(targetBoneLeftHand);
                }
                if(RightHandObject)
                {
                    RightHandObject.transform.position = _localPlayer.GetBonePosition(targetBoneRightHand);
                }
            }
            if(SwitchWithFoots)
            {
                if(LeftFootObject)
                {
                    LeftFootObject.transform.position = _localPlayer.GetBonePosition(targetBoneLeftFoot);
                }
                if(RightFootObject)
                {
                    RightFootObject.transform.position = _localPlayer.GetBonePosition(targetBoneRightFoot);
                }
            }
            if(SwitchWithHead)
            {
                if(HeadObject)
                {
                    HeadObject.transform.position = _localPlayer.GetBonePosition(HumanBodyBones.Head);
                }
            }
        }
    }
}
