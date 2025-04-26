// 作成者:dimebag29 作成日:2023年7月27日
// このプログラムのライセンスはCC0 (クリエイティブ・コモンズ・ゼロ)です。いかなる権利も保有しません。
// This program is licensed to CC0.

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

// Udonの同期モードを無しに設定。
// VRC_ObjectSyncをつけたオブジェクトには手動同期(BehaviourSyncMode.Manual)のスクリプトを追加できないので、このスクリプトを噛ます
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]

public class FpsViewer2_PickupUse_Global_Trigger : UdonSharpBehaviour
{
    public UdonSharpBehaviour TargetUdon;                               // 情報を渡したいUdonBehaviourがついているオブジェクト
    public GameObject Message;                                          // ユーザーへのメッセージ。「持ちながらUSEでFPSグラフを表示」が書かれたCanvasオブジェクト

    void Start()
    {
        Message.SetActive(false);                                       // 初期状態は必ずメッセージ非表示
    }

    public override void OnPickup()
    {
        Message.SetActive(true);                                        // 持たれたらメッセージ表示
    }

    public override void OnDrop()
    {
        Message.SetActive(false);                                       // 離されたらメッセージ非表示
    }

    public override void OnPickupUseDown()
    {
        TargetUdon.SendCustomEvent("OnPickupUseDownEvent");             // 持ちながらUSEされたらTargetUdon内のメソッドを実行
    }

}
