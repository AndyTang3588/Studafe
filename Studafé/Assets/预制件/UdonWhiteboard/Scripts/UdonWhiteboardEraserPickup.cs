
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class UdonWhiteboardEraserPickup : UdonSharpBehaviour
{
    [SerializeField] private UdonWhiteboard settings = null;
    void Start()
    {
        
    }
    public override void OnPickup()
    {
        settings.PickupEraser();
    }
    public override void OnPickupUseDown()
    {
        settings.ClearScreenAll();
    }
    public override void OnDrop()
    {
        settings.DropEraser();
    }
}
