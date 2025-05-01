using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AreaOnOff : UdonSharpBehaviour
{
    [Header("Objects to Show")]
    [Tooltip("The objects you want to show when player touches the cube")]
    [SerializeField]
    private GameObject[] objectsToShow;

    private void Start()
    {
        // Ensure objects are hidden at start
        foreach (GameObject obj in objectsToShow)
        {
            obj.SetActive(false);
        }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        // Check if the player is the local player
        if (player.isLocal)
        {
            foreach (GameObject obj in objectsToShow)
            {
                obj.SetActive(true);
            }
        }
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        // Check if the player is the local player
        if (player.isLocal)
        {
            foreach (GameObject obj in objectsToShow)
            {
                obj.SetActive(false);
            }
        }
    }
}
