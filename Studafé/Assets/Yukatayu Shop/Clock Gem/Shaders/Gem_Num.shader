Shader "Yukatayu/ClockGem/Number" {
	Properties {
		[Header(Color)] [Space(7)]
		_Color1 ("Color 1 (外側の色)", Color) = (1,1,1,1)
		_Color2 ("Color 2 (内側の色)", Color) = (1,1,1,1)
		[HideInspector]_PosX ( "Color Position X", Float) = 0.08
		_PosY ( "Color Position Y", Float) = 0.14
		_ScaleX ( "Color Scale Inverse X", Float) = 10
		_ScaleY ( "Color Scale Inverse Y", Float) = 21.25
		[PowerSlider(2.0)]_Coeff ("Edge Coefficient (色の切り替わりの加減)", Range(0, 200)) = 7
		[Space(7)] [Header(Content)] [Space(7)]
		_Num("Number to show", Int) = 0
		[Space(7)] [Header(Gem)] [Space(7)]
		_ReflectionStrength ("Reflection Strength", Range(0.0,2.0)) = 1.0
		_EnvironmentLight ("Environment Light", Range(0.0,2.0)) = 1.5
		_Emission ("Emission", Range(0.0,2.0)) = 1.0
		[NoScaleOffset] _RefractTex ("Refraction Texture", Cube) = "" {}
	}
	SubShader {
		Tags {
			"Queue" = "Transparent"
		}
		Pass {
			Cull Front
			ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f {
				float4 pos : SV_POSITION;
				float3 uv : TEXCOORD0;
				float4 v : TEXCOORD1;
			};
			float _Num;

			v2f vert (float4 v : POSITION, float3 n : NORMAL, float2 uv : TEXCOORD0) {
				v2f o;
				o.v = v;
				//////
				const unsigned int maxKeta = 6;  // fbx の uv 依存
				bool show = false;
				unsigned int num = abs(floor(_Num));
				for(unsigned int k=0; k < maxKeta; ++k){
					const unsigned int current = num % 10;
					num /= 10;
					const float yLowerBound = (float)k/maxKeta;
					const float yUpperBound = (float)(k+1)/maxKeta;
					const float xLowerBound = current/10.;
					const float xUpperBound = (current+1)/10.;
					show =
						show ||
						(  yLowerBound <= (1-uv.y) && (1-uv.y) < yUpperBound
						&& xLowerBound <= uv.x && uv.x < xUpperBound);
				}
				o.pos = UnityObjectToClipPos(v) * show;
				//////

				float3 viewDir = normalize(ObjSpaceViewDir(v));
				o.uv = -reflect(viewDir, n);
				o.uv = mul(unity_ObjectToWorld, float4(o.uv,0));
				return o;
			}

			fixed4 _Color1, _Color2;
			samplerCUBE _RefractTex;
			half _EnvironmentLight;
			half _Emission;
			float _PosX, _PosY, _ScaleX, _ScaleY, _Coeff;
			half4 frag (v2f i) : SV_Target {
				// 一部の色を変える処理 (下の方に同じコードが存在)
				fixed4 color = lerp(_Color2, _Color1, saturate((length((i.v.xy - float2(_PosX, _PosY))*float2(_ScaleX, _ScaleY)) - 1) * _Coeff));
				// 反射など
				half4 reflection = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, i.uv);
				reflection.rgb = DecodeHDR (reflection, unity_SpecCube0_HDR);
				half3 multiplier = reflection.rgb * _EnvironmentLight + _Emission;
				return half4(texCUBE(_RefractTex, i.uv).rgb * color.rgb * multiplier.rgb, 1.0f);
			}
			ENDCG 
		}

		Pass {
			ZWrite On
			Blend One One
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
        
			struct v2f {
				float4 pos : SV_POSITION;
				float3 uv : TEXCOORD0;
				half fresnel : TEXCOORD1;
				float4 v : TEXCOORD2;
			};
			float _Num;

			v2f vert (float4 v : POSITION, float3 n : NORMAL, float2 uv : TEXCOORD0) {
				v2f o;
				o.v = v;
				//////
				const unsigned int maxKeta = 6;  // fbx の uv 依存
				bool show = false;
				unsigned int num = abs(floor(_Num));
				for(unsigned int k=0; k < maxKeta; ++k){
					const unsigned int current = num % 10;
					num /= 10;
					const float yLowerBound = (float)k/maxKeta;
					const float yUpperBound = (float)(k+1)/maxKeta;
					const float xLowerBound = current/10.;
					const float xUpperBound = (current+1)/10.;
					show =
						show ||
						(  yLowerBound <= (1-uv.y) && (1-uv.y) < yUpperBound
						&& xLowerBound <= uv.x && uv.x < xUpperBound);
				}
				o.pos = UnityObjectToClipPos(v) * show;
				//////

				float3 viewDir = normalize(ObjSpaceViewDir(v));
				o.uv = -reflect(viewDir, n);
				o.uv = mul(unity_ObjectToWorld, float4(o.uv,0));
				o.fresnel = 1.0 - saturate(dot(n,viewDir));
				return o;
			}

			fixed4 _Color1, _Color2;
			samplerCUBE _RefractTex;
			half _ReflectionStrength;
			half _EnvironmentLight;
			half _Emission;
			float _PosX, _PosY, _ScaleX, _ScaleY, _Coeff;
			half4 frag (v2f i) : SV_Target {
				// 一部の色を変える処理 (上の方に同じコードが存在)
				fixed4 color = lerp(_Color2, _Color1, saturate((length((i.v.xy - float2(_PosX, _PosY))*float2(_ScaleX, _ScaleY)) - 1) * _Coeff));
				// 反射など
				half4 reflection = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, i.uv);
				reflection.rgb = DecodeHDR (reflection, unity_SpecCube0_HDR);
				half3 multiplier = reflection.rgb * _EnvironmentLight + _Emission;
				return fixed4(reflection * _ReflectionStrength * i.fresnel
					+ texCUBE(_RefractTex, i.uv).rgb * color.rgb * multiplier, 1.0f);
			}
			ENDCG
		}

		Pass {
			Tags{ "LightMode"="ShadowCaster" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster

			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				V2F_SHADOW_CASTER;
			};
			float _Num;

			v2f vert (appdata v) {
				v2f o;
				//////
				const float2 uv = v.texcoord;
				const unsigned int maxKeta = 6;  // fbx の uv 依存
				bool show = false;
				unsigned int num = abs(floor(_Num));
				for(unsigned int k=0; k < maxKeta; ++k){
					const unsigned int current = num % 10;
					num /= 10;
					const float yLowerBound = (float)k/maxKeta;
					const float yUpperBound = (float)(k+1)/maxKeta;
					const float xLowerBound = current/10.;
					const float xUpperBound = (current+1)/10.;
					show =
						show ||
						(  yLowerBound <= (1-uv.y) && (1-uv.y) < yUpperBound
						&& xLowerBound <= uv.x && uv.x < xUpperBound);
				}
				v.vertex *= show;
				//////
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				return o;
			}

			fixed4 frag (v2f i) : SV_Target {
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
	}
}
