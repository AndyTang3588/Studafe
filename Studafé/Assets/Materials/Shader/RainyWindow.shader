// Modified from:
//
// Making a rainy window in Unity - by Martijn Steinrucken, 2019 (@The_ArtOfCode)
//   https://www.youtube.com/watch?v=EBrAdahFtuo
//   https://www.youtube.com/watch?v=0flY11lVCwY
// ShaderToy Live Version:
//   https://www.shadertoy.com/view/ltffzl
// License:
//   Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License
//
// - Added Japanese descriptions and property for number of samples
//

Shader "Unlit/RainyWindow"
{
	Properties
	{
		[HideInInspector] _MainTex ("Texture", 2D) = "white" {}
		_size ("Size (サイズ)", float) = 5
		_T("Droplet speed (水滴の速さ)", float) = -10
		_Distortion("Distortion (くっせつ)", range(-5, 5)) = -5
		_Blur("Blur (ブラー)", range(0, 1)) = 0.5

		_NumSamples("Blur Samples (ブラーのサンプル数) [Heavy! 重い!]", int) = 8
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
			"Queue" = "Transparent"
		}
		LOD 100

		GrabPass { "_GrabTexture" }
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			#define S(a, b, t) smoothstep(a, b ,t)
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 GrabUv : TEXCOORD1;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex, _GrabTexture;
			float4 _MainTex_ST;
			float _size, _T, _Distortion, _Blur;

			int _NumSamples;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.GrabUv = UNITY_PROJ_COORD(ComputeGrabScreenPos(o.vertex));

				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			float N21(float2 p)
			{
				p = frac(p*float2(123.34, 345.45));
				p += dot(p, p + 34.345);
				return frac(p.x*p.y);
			}

			float3 Layer(float2 UV, float t)
			{
				float2 aspect = float2(2, 1);
				float2 uv = UV * _size * aspect;
				uv.y += t * .25;
				float2 gv = frac(uv) - .5;
				float2 id = floor(uv);

				float n = N21(id); // 0 1
				t += n*6.2831;

				float w = UV.y * 10;
				float x = (n - .5) *.8; // - .4 .4
				x += (.4-abs(x)) * sin(3 * w) * pow(sin(w), 6) * .45;

				float y = -sin(t + sin(t + sin(t) * .5)) *.45;
				y -= (gv.x - x) * (gv.x - x);

				float2 dropPos = (gv-float2(x, y)) / aspect;
				float drop = S(.05, .03, length(dropPos));

				float2 trailPos = (gv-float2(x, t * .25)) / aspect;
				trailPos.y = (frac(trailPos.y * 8) - .5) / 8;
				float trail = S(.03, .01, length(trailPos));
				float fogTrail = S(-.05, .05, dropPos.y);
				fogTrail *= S(.5, y, gv.y);
				trail *= fogTrail;
				fogTrail *= S(.05, .04, abs(dropPos.x));

				float2 offs = drop * dropPos + trail * trailPos;

				return float3(offs, fogTrail);
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float t = fmod(_Time.y + _T, 7200);

				float4 col = 0;

				float3 drops = Layer(i.uv, t);
				drops += Layer(i.uv*1.23 + 7.54, t);
				drops += Layer(i.uv*1.35 + 1.54, t);
				drops += Layer(i.uv*1.57 - 7.54, t);

				float fade = 1 - saturate(fwidth(i.uv) * 60);

				float blur = _Blur * 7 * (1 - drops.z * fade);

				//col = tex2Dlod(_MainTex, float4(i.uv+drops.xy*_Distortion,0,blur));

				float2 projUv = i.GrabUv.xy / i.GrabUv.w;
				projUv += drops.xy * _Distortion * fade;
				blur *= .01;

				const float numSamples = _NumSamples;
				float a = N21(i.uv) * 6.2831;
				for (float i = 0; i < numSamples; i++)
				{
					float2 offs = float2(sin(a), cos(a)) * blur;
					float d = frac(sin((i+1) * 546.) * 5424.);
					d = sqrt(d);
					offs *= d;
					col += tex2D(_GrabTexture, projUv + offs);
					a++;
				}
				col /= numSamples;

				//col *= 0; col += fade;

				return col * .9;
			}
		ENDCG
		}
	}
}
