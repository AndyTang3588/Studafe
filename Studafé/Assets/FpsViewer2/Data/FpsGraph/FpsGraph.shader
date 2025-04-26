// 作成者:dimebag29 作成日:2023年7月27日
// このプログラムのライセンスはCC0 (クリエイティブ・コモンズ・ゼロ)です。いかなる権利も保有しません。
// This program is licensed to CC0.

Shader "FpsGraph"
{
    Properties
    {
        [NoScaleOffset]_MainTex ("FPSが保存されたテクスチャ (CrtFpsData)", 2D) = "black" {}
        _MainColor("グラフの線の色", Color) = (1.0, 1.0, 1.0, 1.0)
        LineWidth("グラフの線の太さ", Range (0.0, 0.2)) = 0.005
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv     : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _MainColor;
            float LineWidth;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float Fps = tex2D(_MainTex, float2(i.uv.x, 0.5)).r;     // CustomRenderTextureから現在のuvの位置におけるFPSの色を取得。UVのY方向は中間狙い(0.5)

                float Distance = distance(i.uv.y, Fps);                 // 現在のuvのY方向座標値(0.0~1.0)とFPSの色(0.0~1.0)との差を取得
                fixed Col = step(Distance, LineWidth);                  // 差がLineWidth以下なら1.0。それ以外なら0.0
                clip(Col - 0.9);                                        // Colが0.9以下なら描画しない。clipは引数が0以下なら描画しない関数

                _MainColor.rgb *= Col;                                  // グラフの線の色を計算

                return _MainColor;
            }
            ENDCG
        }
    }
}
