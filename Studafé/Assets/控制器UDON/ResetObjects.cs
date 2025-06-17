using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class ResetObjects : UdonSharpBehaviour
{
    [Header("需要重置的物体")]
    [Tooltip("需要重置位置和旋转的物体列表")]
    public GameObject[] objectsToReset;

    [Header("重置选项")]
    [Tooltip("是否重置物体的物理状态（速度和角速度）")]
    public bool resetPhysics = true;
    [Tooltip("是否在重置时转移物体所有权")]
    public bool transferOwnership = true;

    private Vector3[] initialPositions;
    private Quaternion[] initialRotations;
    private bool isInitialized = false;

    void Start()
    {
        InitializeResetSystem();
    }

    void OnEnable()
    {
        if (!isInitialized)
        {
            InitializeResetSystem();
        }
    }

    private void InitializeResetSystem()
    {
        if (objectsToReset == null || objectsToReset.Length == 0)
        {
            Debug.LogError("[ResetObjects] 没有设置需要重置的物体！");
            return;
        }

        int count = objectsToReset.Length;
        initialPositions = new Vector3[count];
        initialRotations = new Quaternion[count];

        for (int i = 0; i < count; i++)
        {
            if (objectsToReset[i] != null)
            {
                initialPositions[i] = objectsToReset[i].transform.position;
                initialRotations[i] = objectsToReset[i].transform.rotation;
            }
            else
            {
                Debug.LogWarning($"[ResetObjects] 物体列表中的第 {i} 个物体为空！");
            }
        }

        isInitialized = true;
    }

    public override void Interact()
    {
        if (!isInitialized) return;

        // 通知所有客户端执行重置
        SendCustomNetworkEvent(NetworkEventTarget.All, nameof(ResetAllObjects));
    }

    public void ResetAllObjects()
    {
        if (!isInitialized) return;

        for (int i = 0; i < objectsToReset.Length; i++)
        {
            if (objectsToReset[i] == null) continue;

            // 转移所有权
            if (transferOwnership && Networking.LocalPlayer != null)
            {
                Networking.SetOwner(Networking.LocalPlayer, objectsToReset[i]);
            }

            // 重置物理状态
            if (resetPhysics)
            {
                Rigidbody rb = objectsToReset[i].GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }

            // 重置位置和旋转
            objectsToReset[i].transform.position = initialPositions[i];
            objectsToReset[i].transform.rotation = initialRotations[i];
        }
    }
}
