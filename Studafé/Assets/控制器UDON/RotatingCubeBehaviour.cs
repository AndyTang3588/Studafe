using UnityEngine;
using UdonSharp;

public class RotatingCubeBehaviour : UdonSharpBehaviour
{
    [SerializeField] private float rotationSpeed = 90f; // 角速度，可在 Inspector 面板调整
    [SerializeField] private Transform rotationTarget; // 目标物体，作为旋转中心

    private void Update()
    {
        Vector3 rotationPoint = rotationTarget ? rotationTarget.position : transform.position;
        
        // 绕目标物体的中心点旋转
        transform.RotateAround(rotationPoint, Vector3.up, rotationSpeed * Time.deltaTime);
    }
}