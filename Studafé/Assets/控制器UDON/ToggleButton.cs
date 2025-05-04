using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class ToggleButton : UdonSharpBehaviour
{
    [SerializeField] private Transform target;      // 要控制的目标物体
    [SerializeField] private float offset = 0.15f;  // 升降的高度差

    // 原始的 Y 坐标（假设场景初始时物体在原位）
    private float originalY;
    // 记录是否已下降（同步变量）
    [UdonSynced] private bool isLowered = false;

    void Start()
    {
        // 在脚本启动时记录原始 Y 坐标（场景初始状态）
        if (target != null)
        {
            originalY = target.position.y;
            // 若脚本所在物体拥有所有权，可直接应用一次当前同步状态
            // 但实际首个状态为 isLowered=false 时 target 已在原位，无需额外处理
        }
    }

    public override void Interact()
    {
        if (target == null) return;

        // 将目标物体的拥有者设置为当前玩家
        Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        // 切换状态
        isLowered = !isLowered;
        // 根据新状态立即更新本地物体位置
        UpdateTargetPosition();
        // 手动请求同步，使其他玩家收到更新后的 isLowered 值
        RequestSerialization();
    }

    public override void OnDeserialization()
    {
        // 当同步数据更新或新玩家加入时，根据 isLowered 值更新目标物体位置
        UpdateTargetPosition();
    }

    private void UpdateTargetPosition()
    {
        Vector3 pos = target.position;
        if (isLowered)
        {
            // 如果应处于下降状态，则设置到原始高度减去偏移量
            pos.y = originalY - offset;
        }
        else
        {
            // 否则恢复到原始高度
            pos.y = originalY;
        }
        target.position = pos;
    }
}
