// 作成者:dimebag29 作成日:2023年7月27日
// このプログラムのライセンスはCC0 (クリエイティブ・コモンズ・ゼロ)です。いかなる権利も保有しません。
// This program is licensed to CC0.

// 変数StartDelayが使用されていません という警告を出さないようにする。(実際はSendCustomEventDelayedSecondsの引数で使っている)
#pragma warning disable 0414

using UdonSharp;
using UnityEngine;
using UnityEngine.UI;                                                               // Textオブジェクトを扱う場合に必要
using VRC.SDKBase;
using VRC.Udon;
using System;                                                                       // 文字列を数値に変換するConvert.ToInt32()で必要


[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]                                   // Udonの同期モードを手動に設定

public class FpsViewer2_ForMaster : UdonSharpBehaviour
{
    [UdonSynced(UdonSyncMode.None)] private string SyncData = "";                   // 同期変数 (プラットフォーム1桁、FPS最少値3桁、FPS平均値3桁の7桁数字の文字列)

    private bool UpdateOK = false;                                                  // Update()内の処理開始を遅らせるためのトリガー
    private float StartDelay = 3.0f;                                                // Join何秒後から処理を開始するか
    private float SyncCycle = 1.0f;                                                 // 何秒間隔で同期処理を実行するか

    private bool AmIOwner = false;                                                  // 自分がこのオブジェクトのオーナーだったらtrue

    private VRCPlayerApi MyInfo;                                                    // 自分のプレイヤー情報
    private int Platform = 0;                                                       // 自分のプラットフォーム

    private int[] StackFps = new int[512];                                          // FPS履歴配列
    private int StackFpsNum = 0;                                                    // FPS履歴配列のデータ数
    private float StackTime = 0.0f;                                                 // Update()内の経過時間スタック用
    private int FpsMin = 0;                                                         // FPS最少値
    private int FpsSum = 0;                                                         // FPS平均値を計算するためのFPS合計値
    private int FpsAvg = 0;                                                         // FPS平均値

    private int FpsAvgReceive = 0;                                                  // 自分以外の人から受け取ったFPS平均値
    private int BarNum = 0;                                                         // 自分以外の人から受け取ったFPS平均値をバー表示する用

    // PlatformとFPSバー表示用
    private string[] PlatformString = new string[5];                                // プラットフォーム名を保存しておく箱
    private Color[]  BarColor = new Color[21];                                      // FPSバー表示用色
    private string[] BarString = new string[21];                                    // FPSバー表示用テキスト

    public Text TextName, TextPlatform, TextFpsMin, TextFpsAvg, TextBar;            // Inspector画面で指定してもらうTextオブジェクト
    

    void Start()
    {
        PrepareData();                                                              // PlatformとFPSバー表示用データを用意する

        // Unityでアップロードする際にVRCPlayerApi関係のエラーが出ないように、Editor上では動かないようにする
        #if UNITY_EDITOR
        #else
            SendCustomEventDelayedSeconds(nameof(SaveMyInfo), StartDelay);          // StartDelay[秒]後に実行
        #endif
    }

    // 自分のプレイヤー情報を取得
    public void SaveMyInfo()
    {
        MyInfo = Networking.LocalPlayer;                                            // 自分のVRCPlayerApiを取得

        #if UNITY_ANDROID
            if (true == MyInfo.IsUserInVR()){Platform = 2;}                         // AndroidかつVRならQuest
            else                            {Platform = 3;}                         // AndroidかつVRじゃなかったらAndroid
        #elif UNITY_IOS
            Platform = 4;                                                           // iOS
        #else
            if (true == MyInfo.IsUserInVR()){Platform = 0;}                         // 上記以外でVRならPCVR
            else                            {Platform = 1;}                         // 上記以外でVRじゃなかったらDesktop
        #endif

        OwnerJdgeCycle();                                                           // OwnerJdgeCycle()サイクルをスタートさせる
        UpdateOK = true;                                                            // Update()内の最初のif分の最初の条件式を解除
    }

    // SyncCycle[s]ごとに自分が「このオブジェクトのオーナーか」「インスタンスマスターか」を確認
    public void OwnerJdgeCycle()
    {
        AmIOwner = Networking.IsOwner(MyInfo, this.gameObject);                     // このオブジェクトのオーナーか確認
        SendCustomEventDelayedSeconds(nameof(OwnerJdgeCycle), SyncCycle);           // SyncCycle[s]後にこおメソッドを再度実行
    }

    // 毎フレーム全員が実行
    void Update()
    {
        // SaveMyInfo()が完了 && このオブジェクトのオーナー なら実行
        // AmIOwnerはOwnerJdgeCycle()内でSyncCycle[s]間隔で随時更新される
        if (true == UpdateOK && true == AmIOwner)
        {
            // 同期変数更新処理 (SyncCycle[s]間隔で実行)
            if (StackTime > SyncCycle)  
            {
                SendSyncData();                                                     // 同期変数更新
                StackFpsNum = 0;                                                    // FPS履歴配列のデータ数をリセットする
                StackTime = 0.0f;                                                   // 経過時間をリセットする
            }

            // FPS履歴配列更新
            else
            {
                StackFps[StackFpsNum] = (int)(1.0f / Time.deltaTime);               // 現在のFPSを計算してFPS履歴配列に追加
                StackFpsNum++;                                                      // FPS履歴配列のデータ数を加算
                StackTime += Time.deltaTime;                                        // 経過時間を加算
            }
        }
    }

    // 同期変数更新
    public void SendSyncData()
    {
        // SyncCycle[s]中にUpdate()が回った回数分ループ
        FpsSum = 0;                                                                 // FPS平均値を計算するための合計値
        FpsMin = 512;                                                               // FPS最少値。最初はわざと大きい値を設定しておく
        for (int i = 0; i < StackFpsNum ; i++)
        {
            FpsSum += StackFps[i];                                                  // FPS平均値を計算するための合計値 加算
            if (FpsMin > StackFps[i]){FpsMin = StackFps[i];}                        // FPS最少値探索
        }
        FpsAvg = FpsSum / StackFpsNum;                                              // FPS平均値計算

        // 同期変数としてプラットフォーム1桁、FPS最少値3桁、FPS平均値3桁を保存
        SyncData = Platform.ToString("0") + FpsMin.ToString("000") + FpsAvg.ToString("000");
        RequestSerialization(); ViewUpdate();                                       // 同期変数更新を非オーナーに通知 & オーナー自身は表示更新
    }

    // オーナーが同期変数を更新した後に、オーナー以外全員が実行する
    public override void OnDeserialization()
    {
        // オブジェクトの初期Active状態がfalseだと、Active状態がtrueになった瞬間Start()よりもOnDeserialization()が先に発火するらしい仕様を回避
        if (true == UpdateOK){ViewUpdate();}
    }

    // 表示更新
    public void ViewUpdate()
    {
        TextName.text     = Networking.GetOwner(this.gameObject).displayName;           // このオブジェクトのオーナー名表示
        TextPlatform.text = PlatformString[Convert.ToInt32(SyncData.Substring(0, 1))];  // 同期変数の1文字目からプラットフォーム名を表示
        TextFpsMin.text   = $"{Convert.ToInt32(SyncData.Substring(1, 3))}";             // 同期変数の2~4文字目からFPS最少値を表示

        FpsAvgReceive     = Convert.ToInt32(SyncData.Substring(4, 3));                  // 同期変数の5~7文字目からFPS平均値を取得
        TextFpsAvg.text   = $"{FpsAvgReceive}";                                         // FPS平均値を表示

        BarNum = (int)(FpsAvgReceive / 5.0f);                                           // FPSバーが何段階目か計算
        if (BarNum > 20){BarNum = 20;}                                                  // FPSバーが20段階目(最大)を超えていたら20にしてしまう
        TextBar.text = BarString[BarNum];                                               // FPSバーのテキスト表示
        TextBar.color = BarColor[BarNum];                                               // FPSバーの色を設定
    }


    // プラットフォーム名 と FPSバーの色とバー文字列を用意
    public void PrepareData()
    {
        // プラットフォーム名を準備しておく
        PlatformString[0] = "PCVR";
        PlatformString[1] = "Desktop";
        PlatformString[2] = "Quest";
        PlatformString[3] = "Android";
        PlatformString[4] = "iOS";

        // FPSバーの色とバー文字列を用意しておく
        BarColor[ 0] = new Color(1.0f, 0.0f, 0.0f, 1.0f);   BarString[ 0] = "";                     //   5 FPS未満
        BarColor[ 1] = new Color(1.0f, 0.1f, 0.0f, 1.0f);   BarString[ 1] = "█";                    //  10 FPS未満
        BarColor[ 2] = new Color(1.0f, 0.2f, 0.0f, 1.0f);   BarString[ 2] = "██";                   //  15 FPS未満
        BarColor[ 3] = new Color(1.0f, 0.3f, 0.0f, 1.0f);   BarString[ 3] = "███";                  //  20 FPS未満
        BarColor[ 4] = new Color(1.0f, 0.4f, 0.0f, 1.0f);   BarString[ 4] = "████";                 //  25 FPS未満
        BarColor[ 5] = new Color(1.0f, 0.5f, 0.0f, 1.0f);   BarString[ 5] = "█████";                //  30 FPS未満
        BarColor[ 6] = new Color(1.0f, 0.6f, 0.0f, 1.0f);   BarString[ 6] = "██████";               //  35 FPS未満
        BarColor[ 7] = new Color(1.0f, 0.7f, 0.0f, 1.0f);   BarString[ 7] = "███████";              //  40 FPS未満
        BarColor[ 8] = new Color(1.0f, 0.8f, 0.0f, 1.0f);   BarString[ 8] = "████████";             //  45 FPS未満
        BarColor[ 9] = new Color(1.0f, 0.9f, 0.0f, 1.0f);   BarString[ 9] = "█████████";            //  50 FPS未満
        BarColor[10] = new Color(1.0f, 1.0f, 0.0f, 1.0f);   BarString[10] = "██████████";           //  55 FPS未満
        BarColor[11] = new Color(0.9f, 1.0f, 0.0f, 1.0f);   BarString[11] = "███████████";          //  60 FPS未満
        BarColor[12] = new Color(0.8f, 1.0f, 0.0f, 1.0f);   BarString[12] = "████████████";         //  65 FPS未満
        BarColor[13] = new Color(0.7f, 1.0f, 0.0f, 1.0f);   BarString[13] = "█████████████";        //  70 FPS未満
        BarColor[14] = new Color(0.6f, 1.0f, 0.0f, 1.0f);   BarString[14] = "██████████████";       //  75 FPS未満
        BarColor[15] = new Color(0.5f, 1.0f, 0.0f, 1.0f);   BarString[15] = "███████████████";      //  80 FPS未満
        BarColor[16] = new Color(0.4f, 1.0f, 0.0f, 1.0f);   BarString[16] = "████████████████";     //  85 FPS未満
        BarColor[17] = new Color(0.3f, 1.0f, 0.0f, 1.0f);   BarString[17] = "█████████████████";    //  90 FPS未満
        BarColor[18] = new Color(0.2f, 1.0f, 0.0f, 1.0f);   BarString[18] = "██████████████████";   //  95 FPS未満
        BarColor[19] = new Color(0.1f, 1.0f, 0.0f, 1.0f);   BarString[19] = "███████████████████";  // 100 FPS未満
        BarColor[20] = new Color(0.0f, 1.0f, 0.0f, 1.0f);   BarString[20] = "████████████████████"; // 105 FPS未満
    }
}
