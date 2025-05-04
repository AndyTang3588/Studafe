using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class MonitorChanger : UdonSharpBehaviour
{
    [SerializeField] private GameObject[] objects;   
    [SerializeField] private GameObject[] labels;    
    [SerializeField] private GameObject[] speakers;  
    [SerializeField] private float labelDuration = 2f;

    [UdonSynced] private int syncedIndice = 0;     // **同步索引**

    private float labelHideTime = 0f;
    private Vector3[] speakersInitPos;

    void Start()
    {
        // 记录扬声器初始位置 & 隐藏
        speakersInitPos = new Vector3[speakers.Length];
        for (int i = 0; i < speakers.Length; i++)
        {
            speakersInitPos[i] = speakers[i].transform.localPosition;
            speakers[i].transform.localPosition += Vector3.right * 100f;
        }
        // 一进场按同步值更新一次
        UpdateAll();
    }

    public override void Interact()
    {
        // 转移拥有权
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        // 更新同步变量
        syncedIndice = (syncedIndice + 1) % objects.Length;
        // 本地立即更新
        UpdateAll();  
        // 发同步
        RequestSerialization();
        // 显示当前标签并设置隐藏时间
        if (syncedIndice < labels.Length)
        {
            labels[syncedIndice].SetActive(true);
            labelHideTime = Time.time + labelDuration;
        }
    }

    public override void OnDeserialization()
    {
        // 收到新同步值时刷新
        UpdateAll();
    }

    void Update()
    {
        if (Time.time >= labelHideTime)
        {
            for (int i = 0; i < labels.Length; i++)
                labels[i].SetActive(false);
        }
    }

    private void UpdateAll()
    {
        // objects 显示逻辑
        for (int i = 0; i < objects.Length; i++)
            objects[i].SetActive(i == syncedIndice);

        // labels 隐藏除当前外
        for (int i = 0; i < labels.Length; i++)
            if (i != syncedIndice) labels[i].SetActive(false);

        // speakers 只激活当前一个
        for (int i = 0; i < speakers.Length; i++)
        {
            if (i == syncedIndice)
                speakers[i].transform.localPosition = speakersInitPos[i];
            else
                speakers[i].transform.localPosition = speakersInitPos[i] + Vector3.right * 100f;
        }
    }
}
