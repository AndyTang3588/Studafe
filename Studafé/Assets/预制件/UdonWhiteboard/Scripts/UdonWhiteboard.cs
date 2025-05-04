
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class UdonWhiteboard : UdonSharpBehaviour
{
    [SerializeField] private Color[] markerColorValues = new Color[]{Color.black, Color.red, Color.blue};
    [SerializeField] private float[] markerSizeValues = new float[]{0.01f, 0.005f, 0.0025f};
    [SerializeField] private float eraserSizeValue = 0.03f;
    [SerializeField] private GameObject cameraObject = null;
    [SerializeField] private Mesh[] MarkerMeshes = null;
    [SerializeField] private Mesh LockButtonMeshOpen = null;
    [SerializeField] private Mesh LockButtonMeshClose = null;
    [SerializeField] private GameObject MarkerObject = null;
    [SerializeField] private GameObject EraserObject = null;
    [SerializeField] private GameObject MarkerDummyObject = null;
    [SerializeField] private GameObject EraserDummyObject = null;
    [SerializeField] private GameObject MarkerTriggerObject = null;
    [SerializeField] private GameObject EraserTriggerObject = null;
    [SerializeField] private GameObject buttonParent = null;
    [SerializeField] private GameObject[] colorButtons = null;
    [SerializeField] private GameObject[] sizeButtons = null;
    [SerializeField] private GameObject lockButton = null;
    [SerializeField] private GameObject screenBackground = null;
    [SerializeField] private ParticleSystem[] markerParticles = null;
    [SerializeField] private ParticleSystem[] eraserParticles = null;
    [UdonSynced] private bool BoardIsPicked = false;
    [UdonSynced] private bool MarkerIsPicked = false;
    [UdonSynced] private bool EraserIsPicked = false;
    [UdonSynced] private byte MarkerColor = 0;
    [UdonSynced] private byte MarkerSize = 0;
    [UdonSynced, HideInInspector] public bool IsLocked = false;

    private MeshFilter markerMeshFilter = null;
    private MeshFilter markerDummyMeshFilter = null;
    private MeshFilter lockButtonMeshFilter = null;
    private Renderer markerRenderer = null;
    private Renderer eraserRenderer = null;
    private UdonWhiteboardMarker marker = null;
    private UdonWhiteboardEraser eraser = null;

    void Start()
    {
    }
    void OnEnable()
    {
        if (markerMeshFilter == null) {
            markerMeshFilter = MarkerObject.GetComponent<MeshFilter>();
            markerRenderer = MarkerObject.GetComponent<Renderer>();
            eraserRenderer = EraserObject.GetComponent<Renderer>();
            marker = MarkerObject.GetComponentInChildren<UdonWhiteboardMarker>(true);
            eraser = EraserObject.GetComponentInChildren<UdonWhiteboardEraser>(true);
            markerDummyMeshFilter = MarkerDummyObject.GetComponent<MeshFilter>();
            lockButtonMeshFilter = lockButton.GetComponent<MeshFilter>();
            for(int i = 0;i < eraserParticles.Length;i++) {
                var main = eraserParticles[i].main;
                main.startSize = eraserSizeValue;
            }
            ClearScreen();
            SetMarker();
        } 
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(RequestSerialization_));
    }
    void Update()
    {
        if (Networking.IsOwner(MarkerObject) && !MarkerIsPicked && BoardIsPicked) {
            MarkerObject.transform.position = MarkerDummyObject.transform.position;
            MarkerObject.transform.rotation = MarkerDummyObject.transform.rotation;
        }
        if (Networking.IsOwner(EraserObject) && !EraserIsPicked && BoardIsPicked) {
            EraserObject.transform.position = EraserDummyObject.transform.position;
            EraserObject.transform.rotation = EraserDummyObject.transform.rotation;
        }
        if (!Networking.IsOwner(gameObject)) return;
        if (MarkerIsPicked) {
            VRCPlayerApi localPlayer = Networking.LocalPlayer;
            if (Utilities.IsValid(localPlayer)) {
                Vector3 direction = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position - buttonParent.transform.position;
                Quaternion buttonRotation = Quaternion.LookRotation(direction, Vector3.up);
                lockButton.transform.rotation = buttonRotation;
                for (int i = 0;i < sizeButtons.Length;i++) {
                    sizeButtons[i].transform.rotation = buttonRotation;
                }
            }
        }
    }
    public void PickupMarker()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        MarkerIsPicked = true;
        RequestSerialization();
        SetMarker();
    }
    public void DropMarker()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        MarkerIsPicked = false;
        RequestSerialization();
        SetMarker();
    }
    public void PickupEraser()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        EraserIsPicked = true;
        RequestSerialization();
        SetMarker();
    }
    public void DropEraser()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        EraserIsPicked = false;
        RequestSerialization();
        SetMarker();
    }
    public void PickupBoard()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        BoardIsPicked = true;
        RequestSerialization();
        SetMarker();
    }
    public void DropBoard()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        BoardIsPicked = false;
        RequestSerialization();
        SetMarker();
    }
    public void SetMarkerColor(int colorNum)
    {
        if (colorNum == MarkerColor) return;
        if (colorNum >= colorButtons.Length) return;
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        MarkerColor = (byte)colorNum;
        RequestSerialization();
        SetMarker();
    }
    public void SetMarkerSize(int sizeNum)
    {
        if (sizeNum == MarkerSize) return;
        if (sizeNum >= sizeButtons.Length) return;
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        MarkerSize = (byte)sizeNum;
        RequestSerialization();
        SetMarker();
    }
    public void ToggleLock()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        IsLocked = !IsLocked;
        RequestSerialization();
        SetMarker();
    }
    public void SetMarker()
    {
        int meshNum = (int)MarkerColor;
        if (MarkerIsPicked) {
            meshNum += 3;
        }
        markerMeshFilter.sharedMesh = MarkerMeshes[meshNum];
        markerDummyMeshFilter.sharedMesh = MarkerMeshes[MarkerColor];
        cameraObject.SetActive(!IsLocked);
        if (IsLocked) {
            lockButtonMeshFilter.sharedMesh = LockButtonMeshClose;
        } else {
            lockButtonMeshFilter.sharedMesh = LockButtonMeshOpen;
        }
        MarkerTriggerObject.SetActive(MarkerIsPicked);
        EraserTriggerObject.SetActive(EraserIsPicked);
        for(int i = 0;i < markerParticles.Length;i++) {
            var main = markerParticles[i].main;
            main.startColor = markerColorValues[MarkerColor];
            main.startSize = markerSizeValues[MarkerSize];
        }
        if (!MarkerIsPicked && BoardIsPicked) {
            markerRenderer.enabled = false;
            MarkerDummyObject.SetActive(true);
            marker.IsOn = false;
        } else {
            markerRenderer.enabled = true;
            MarkerDummyObject.SetActive(false);
        }
        if (!EraserIsPicked && BoardIsPicked) {
            eraserRenderer.enabled = false;
            EraserDummyObject.SetActive(true);
            eraser.IsOn = false;
        } else {
            eraserRenderer.enabled = true;
            EraserDummyObject.SetActive(false);
        }
        if (Networking.IsOwner(gameObject)) {
            buttonParent.SetActive(MarkerIsPicked);
            if(MarkerIsPicked) {
                for (int i = 0;i < colorButtons.Length;i++) {
                    Vector3 pos = colorButtons[i].transform.localPosition;
                    if (i == MarkerColor) {
                        pos.x = -0.015f;
                        pos.z = -0.015f;
                    } else {
                        pos.x = 0;
                        pos.z = 0;
                    }
                    colorButtons[i].transform.localPosition = pos;
                }
                for (int i = 0;i < sizeButtons.Length;i++) {
                    Vector3 pos = sizeButtons[i].transform.localPosition;
                    if (i == MarkerSize) {
                        pos.x = -0.015f;
                        pos.z = -0.015f;
                    } else {
                        pos.x = 0;
                        pos.z = 0;
                    }
                    sizeButtons[i].transform.localPosition = pos;
                }
            }
        }
    }
    public void ClearScreenAll()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ClearScreen));
    }
    public void ClearScreen()
    {
        screenBackground.SetActive(true);
        SendCustomEventDelayedSeconds(nameof(ClearScreen_), 1f);
    }
    public void ClearScreen_()
    {
        screenBackground.SetActive(false);
    }
    public override void OnDeserialization()
    {
        SetMarker();
    }
    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if(Networking.IsOwner(gameObject)){
            SendCustomEventDelayedSeconds(nameof(RequestSerialization_), 5f);
        }
    }

    public void RequestSerialization_()
    {
        RequestSerialization();
    }
}
