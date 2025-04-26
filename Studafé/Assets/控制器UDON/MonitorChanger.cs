using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MonitorChanger : UdonSharpBehaviour
{
    public GameObject[] objects;  // **信号源**
    public GameObject[] labels;   // **提示框**
    public GameObject[] speakers; // **扬声器**

    public float labelDuration = 2.0f; // **提示显示时长**

    private int indice = -1; // 当前索引
    private float labelHideTime = 0f;
    private Vector3[] speakersInitPos; // **记录扬声器初始位置**

    private void Start()
    {
        // 隐藏所有 **speakers**，并记录初始位置，X +100 隐藏
        speakersInitPos = new Vector3[speakers.Length];
        for (int i = 0; i < speakers.Length; i++)
        {
            if (speakers[i] != null)
            {
                speakersInitPos[i] = speakers[i].transform.localPosition;
                speakers[i].transform.localPosition = new Vector3(speakersInitPos[i].x + 100, speakersInitPos[i].y, speakersInitPos[i].z);
            }
        }
        // 隐藏所有 **objects** 与 **labels**
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null) objects[i].SetActive(false);
        }
        for (int i = 0; i < labels.Length; i++)
        {
            if (labels[i] != null) labels[i].SetActive(false);
        }
        // 初始时自动递增，避免手动开电视
        indice++;
        UpdateAll();
    }

    public override void Interact()
    {
        // 循环更新 **indice**
        if (indice >= objects.Length - 1) indice = 0;
        else indice++;
        UpdateAll();
        // 显示当前 **标签** 并设置隐藏时间
        if (indice < labels.Length && labels[indice] != null)
        {
            labels[indice].SetActive(true);
            labelHideTime = Time.time + labelDuration;
        }
    }

    private void Update()
    {
        if (Time.time >= labelHideTime) HideLabels();
    }

    private void UpdateAll()
    {
        UpdateObjects();
        UpdateLabels();
        //MoveSpeakers();
        UpdateSpeakers();
    }

    // 只显示当前信号源
    private void UpdateObjects()
    {
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
                objects[i].SetActive(i == indice);
        }
    }

    // 隐藏除当前外的所有标签
    private void UpdateLabels()
    {
        for (int i = 0; i < labels.Length; i++)
        {
            if (labels[i] != null && i != indice)
                labels[i].SetActive(false);
        }
    }

    // 当前扬声器恢复原位置，其余 X 坐标 +100 隐藏
    private void MoveSpeakers()
    {
        for (int i = 0; i < speakers.Length; i++)
        {
            if (speakers[i] != null)
            {
                if (i == indice)
                    speakers[i].transform.localPosition = speakersInitPos[i];
                else
                    speakers[i].transform.localPosition = new Vector3(speakersInitPos[i].x + 100, speakersInitPos[i].y, speakersInitPos[i].z);
            }
        }
    }

    private void UpdateSpeakers()
    {
        for (int i = 0; i < speakers.Length; i++)
        {
            if (speakers[i] != null)
            {
                // 当前索引的喇叭要显示，其他的隐藏
                speakers[i].SetActive(i == indice);
            }
        }
    }

    private void HideLabels()
    {
        for (int i = 0; i < labels.Length; i++)
        {
            if (labels[i] != null)
                labels[i].SetActive(false);
        }
    }
}