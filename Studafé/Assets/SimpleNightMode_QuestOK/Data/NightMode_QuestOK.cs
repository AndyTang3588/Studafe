// 作成者:dimebag29 作成日:2024年10月14日 バージョン:v0.0
// (Author:dimebag29 Creation date:October 14, 2024 Version:v0.0)

// このプログラムのライセンスはCC0 (クリエイティブ・コモンズ・ゼロ)です。いかなる権利も保有しません。
// (This program is licensed under CC0 (Creative Commons Zero). No rights reserved.)
// https://creativecommons.org/publicdomain/zero/1.0/

using UdonSharp;
using UnityEngine;
using UnityEngine.UI;   // Textを扱う際に必要
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]     // 同期モード無し

public class NightMode_QuestOK : UdonSharpBehaviour
{
    [SerializeField] private Text TextObject;
    [SerializeField] private Slider SliderObject;
    [SerializeField] private GameObject DarknessSphereObject;

    private Material DarknessBoxMaterial;           // このスクリプトから値を変更したいShaderがアタッチされているマテリアル
    private string ShaderPropertyName = "Darkness"; // このスクリプトから値を変更したいShader内の変数名
    private int ShaderPropertyId = 0;               // このスクリプトから値を変更したいShader内の変数名のint値
    private bool UpdateOk = false;                  // PostLateUpdate内の処理を遅らせる用フラグ

    void Start()
    {
        // 値を変更したいShader内の変数名をintに変換 (StringのままMaterial.SetFloat()すると遅い)
        ShaderPropertyId = VRCShader.PropertyToID(ShaderPropertyName);

        // 値を変更したいShaderがアタッチされているマテリアル取得
        DarknessBoxMaterial = DarknessSphereObject.GetComponent<Renderer>().material;

        // Editorプレイ時にVRCPlayerApi関係のエラーが出るのでEditor上では動かないようにする用
        #if UNITY_EDITOR
        #else
            // Start時はなにかと不安定なので遅らせる
            SendCustomEventDelayedSeconds(nameof(Start2), 2.0f);
        #endif
    }
    public void Start2()
    {
        UpdateOk = true;    // PostLateUpdate内の処理が実行されるようにする
    }

    void PostLateUpdate()
    {
        if (true == UpdateOk)
        {
            // プレイヤーカメラにDarknessSphereObjectを追従させる
            DarknessSphereObject.transform.position = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            // 現状のバグでUpdate内でGetTrackingDataを取得すると1フレーム遅れた情報が取得されてしまうので注意。PostLateUpdate内で取得すること
            // https://feedback.vrchat.com/bug-reports/p/1342-gettrackingdata-returns-not-actual-head-rotation-position
        }
    }

    // スライダーの値が変更されると呼ばれるメソッド。SliderオブジェクトのOnValueChanged欄でこのメソッド名を指定しておく
    public void SliderValueChanged()
    {
        // 暗さの%表示
        TextObject.text = $"Night Mode {(100.0f * SliderObject.value).ToString("F0")}%";

        // Shader内の変数の値を変更 (最初に一気に暗くなるように値を0.2乗する)
        DarknessBoxMaterial.SetFloat(ShaderPropertyId, Mathf.Pow(SliderObject.value, 0.2f));
    }

}
