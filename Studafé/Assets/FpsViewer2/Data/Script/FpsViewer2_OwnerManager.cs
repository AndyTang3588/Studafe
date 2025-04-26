// 作成者:dimebag29 作成日:2023年7月27日
// このプログラムのライセンスはCC0 (クリエイティブ・コモンズ・ゼロ)です。いかなる権利も保有しません。
// This program is licensed to CC0.

// 変数SetOwnerDelayが使用されていません という警告を出さないようにする。(実際はSendCustomEventDelayedSecondsの引数で使っている)
#pragma warning disable 0414

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]                                   // Udonの同期モードを手動に設定

public class FpsViewer2_OwnerManager : UdonSharpBehaviour
{
    private float SetOwnerDelay = 2.0f;                                             // Join何秒後から同期処理を開始するか
    private float SetOwnerCycle = 2.0f;                                             // 何秒間隔でSetOwner()を実行するか

    private VRCPlayerApi MyInfo;                                                    // 自分自身のプレイヤー情報
    private VRCPlayerApi[] PlayersInfo;                                             // インスタンス内にいるプレイヤーの情報
    private GameObject[] FpsViewer;                                                 // 子オブジェクト
    private int FpsViewerNum = 0;                                                   // 子オブジェクトの数
    private int FpsViewerCount = 0;                                                 // 子オブジェクトの何個目か?


    void Start()
    {
        FpsViewerNum = this.gameObject.transform.childCount;                        // 子オブジェクトの数を数える
        FpsViewer    = new GameObject[FpsViewerNum];                                // 子オブジェクトの数分の箱を用意する

        // 子オブジェクトをFpsViewerに紐づけしていく
        for (int i = 0; i < FpsViewerNum; i++)
        {
            FpsViewer[i] = this.gameObject.transform.GetChild(i).gameObject;
        }

        // Unityでアップロードする際にVRCPlayerApi関係のエラーが出ないように、Editor上では動かないようにする
        #if UNITY_EDITOR
        #else
            MyInfo = Networking.LocalPlayer;                                        // 自分自身のプレイヤー情報を保存
            SendCustomEventDelayedSeconds(nameof(SetOwner), SetOwnerDelay);         // SetOwnerDelay[秒]後にSetOwnerを実行
        #endif
    }


    public void SetOwner()
    {
        // このオブジェクトのオーナーだったら実行
        if (true == Networking.IsOwner(MyInfo, this.gameObject))
        {
            // インスタンス内にいるプレイヤーの情報を取得
            PlayersInfo = VRCPlayerApi.GetPlayers(new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()]);

            // インスタンス内にいるプレイヤーの人数分、FpsViewerにオーナー設定する
            FpsViewerCount = 0;                                                     // 子オブジェクトの何個目か?リセット
            foreach (VRCPlayerApi TempPlayer in PlayersInfo)
            {
                // エラーチェック
                if (null  == TempPlayer)                             {continue;}    // VRCPlayerApi関連
                if (false == Utilities.IsValid(TempPlayer))          {continue;}    // VRCPlayerApi関連
                if (FpsViewerCount >= FpsViewerNum)                  {break;   }    // プレイヤー人数に対してFpsViewerの数が足りてなかったらこのforeach文を抜ける

                // マスター専用のFpsViewerがあるので、自分自身だったらスキップ
                if (true == Networking.IsOwner(TempPlayer, this.gameObject)){continue;}

                // まだオーナーの割り当てがされてなかったら実行
                if (false == Networking.IsOwner(TempPlayer, FpsViewer[FpsViewerCount]))
                {
                    Debug.Log ("[FpsViewer2] SetOwner: FpsViewer[" + FpsViewerCount + "] " + TempPlayer.displayName);
                    Networking.SetOwner(TempPlayer, FpsViewer[FpsViewerCount]);     // 今のFpsViewerに今のTempPlayerをオーナー割り当て
                }

                FpsViewerCount++;                                                   // 子オブジェクトの何個目か?カウントアップ
            }

            // FpsViewerが余ってたらマスターである自分をオーナーとして設定しておく
            for (; FpsViewerCount < FpsViewerNum; FpsViewerCount++)
            {
                if (false == Networking.IsOwner(MyInfo, FpsViewer[FpsViewerCount]))
                {
                    Debug.Log ("[FpsViewer2] ResetOwner: FpsViewer[" + FpsViewerCount + "]");
                    Networking.SetOwner(MyInfo, FpsViewer[FpsViewerCount]);
                }
            }
        }

        SendCustomEventDelayedSeconds(nameof(SetOwner), SetOwnerCycle);             // このメソッドをSetOwnerCycle[s]間隔で実行
    }

}
