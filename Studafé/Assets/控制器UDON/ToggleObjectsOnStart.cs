using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ToggleObjectsOnStart : UdonSharpBehaviour
{
    [Header("开场自动启用的物体")]
    [SerializeField] private GameObject[] objectsToEnable;

    [Header("开场自动禁用的物体")]
    [SerializeField] private GameObject[] objectsToDisable;

    private void Start()
    {
        // 启用第一个数组里的物体
        for (int i = 0; i < objectsToEnable.Length; i++)
        {
            if (objectsToEnable[i] != null)
            {
                objectsToEnable[i].SetActive(true);
            }
        }

        // 禁用第二个数组里的物体
        for (int i = 0; i < objectsToDisable.Length; i++)
        {
            if (objectsToDisable[i] != null)
            {
                objectsToDisable[i].SetActive(false);
            }
        }
    }
}
