// 作成者:dimebag29 作成日:2023年7月27日
// このプログラムのライセンスはCC0 (クリエイティブ・コモンズ・ゼロ)です。いかなる権利も保有しません。
// This program is licensed to CC0.

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]                                         // Udonの同期モードを無しに設定

public class FpsViewer2_PickupUse_Local : UdonSharpBehaviour
{
    public GameObject FpsGraph, Message;                                                // Inspector画面で指定してもらうオブジェクト

    void Start()                           {Message.SetActive(false);                }  // 初期状態は必ずメッセージ非表示
    public override void OnPickup()        {Message.SetActive(true);                 }  // 持たれたらメッセージ表示
    public override void OnPickupUseDown() {FpsGraph.SetActive(!FpsGraph.activeSelf);}  // 持ちながらUSEされたらFpsGraphのアクティブ状態を切り替え
    public override void OnDrop()          {Message.SetActive(false);                }  // 離されたらメッセージ非表示


}
