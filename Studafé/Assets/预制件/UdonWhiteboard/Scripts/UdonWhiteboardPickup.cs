
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class UdonWhiteboardPickup : UdonSharpBehaviour
{
    [SerializeField] private UdonWhiteboard settings = null;
    void Start()
    {
        
    }
    public override void OnPickup()
    {
        settings.PickupBoard();
    }
    public override void OnDrop()
    {
        settings.DropBoard();
    }
}
