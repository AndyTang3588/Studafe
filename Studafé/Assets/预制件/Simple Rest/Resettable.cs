using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Resettable : UdonSharpBehaviour
{
    [Header("复位设置")]
    [Tooltip("是否自动记录初始状态（建议保持启用）")]
    public bool autoRecord = true;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Rigidbody rb;
    private bool isInitialized;

    void Start()
    {
        if (autoRecord) RecordInitialState();
    }

    public void RecordInitialState()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
        isInitialized = true;
    }

    public void ResetObject()
    {
        if (!isInitialized)
        {
            Debug.LogWarning($"[Resettable] {name} 未初始化！");
            return;
        }

        // 获取所有碰撞体（包括子物体）
        Collider[] allColliders = GetComponentsInChildren<Collider>(true);
        bool[] colliderStates = new bool[allColliders.Length];

        // 记录并临时禁用所有碰撞体
        for (int i = 0; i < allColliders.Length; i++)
        {
            colliderStates[i] = allColliders[i].enabled;
            allColliders[i].enabled = false;
        }

        // 临时设置Rigidbody为运动学（避免物理干扰）
        bool wasKinematic = false;
        if (rb != null)
        {
            wasKinematic = rb.isKinematic;
            rb.isKinematic = true;
        }

        // 重置位置和旋转
        transform.SetPositionAndRotation(originalPosition, originalRotation);

        // 恢复物理状态
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = wasKinematic; // 恢复原始运动学状态
        }

        // 恢复碰撞体状态
        for (int i = 0; i < allColliders.Length; i++)
        {
            if (allColliders[i] != null) // 防止物体被销毁
            {
                allColliders[i].enabled = colliderStates[i];
            }
        }

        // 重置子物体状态
        foreach (Transform child in transform)
        {
            if (child) child.gameObject.SetActive(true);
        }
    }

    // 手动更新初始状态（可选调用）
    public void UpdateInitialState()
    {
        RecordInitialState();
        Debug.Log($"[Resettable] {name} 已更新初始状态");
    }
}