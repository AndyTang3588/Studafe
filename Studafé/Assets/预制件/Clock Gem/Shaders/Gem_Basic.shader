Shader "Yukatayu/ClockGem/Basic" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_ReflectionStrength ("Reflection Strength", Range(0.0,2.0)) = 1.0
		_EnvironmentLight ("Environment Light", Range(0.0,2.0)) = 1.0
		_Emission ("Emission", Range(0.0,2.0)) = 0.0
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
			};

			v2f vert (float4 v : POSITION, float3 n : NORMAL) {
				v2f o;
				o.pos = UnityObjectToClipPos(v);
				float3 viewDir = normalize(ObjSpaceViewDir(v));
				o.uv = -reflect(viewDir, n);
				o.uv = mul(unity_ObjectToWorld, float4(o.uv,0));
				return o;
			}

			fixed4 _Color;
			samplerCUBE _RefractTex;
			half _EnvironmentLight;
			half _Emission;
			half4 frag (v2f i) : SV_Target {
				half4 reflection = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, i.uv);
				reflection.rgb = DecodeHDR (reflection, unity_SpecCube0_HDR);
				half3 multiplier = reflection.rgb * _EnvironmentLight + _Emission;
				return half4(texCUBE(_RefractTex, i.uv).rgb * _Color.rgb * multiplier.rgb, 1.0f);
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
			};

			v2f vert (float4 v : POSITION, float3 n : NORMAL) {
				v2f o;
				o.pos = UnityObjectToClipPos(v);
				float3 viewDir = normalize(ObjSpaceViewDir(v));
				o.uv = -reflect(viewDir, n);
				o.uv = mul(unity_ObjectToWorld, float4(o.uv,0));
				o.fresnel = 1.0 - saturate(dot(n,viewDir));
				return o;
			}

			fixed4 _Color;
			samplerCUBE _RefractTex;
			half _ReflectionStrength;
			half _EnvironmentLight;
			half _Emission;
			half4 frag (v2f i) : SV_Target {
				half4 reflection = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, i.uv);
				reflection.rgb = DecodeHDR (reflection, unity_SpecCube0_HDR);
				half3 multiplier = reflection.rgb * _EnvironmentLight + _Emission;
				return fixed4(reflection * _ReflectionStrength * i.fresnel
					+ texCUBE(_RefractTex, i.uv).rgb * _Color.rgb * multiplier, 1.0f);
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

			v2f vert (appdata v) {
				v2f o;
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
