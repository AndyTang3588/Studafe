using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

public class SimpleResetButton : UdonSharpBehaviour
{
    [Header("复位设置")]
    [Tooltip("需要复位的物体列表（必须挂载Resettable脚本）")]
    public Resettable[] resetTargets;

    [Header("交互设置")]
    [Tooltip("按钮冷却时间（秒）")]
    public float cooldown = 0f;
    [Tooltip("启用网络同步")]
    public bool useNetworkSync = true;

    private float lastResetTime;
    private bool isCoolingDown;

    private void Update()
    {
        // 冷却状态更新
        if (isCoolingDown && Time.time - lastResetTime >= cooldown)
        {
            isCoolingDown = false;
        }
    }

    public override void Interact()
    {
        if (isCoolingDown) return;

        StartResetProcess();
    }

    private void StartResetProcess()
    {
        lastResetTime = Time.time;
        isCoolingDown = true;

        // 本地优先执行
        ExecuteReset();

        // 网络同步
        if (useNetworkSync && Networking.IsOwner(gameObject))
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(NetworkReset));
        }
    }

    public void NetworkReset()
    {
        if (!useNetworkSync) return;
        ExecuteReset();
    }

    private void ExecuteReset()
    {
        foreach (var target in resetTargets)
        {
            if (target != null)
            {
                target.ResetObject();
            }
            else
            {
                Debug.LogWarning("[复位按钮] 检测到空引用目标");
            }
        }
    }

    // 手动触发复位（编辑器测试用）
    public void ForceReset()
    {
        ExecuteReset();
        Debug.Log("[复位按钮] 强制复位完成");
    }
}