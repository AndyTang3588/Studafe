// 作成者:dimebag29 作成日:2023年7月27日
// このプログラムのライセンスはCC0 (クリエイティブ・コモンズ・ゼロ)です。いかなる権利も保有しません。
// This program is licensed to CC0.

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]                           // Udonの同期モードを手動に設定

public class FpsViewer2_PickupUse_Global : UdonSharpBehaviour
{
    [UdonSynced(UdonSyncMode.None)] private bool SyncData = false;          // FpsGraphのアクティブ状態を決める同期変数

    public GameObject FpsGraph;                                             // Inspector画面で指定してもらうFpsGraphオブジェクト

    void Start()
    {
        SendCustomEventDelayedSeconds(nameof(UpdateFpsGraphActive), 2.0f);  // 2秒後にその時の同期変数をもとにFpsGraphのアクティブ状態を更新
    }

    // 自分がFpsViewer2を持った状態でUSEした時に、自分のFpsViewer2_PickupUse_Global_Trigger.csから呼ばれるメソッド
    public void OnPickupUseDownEvent()
    {
        Networking.SetOwner(Networking.LocalPlayer, this.gameObject);       // このオブジェクトのオーナーになる
        SyncData = !SyncData;                                               // 同期変数を切り替え
        RequestSerialization(); UpdateFpsGraphActive();                     // 同期変数更新を非オーナーに通知 & オーナー自身はFpsGraphのアクティブ状態更新
    }

    // オーナーが同期変数を更新した後に、オーナー以外全員が実行する
    public override void OnDeserialization()
    {
        UpdateFpsGraphActive();                                             // FpsGraphのアクティブ状態更新
    }

    // FpsGraphのアクティブ状態更新メソッド
    public void UpdateFpsGraphActive()
    {
        // 同期変数をもとにFpsGraphのアクティブ状態
        if (true == SyncData)
        {
            FpsGraph.SetActive(true);
        }
        else
        {
            FpsGraph.SetActive(false);
        }
    }
}

