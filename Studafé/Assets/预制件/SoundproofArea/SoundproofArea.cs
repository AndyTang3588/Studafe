
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class SoundproofArea : UdonSharpBehaviour
{
    [Header("【墙体穿透的语音音量】", order = 0)]
    [Header("从区域外→内时，声音可被听到的距离。0表示完全隔音。", order = 1)]
    [SerializeField, Min(0)] private float voiceDistanceOutsideToInside = 0.9f;
    [Header("从区域内→外时，声音可被听到的距离。0表示完全隔音。")]
    [SerializeField, Min(0)] private float voiceDistanceInsideToOutside = 0.9f;

    [Space(20, order = 0)]
    [Header("【正常情况下的语音音量】", order = 1)]
    [Header("从区域内→内时，声音可被听到的距离。推荐使用VRC默认值25。", order = 2)]
    [SerializeField, Min(0)] private float voiceDistanceInsideToInside = 25.0f;
    [Header("从区域外→外时，声音可被听到的距离。推荐使用VRC默认值25。")]
    [SerializeField, Min(0)] private float voiceDistanceOutsideToOutside = 25.0f;

    // [Space(20, order = 0)]
    // [Header("【墙体穿透的 Avatar Audio 调整】", order = 1)]
    // [Header("从区域外→内时，Avatar Audio 可被听到的距离。0表示完全隔音。", order = 2)]
    // [SerializeField, Min(0)] private float avatarAudioDistanceOutsideToInside = 0.25f;
    // [Header("从区域内→外时，Avatar Audio 可被听到的距离。0表示完全隔音。")]
    // [SerializeField, Min(0)] private float avatarAudioDistanceInsideToOutside = 0.25f;

    // [Space(20, order = 0)]
    // [Header("【正常情况下的 Avatar Audio 调整】", order = 1)]
    // [Header("从区域内→内时，Avatar Audio 可被听到的距离。VRC默认值为40。", order = 2)]
    // [SerializeField, Min(0)] private float avatarAudioDistanceInsideToInside = 40f;
    // [Header("从区域外→外时，Avatar Audio 可被听到的距离。VRC默认值为40。")]
    // [SerializeField, Min(0)] private float avatarAudioDistanceOutsideToOutside = 40f;

    [Space(20, order = 0)]
    [Header("【其他 / 高级设置】", order = 1)]
    [Header("更新频率。推荐值为5。", order = 2)]
    [Header("移动端或大量玩家等极限降低负载时可提高该值。", order = 3)]
    [SerializeField, Range(1, 30)] private int updateEveryNFrame = 5;

    private VRCPlayerApi[] players = new VRCPlayerApi[100];

    void Start()
    {
        RefreshTimerCallback();
    }

    public void RefreshTimerCallback()
    {
        if (Networking.LocalPlayer == null) return; // Prevent errors when the local player leaves
        UpdateAllPlayersVoiceDistance();
        SendCustomEventDelayedFrames(nameof(RefreshTimerCallback), updateEveryNFrame);
    }

    private void UpdateAllPlayersVoiceDistance()
    {
        VRCPlayerApi.GetPlayers(players);
        int playerCount = VRCPlayerApi.GetPlayerCount();
        bool isLocalPlayerInside = IsThisPlayerInside(Networking.LocalPlayer);
        for (int i = 0; i < playerCount; i++)
        {
            if (players[i] == Networking.LocalPlayer) continue;
            bool isRemotePlayerInside = IsThisPlayerInside(players[i]);
            // https://creators.vrchat.com/worlds/udon/players/player-audio
            players[i].SetVoiceDistanceFar(GetVoiceDistance(isLocalPlayerInside, isRemotePlayerInside));
        }
    }

    private float GetVoiceDistance(bool isLocalPlayerInside, bool isRemotePlayerInside)
    {
        if (isLocalPlayerInside) {
            if (isRemotePlayerInside) {
                return voiceDistanceInsideToInside;
            } else {
                return voiceDistanceOutsideToInside;
            }
        } else {
            if (isRemotePlayerInside) {
                return voiceDistanceInsideToOutside;
            } else {
                return voiceDistanceOutsideToOutside;
            }
        }
    }

    private bool IsThisPlayerInside(VRCPlayerApi player)
    {
        return GetComponent<Collider>().bounds.Contains(player.GetPosition());
    }
}
