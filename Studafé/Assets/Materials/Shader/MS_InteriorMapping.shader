// Copyright (c) 2024 momoma
// Released under the MIT license
// https://opensource.org/licenses/mit-license.php

Shader "MomomaShader/Surface/InteriorMapping"
{
	Properties
	{
		[NoScaleOffset] _InteriorMap ("Interior Map", Cube) = "" { }
		_InteriorSize ("Interior Size", Vector) = (1, 1, 1, 1)
		_InteriorOffset ("Interior Offset", Vector) = (0, 0, 0, 0)
		_MainTex ("Main Texture", 2D) = "black" { }
		_Color ("Color", Color) = (1, 1, 1, 1)
		[NoScaleOffset] _GlassColorRamp ("Glass Color Ramp", 2D) = "white" { }
		[HDR] _GlassColor ("Glass Color", Color) = (1, 1, 1, 1)
		[NoScaleOffset] _BumpMap ("Normal Map", 2D) = "bump" { }
		_BumpScale ("Normal Scale", Float) = 1.0
		_IoR ("Index of Refraction", Range(1.0, 2.0)) = 1.5
		_Thickness ("Thickness", Range(0.0, 0.1)) = 0.05
		[NoScaleOffset] _SpecGlossMap ("Roughness Map", 2D) = "white" { }
		_Glossiness ("Roughness", Range(0.0, 1.0)) = 0.5
		[IntRange] _FixedRotation ("Fixed Rotation", Range(0, 3)) = 0
		[ToggleUI] _RandomRotation ("Use Random Rotation", Float) = 0
	}
	Subshader
	{
		Tags { "DisableBatching" = "True" }
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows addshadow
		#pragma target 3.0

		struct Input
		{
			float2 uv_MainTex;
			float3 worldPos;
		};

		UNITY_DECLARE_TEXCUBE(_InteriorMap);
		float4 _InteriorMap_HDR;
		UNITY_DECLARE_TEX2D(_MainTex);
		UNITY_DECLARE_TEX2D_NOSAMPLER(_BumpMap);
		UNITY_DECLARE_TEX2D_NOSAMPLER(_SpecGlossMap);
		UNITY_DECLARE_TEX2D(_GlassColorRamp);

		UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_DEFINE_INSTANCED_PROP(float4, _InteriorSize)
		UNITY_DEFINE_INSTANCED_PROP(float4, _InteriorOffset)
		UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
		UNITY_DEFINE_INSTANCED_PROP(half4, _GlassColor)
		UNITY_DEFINE_INSTANCED_PROP(float, _BumpScale)
		UNITY_DEFINE_INSTANCED_PROP(float, _IoR)
		UNITY_DEFINE_INSTANCED_PROP(float, _Thickness)
		UNITY_DEFINE_INSTANCED_PROP(float, _Glossiness)
		UNITY_DEFINE_INSTANCED_PROP(float, _FixedRotation)
		UNITY_DEFINE_INSTANCED_PROP(fixed, _RandomRotation)
		UNITY_INSTANCING_BUFFER_END(Props)

		inline float boxIntersection(float3 ro, float3 rd, float3 boxSize)
		{
			float3 m = 1.0 / rd;
			float3 t2 = -m * ro + abs(m) * boxSize;
			return min(min(t2.x, t2.y), t2.z);
		}

		float3 getInteriorDirection(float3 ro, float3 rd, float3 n, float3 boxSize, float ior, float thickness, out float2 index)
		{
			ro -= rd / max(rd.z, 1e-6) * ro.z;
			index = round((ro.xy) / boxSize.xy) * boxSize.xy;
			ro.xy -= index;
			ro.z -= boxSize.z * 0.5;
			float3 rr = refract(rd, -n, 1.0 / ior);
			float nor = dot(n, rr);
			ro += rr * thickness / max(nor, 1e-4);
			float t = boxIntersection(ro, rd, boxSize * 0.5);
			return ro + rd * t;
		}

		inline float roughnessToLod(float roughness)
		{
			return UNITY_SPECCUBE_LOD_STEPS * roughness * (1.7 - 0.7 * roughness);
		}

		inline float2 rotate(float2 p, float rad)
		{
			float s, c;
			sincos(rad, s, c);
			return mul(float2x2(c, -s, s, c), p);
		}

		float hash12(float2 p)
		{
			float3 p3 = frac(p.xyx * 0.1031);
			p3 += dot(p3, p3.yzx + 33.33);
			return frac((p3.x + p3.y) * p3.z);
		}

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			float4 boxSize = UNITY_ACCESS_INSTANCED_PROP(Props, _InteriorSize);
			boxSize.xyz *= boxSize.w;
			float4 c = UNITY_SAMPLE_TEX2D(_MainTex, IN.uv_MainTex);
			c *= UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
			float bumpScale = UNITY_ACCESS_INSTANCED_PROP(Props, _BumpScale);
			float3 normal = UnpackNormalWithScale(UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap, _MainTex, IN.uv_MainTex), bumpScale);
			float ior = UNITY_ACCESS_INSTANCED_PROP(Props, _IoR);
			float thickness = UNITY_ACCESS_INSTANCED_PROP(Props, _Thickness);
			float roughness = UNITY_ACCESS_INSTANCED_PROP(Props, _Glossiness);
			roughness *= UNITY_SAMPLE_TEX2D_SAMPLER(_SpecGlossMap, _MainTex, IN.uv_MainTex).r;

			float3 oPos = mul(unity_WorldToObject, float4(IN.worldPos, 1)).xyz;
			float3 ro = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1)).xyz;
			float3 rd = normalize(oPos - ro);
			ro -= UNITY_ACCESS_INSTANCED_PROP(Props, _InteriorOffset).xyz;
			float2 index;
			float3 dir = getInteriorDirection(ro, rd, normal, boxSize.xyz, ior, thickness, index);
			float2 seed = index + float2(unity_ObjectToWorld[0][3], unity_ObjectToWorld[2][3]);
			float rotation = (UNITY_ACCESS_INSTANCED_PROP(Props, _FixedRotation) + UNITY_ACCESS_INSTANCED_PROP(Props, _RandomRotation) * floor(4.0 * hash12(seed + 1.84))) * UNITY_HALF_PI;
			dir.xz = rotate(dir.xz, rotation);
			float lod = roughnessToLod(roughness);
			float3 cubemapColor = DecodeHDR(UNITY_SAMPLE_TEXCUBE_LOD(_InteriorMap, dir, lod), _InteriorMap_HDR);
			cubemapColor *= UNITY_ACCESS_INSTANCED_PROP(Props, _GlassColor).rgb;
			cubemapColor *= UNITY_SAMPLE_TEX2D_LOD(_GlassColorRamp, float2(hash12(seed), 0.5), 0).rgb;
			float3 inCol = cubemapColor * (1 - c.a);
			c.rgb *= c.a;

			o.Albedo = c.rgb;
			o.Emission = inCol;
			o.Normal = normal;
			o.Metallic = 0;
			o.Smoothness = 1 - roughness;
			o.Alpha = 1;
		}
		ENDCG
	}
}