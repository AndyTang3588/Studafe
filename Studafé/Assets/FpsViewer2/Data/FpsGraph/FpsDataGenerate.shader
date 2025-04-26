// 作成者:dimebag29 作成日:2023年7月27日
// このプログラムのライセンスはCC0 (クリエイティブ・コモンズ・ゼロ)です。いかなる権利も保有しません。
// This program is licensed to CC0.

Shader "FpsDataGenerate"
{
    Properties
    {
        ScaleX("横軸スケール[s]   (何秒間のデータを表示するか)", Float) = 4.0
        ScaleY("縦軸スケール[fps] (何fpsまでのデータを表示するか)", Float) = 150.0
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            
            #include "UnityCustomRenderTexture.cginc"

            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag

            float ScaleX;
            float ScaleY;

            half frag(v2f_customrendertexture i) : SV_Target
            {
                float Dt  = unity_DeltaTime.x;                                          // 前フレームからの経過時間[s]
                float Fps = unity_DeltaTime.y;                                          // 前フレームからの経過時間の逆数[1/s] (FPSということ)

                float2 iuv = i.globalTexcoord;                                          // CustomRenderTextureの場合はこうやってuvを取得するらしい

                float Offset = Dt / ScaleX;                                             // 前フレームからどのくらいグラフを進めるか計算
                float OldCol = tex2D(_SelfTexture2D, float2(iuv.x - Offset, 0.5)).r;    // 前フレームのテクスチャ(色)をOffset分右にずらしたテクスチャ(色)を生成。UVのY方向は中間狙い(0.5)
                OldCol *= step(Offset, iuv.x);                                          // 最新のFPSの値が書き込まれる領域は黒塗りしておく
                
                float NowCol = Fps / ScaleY;                                            // 最新のFPSの値を色として保存するために、FPSの値が縦軸スケールの最大値になったら1.0になるようにする (色の範囲は0.0~1.0)
                clamp(NowCol, 0.0, 1.0);                                                // もしFPSの色が1.0を超えてたら1.0にクランプさせる
                NowCol *= step(iuv.x, Offset);                                          // 過去のFPSが書き込まれる領域は黒塗りしておく

                return OldCol + NowCol;                                                 // 過去のFPSと最新のFPSのテクスチャ(色)を足して出力
            }
            
            ENDCG
        }
    }
}