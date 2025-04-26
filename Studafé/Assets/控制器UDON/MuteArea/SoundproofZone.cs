using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using System.Text;

public class SoundproofZone : UdonSharpBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI _mutedList;
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private float _voiceDistance = 15f;

    [Header("Debug")]
    [SerializeField] private bool _debugMode = true;

    // 使用固定大小数组代替List
    private int[] _playerIds = new int[9999];
    private int _playerCount;
    private bool _isLocalInside;

    private void Start()
    {
        // 正确初始化方式
        _playerCount = 0;
        _isLocalInside = false;
        UpdateSystemState(true);
        Log("系统初始化完成");
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (!player.IsValid()) return;

        if (player.isLocal)
        {
            if (!_isLocalInside)
            {
                _isLocalInside = true;
                Log("本地玩家进入区域");
                UpdateSystemState();
            }
        }
        else
        {
            AddPlayer(player.playerId);
        }
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (!player.IsValid()) return;

        if (player.isLocal)
        {
            if (_isLocalInside)
            {
                _isLocalInside = false;
                Log("本地玩家离开区域");
                UpdateSystemState();
            }
        }
        else
        {
            RemovePlayer(player.playerId);
        }
    }

    // 自定义数组操作方法
    private void AddPlayer(int id)
    {
        if (_playerCount >= _playerIds.Length) return;

        // 检查是否已存在
        for (int i = 0; i < _playerCount; i++)
        {
            if (_playerIds[i] == id) return;
        }

        _playerIds[_playerCount++] = id;
        Log($"添加玩家ID: {id}");
        UpdateSystemState();
    }

    private void RemovePlayer(int id)
    {
        for (int i = 0; i < _playerCount; i++)
        {
            if (_playerIds[i] == id)
            {
                // 将最后一个元素移到当前位置
                _playerIds[i] = _playerIds[--_playerCount];
                Log($"移除玩家ID: {id}");
                UpdateSystemState();
                return;
            }
        }
    }

    private void UpdateSystemState(bool forceUpdate = false)
    {
        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (!localPlayer.IsValid()) return;

        VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
        VRCPlayerApi.GetPlayers(players);

        StringBuilder mutedBuilder = new StringBuilder();
        int muteCount = 0;

        foreach (var player in players)
        {
            if (!player.IsValid() || player == localPlayer) continue;

            bool shouldHear = ShouldHearPlayer(player);
            float targetDistance = shouldHear ? _voiceDistance : 0f;

            if (forceUpdate || !Mathf.Approximately(player.GetVoiceDistanceFar(), targetDistance))
            {
                player.SetVoiceDistanceFar(targetDistance);
                Log($"设置 {player.displayName} 语音距离: {targetDistance}");
            }

            if (!shouldHear)
            {
                if (muteCount++ > 0) mutedBuilder.Append(", ");
                mutedBuilder.Append(player.displayName);
            }
        }

        UpdateUI(mutedBuilder.ToString());
    }

    private bool ShouldHearPlayer(VRCPlayerApi player)
    {
        if (!_isLocalInside) return true;

        for (int i = 0; i < _playerCount; i++)
        {
            if (_playerIds[i] == player.playerId)
                return true;
        }
        return false;
    }

    private void UpdateUI(string mutedNames)
    {
        if (_mutedList != null)
        {
            _mutedList.text = _isLocalInside 
                ? (string.IsNullOrEmpty(mutedNames) ? "无屏蔽玩家" : $"已屏蔽:\n{mutedNames}") 
                : "功能未激活";
        }

        if (_statusText != null)
        {
            _statusText.text = _isLocalInside 
                ? "🔇 隔音模式" 
                : "🔈 正常模式";
        }
    }

    private void Log(string message)
    {
        if (_debugMode) Debug.Log($"[隔音系统] {message}");
    }
}