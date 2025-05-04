
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class UdonWhiteboardEraser : UdonSharpBehaviour
{
    [SerializeField] private UdonWhiteboard settings = null;
    [SerializeField] private GameObject whiteboardObject = null;
    [SerializeField] private GameObject particleObject = null;
    private bool _isOn = false;
    public bool IsOn
    {
        set
        {
            if (_isOn == value) return;
            _isOn = value;
            if (value == true) {
                MoveTrail();
            }
            particleObject.SetActive(value);
        }
        get => _isOn;
    }
    void Start()
    {
        
    }
    void Update()
    {
        if (IsOn == false) return;
        if (settings.IsLocked == true) return;
        MoveTrail();
    }
    public void MoveTrail()
    {
        Vector3 trailPosition = whiteboardObject.transform.parent.InverseTransformPoint(transform.position);
        trailPosition.z = 0;
        particleObject.transform.localPosition = trailPosition;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (other.gameObject == null) return;
        if (settings.IsLocked) return;
        if (other.gameObject == whiteboardObject) {
            IsOn = true;
            return;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other == null) return;
        if (other.gameObject == null) return;
        if (settings.IsLocked) return;
        if (other.gameObject == whiteboardObject) {
            IsOn = false;
        }
    }
}
