
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ClockGem : UdonSharpBehaviour {
    [SerializeField] private SkinnedMeshRenderer meshRenderer;
    [SerializeField] private string paramName = "_Num";

    void Update() {
        System.DateTime now = System.DateTime.Now;
        meshRenderer.material.SetFloat(paramName, now.Hour * 100 + now.Minute);
    }
}
