using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class MuteOthers : UdonSharpBehaviour
{
    // UI Fields
    [Header("UI Fields")]
    [SerializeField] private TextMeshProUGUI _buttonLabel;
    
    // Messages
    [Header("Messages")]
    [SerializeField] private string _messageMuted;
    [SerializeField] private string _messageUnmuted;
    
    private bool _areOtherPlayersMuted;

    // Voice Properties
    [Header("Voice Properties")] [SerializeField]
    private float _defaultVoiceDistance = 25f;
    
    // Set up the scene
    void Start()
    {
        OnMuteUpdated();
    }
    
    public void _Trigger()
    {
        _areOtherPlayersMuted = !_areOtherPlayersMuted;
        OnMuteUpdated();
    }

    private void OnMuteUpdated()
    {
        var players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];  
        VRCPlayerApi.GetPlayers(players);

        // Change the distance that other player voices will travel
        foreach (var player in players)
        {
            player.SetVoiceDistanceFar(_areOtherPlayersMuted ? 0 : _defaultVoiceDistance);
        }

        // Set button label
        if (Utilities.IsValid(_buttonLabel))
        {
            _buttonLabel.text = _areOtherPlayersMuted ? _messageMuted : _messageUnmuted;
        }
    }
}
