using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using Random = UnityEngine.Random;

public class BossPicker : PlayerJoinZone
{
    // UI Fields
    [Header("UI Fields")]
    [SerializeField] private TextMeshProUGUI _playerNamesField;
    [SerializeField] private TextMeshProUGUI _labelField;
    [SerializeField] private TextMeshProUGUI _buttonLabelField;
    [SerializeField] private TextMeshProUGUI _ownerField;
    [SerializeField] private UnityEngine.UI.Button _toggleButton;
    
    // Game Settings
    [Header("Game Settings")]
    [SerializeField] private float _bossHeight = 5;
    [SerializeField] private float _maxPlayerHeight = 2;
    [SerializeField] private float _gameDuration = 5;
    
    // Strings
    [Header("Messages")]
    private string _buttonLabelNoPlayers = "JOIN: Waiting for Players";
    private string _buttonLabelEnd = "END: Reset";
    private string _buttonLabelJoin = "JOIN: Start Game";
    private string _buttonLabelGame = "GAME: Playing Game";
    private string _labelGame = "Boss:";
    private string _labelJoin = "Possible Bosses:";
    private string _labelEnd = "Final Score:";
    
    // Synced PlayerNames
    [UdonSynced, FieldChangeCallback(nameof(PlayerNamesString))]
    private string _playerNamesString;
    public string PlayerNamesString
    {
        set
        {
            _playerNamesString = value;
            
            // Show all the player names in a text field
            _playerNamesField.text = PlayerNamesString;
            SetupButtonFromPlayers();
        }
        get => _playerNamesString;
    }

    // Synced Boss PlayerId
    [UdonSynced, FieldChangeCallback(nameof(BossPlayerId))]
    private int _bossPlayerId = -1;

    public int BossPlayerId
    {
        set
        {
            _bossPlayerId = value;
            OnBossChanged();
        }
        get => _bossPlayerId;
    }

    public override void PostStart()
    {
        // Sets up button state and listing
        SetupButtonFromPlayers();
        
        SetOwnerName(Networking.GetOwner(gameObject));
    }
    
    private void SetupButtonFromPlayers()
    {
        if (PlayerNamesString.Length > 0)
        {
            _toggleButton.interactable = true;
            _buttonLabelField.text = _buttonLabelJoin;
        }
        else
        {
            _toggleButton.interactable = false;
            _buttonLabelField.text = _buttonLabelNoPlayers;
        }
    }
    
    private void OnBossChanged()
    {
        foreach (var target in targets)
        {
            target.SetProgramVariable(nameof(BossPlayerId), BossPlayerId);
        }
        
        var player = Networking.LocalPlayer;
        var bossPlayer = VRCPlayerApi.GetPlayerById(BossPlayerId);

        // Don't proceed with an invalid Boss Player
        if (!Utilities.IsValid(bossPlayer))
        {
            Debug.LogError($"Tried to set Boss to Invalid Player, exiting OnBossChanged early.");
            return;
        }

        _playerNamesField.text = $"{bossPlayer.displayName} {BossPlayerId}";
        
        if (_bossPlayerId == player.playerId)
        {
            // Set boss to _bossHeight
            player.SetAvatarEyeHeightByMultiplier(_bossHeight);
        }
        else
        {
            // Ensure no players are taller than _maxPlayerHeight
            var playerHeight = player.GetAvatarEyeHeightAsMeters();
            var newHeight = Math.Min(playerHeight, _maxPlayerHeight);
            player.SetAvatarEyeHeightByMeters(newHeight);
        }
        
        // Don't let anyone change their height manually
        player.SetManualAvatarScalingAllowed(false);
    }

    public override void OnPlayersChanged()
    {
        base.OnPlayersChanged(); // sends event to all targets
        
        PlayerNamesString = GetPlayersAsStringList();
        
        // Serialize changes to playerNamesString
        RequestSerialization();
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        base.OnOwnershipTransferred(player);
        SetOwnerName(player);
    }

    private void SetOwnerName(VRCPlayerApi player)
    {
        // Set displayName or "Me" to make it easy to know whether you're the owner
        string displayName = player.isLocal ? "Me" : player.displayName;
        _ownerField.text = $"Owner:\n{displayName}";
    }

    // Convenience method for Delayed call
    public void SetModeToEnd()
    {
        Mode = MODE_END;
        RequestSerialization();
    }

    // Restores boss size after the user changes their avatar
    public override void OnAvatarChanged(VRCPlayerApi player)
    {
        // Ensure only the local player who is the boss continues
        if (!player.isLocal || BossPlayerId != Networking.LocalPlayer.playerId) return;
        
        // Set boss to _bossHeight
        player.SetAvatarEyeHeightByMultiplier(_bossHeight);
    }

    public override void OnModeChanged()
    {
        base.OnModeChanged();
        
        switch (Mode)
        {
            case MODE_JOIN:
                _buttonLabelField.text = _buttonLabelNoPlayers;
                _labelField.text = _labelJoin;
                
                // Resets player height to defaults
                Networking.LocalPlayer.SetAvatarEyeHeightByMultiplier(1);
                break;
            
            case MODE_GAME:
                // Deactivate button during game, update messages
                _toggleButton.interactable = false;
                _buttonLabelField.text = _buttonLabelGame;
                _labelField.text = _labelGame;
                
                // Have Owner choose one player randomly
                if (Networking.IsOwner(gameObject))
                {
                    int randomPlayerIndex = Random.Range(0, Players.Count);
                    var boss = (VRCPlayerApi)Players[randomPlayerIndex].Reference;

                    if (!Utilities.IsValid(boss))
                    {
                        Debug.LogError($"Got invalid player as Boss, this shouldn't happen! Things may broken");
                        return;
                    }

                    // Serialize new Boss ID to all other players
                    BossPlayerId = boss.playerId;
                    RequestSerialization();

                    // Switch to Gameover mode after _gameDuration seconds
                    SendCustomEventDelayedSeconds(nameof(SetModeToEnd), _gameDuration);
                }
                break;
            
            case MODE_END:
                _toggleButton.interactable = true;
                _buttonLabelField.text = _buttonLabelEnd;
                _labelField.text = _labelEnd;
                // Set locally by each player, so it won't match - just a demo!
                _playerNamesField.text = $"{Random.Range(0, 100)}";
                
                break;
        }
    }
}