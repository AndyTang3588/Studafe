
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class UdonWhiteboardMarkerPickup : UdonSharpBehaviour
{
    [SerializeField] private UdonWhiteboard settings = null;
    void Start()
    {
        
    }
    public override void OnPickup()
    {
        settings.PickupMarker();
    }
    public override void OnDrop()
    {
        settings.DropMarker();
    }
}
