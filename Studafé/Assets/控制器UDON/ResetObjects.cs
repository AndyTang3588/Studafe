using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ResetObjects : UdonSharpBehaviour
{
    [Header("需要重置的物体")]
    public GameObject[] objectsToReset;

    private Vector3[] initialPositions;
    private Quaternion[] initialRotations;

    void Start()
    {
        int count = objectsToReset.Length;
        initialPositions = new Vector3[count];
        initialRotations = new Quaternion[count];

        for (int i = 0; i < count; i++)
        {
            if (objectsToReset[i] != null)
            {
                initialPositions[i] = objectsToReset[i].transform.position;
                initialRotations[i] = objectsToReset[i].transform.rotation;
            }
        }
    }

    public override void Interact()
    {
        for (int i = 0; i < objectsToReset.Length; i++)
        {
            if (objectsToReset[i] != null)
            {
                objectsToReset[i].transform.position = initialPositions[i];
                objectsToReset[i].transform.rotation = initialRotations[i];
            }
        }
    }
}
