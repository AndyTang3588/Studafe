using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class BottleTossGame : UdonSharpBehaviour {
    public Transform bottle;           // 拖入瓶子的 Transform
    public AudioSource winAudio;       // 拖入音频
    public GameObject effectObj;       // 拖入粒子特效
    public float duration = 2f;        // 粒子持续时间

    private float lastAngle;           // 记录上一次的X轴角度
    private float stableTimer = 0f;    // 计算稳定时间
    private bool hasWon = false;       // 防止多次触发胜利

    void Update() {
        if (hasWon) return; // 避免重复触发胜利

        float xRotation = bottle.eulerAngles.x;
        if (xRotation > 180f) xRotation -= 360f; // 处理Unity欧拉角的问题（-180° 到 180°）

        float angleDiff = Mathf.Abs(xRotation - lastAngle);
        lastAngle = xRotation;

        // 角度在立正范围 & 旋转变化很小
        if (xRotation > -92f && xRotation < -88f) {
            if (angleDiff < 0.05f) { // 旋转变化接近 0，表示稳定
                stableTimer += Time.deltaTime;
                if (stableTimer >= 0.3f) { // 持续 0.3 秒
                    OnPlayerWin();
                }
            } else {
                stableTimer = 0f; // 变化大，重置计时
            }
        } else {
            stableTimer = 0f; // 角度超出范围，重置计时
        }
    }

    public void OnPlayerWin() {
        hasWon = true; // 防止重复触发
        winAudio.Play();
        effectObj.SetActive(true);
        SendCustomEventDelayedSeconds(nameof(DeactivateEffect), duration);
    }

    public void DeactivateEffect() {
        effectObj.SetActive(false);
    }
}
