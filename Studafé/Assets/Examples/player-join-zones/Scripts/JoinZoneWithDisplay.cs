using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

public class JoinZoneWithDisplay : PlayerJoinZone
{
    // UI Fields
    [Header("UI Fields")]
    [SerializeField] private TextMeshProUGUI _playerNamesField;
    [SerializeField] private TextMeshProUGUI _buttonLabelField;
    [SerializeField] private TextMeshProUGUI _ownerField;
    [SerializeField] private Button _toggleButton;
    
    // Durations
    [Header("Game Settings")]
    [SerializeField] private float _waitDuration = 3;
    
    // Strings
    [Header("Messages")]
    [SerializeField] private string _messageNoPlayers = "JOIN: Waiting for Players";
    [SerializeField] private string _messageStart = "JOIN: Start Game";
    [SerializeField] private string _messageWait = "WAIT: Starting game...";
    [SerializeField] private string _messageFinishGame = "GAME: Finish Game";
    [SerializeField] private string _messageReset = "END: Reset";
    
    // Extra Mode for this Class
    protected const int MODE_WAIT = 3;
    
    // PlayerNames
    [UdonSynced, FieldChangeCallback(nameof(PlayerNamesString))]
    private string _playerNamesString = "";
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
            _buttonLabelField.text = _messageStart;
        }
        else
        {
            _toggleButton.interactable = false;
            _buttonLabelField.text = _messageNoPlayers;
        }
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

    public override void OnPlayersChanged()
    {
        base.OnPlayersChanged(); // Sends event to all targets
        
        PlayerNamesString = GetPlayersAsStringList();
        
        // Serialize changes to playerNamesString
        RequestSerialization();
    }

    // Override the game logic to include our extra state
    public override void ToggleModeRPC()
    {
        Debug.Log($"Heard ToggleModeRPC with current mode {Mode}");
        switch (Mode)
        {
            case MODE_JOIN:
                Mode = MODE_WAIT;
                break;
            case MODE_WAIT:
                // do nothing, we just wait for timeout here
                break;
            case MODE_GAME:
                Mode = MODE_END;
                break;
            case MODE_END:
                Mode = MODE_JOIN;
                break;
        }
        RequestSerialization();
    }
    
    // Convenience method for Delayed call
    public void SetModeToGame()
    {
        Mode = MODE_GAME;
        RequestSerialization();
    }

    public override void OnModeChanged()
    {
        base.OnModeChanged(); // Sends event to all targets

        // most modes restore the button's interactivity
        _toggleButton.interactable = true;
        switch (Mode)
        {
            case MODE_JOIN:
                // label will be handled by number of players
                break;
            case MODE_WAIT:
                // Deactivate button during wait, update messages
                _toggleButton.interactable = false;
                _buttonLabelField.text = _messageWait;
                // Switch to Game mode after _waitDuration seconds
                SendCustomEventDelayedSeconds(nameof(SetModeToGame), _waitDuration);
                break;
            case MODE_GAME:
                _buttonLabelField.text = _messageFinishGame;
                break;
            case MODE_END:
                _buttonLabelField.text = _messageReset;
                break;
        }
    }
}