// 作成者:dimebag29 作成日:2024年10月14日 バージョン:v0.0
// (Author:dimebag29 Creation date:October 14, 2024 Version:v0.0)

// このシェーダーのライセンスはCC0 (クリエイティブ・コモンズ・ゼロ)です。いかなる権利も保有しません。
// (This Shader is licensed under CC0 (Creative Commons Zero). No rights reserved.)
// https://creativecommons.org/publicdomain/zero/1.0/

Shader "NightMode_QuestOK"
{
    Properties
    {
        Darkness("暗さ", range(0.0, 1.0)) = 0.0
        DarknessLimit("暗さ上限 (最大1.0)", float) = 0.995
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay+1000" }

        Cull Off ZTest Always   // 裏面描画ON、最前面表示
		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float Darkness;
            float DarknessLimit;

            // VRChat Shader Globals https://creators.vrchat.com/worlds/udon/vrc-graphics/vrchat-shader-globals/
            float _VRChatCameraMode;
            float _VRChatMirrorMode;

            float4 vert (float4 Vertex : POSITION) : SV_POSITION
            {
                if (0.001 > Darkness || 0.5 < _VRChatCameraMode ||0.5 < _VRChatMirrorMode)
                {
                    return 0.0; // ポリゴンを表示しない
                }
                return UnityObjectToClipPos(Vertex);
            }

            fixed4 frag () : SV_Target
            {
                Darkness = clamp(Darkness, 0.0, DarknessLimit);
                return fixed4(0.0, 0.0, 0.0, Darkness);
            }
            ENDCG
        }
    }
}
