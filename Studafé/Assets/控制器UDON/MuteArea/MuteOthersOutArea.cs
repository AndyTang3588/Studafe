using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class MuteOthersOutArea : UdonSharpBehaviour
{
    // UI Fields
    [Header("UI Fields")]
    [SerializeField] private TextMeshProUGUI _buttonLabel;

    // Messages
    [Header("Messages")]
    [SerializeField] private string _messageMuted;
    [SerializeField] private string _messageUnmuted;

    // Voice Properties
    [Header("Voice Properties")]
    [SerializeField] private float _defaultVoiceDistance = 25f;

    private bool _areOtherPlayersMuted;

    void Start()
    {
        OnMuteUpdated(new VRCPlayerApi[0]); // 初始化时先不静音任何人
    }
    //public void _Trigger(){
        //_areOtherPlayersMuted = !_areOtherPlayersMuted;
        //OnMuteUpdated(GetPlayersInZone()); // 重新计算静音列表
    //}

    public void OnMuteUpdated(VRCPlayerApi[] playersInZone)
    {
        VRCPlayerApi[] allPlayers = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
        VRCPlayerApi.GetPlayers(allPlayers);
        
        string mutedPlayers = "";

        foreach (VRCPlayerApi player in allPlayers)
        {
            if (player == null) continue;

            bool isInside = false;
            foreach (VRCPlayerApi inZonePlayer in playersInZone)
            {
                if (player == inZonePlayer)
                {
                    isInside = true;
                    break;
                }
            }

            if (!isInside && _areOtherPlayersMuted)
            {
                player.SetVoiceDistanceFar(0); // 静音
                mutedPlayers += player.displayName + "\n";
            }
            else
            {
                player.SetVoiceDistanceFar(_defaultVoiceDistance); // 取消静音
            }
        }

        // 更新 UI 按钮文字
        if (Utilities.IsValid(_buttonLabel))
        {
            _buttonLabel.text = _areOtherPlayersMuted ? _messageMuted : _messageUnmuted;
        }
    }
    // 这个方法需要 `JoinZoneWithMute` 调用
    public void UpdateMutedPlayersList(VRCPlayerApi[] playersInZone)
    {
        OnMuteUpdated(playersInZone);
    }
}
