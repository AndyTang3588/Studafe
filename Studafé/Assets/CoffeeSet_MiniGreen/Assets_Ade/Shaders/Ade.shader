// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "VRCCoffee/Ade"
{
	Properties
	{
		_TopColor("Top Color", Color) = (1,1,1,1)
		_BottomColor("Bottom Color", Color) = (1,1,1,1)
		_Depth("Depth", Float) = 0
		_DepthSmooth("Depth Smooth", Float) = 0
		_Bias("Bias", Float) = 0
		_Scale("Scale", Float) = 0
		_Power("Power", Float) = 0
		[Header(Blend)][Toggle]_ViewBlend("View Blend", Float) = 0
		_Pos("Pos", Float) = 0
		_Smooth("Smooth", Float) = 0
		_NoiseStrength("Noise Strength", Range( 0 , 1)) = 0
		_NoiseScale("Noise Scale", Float) = 1
		_DistortionAmount("Distortion Amount", Float) = 0
		_DistortionScale("Distortion Scale", Float) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "AlphaTest+0" "IgnoreProjector" = "True" }
		Cull Back
		ZWrite On
		Blend SrcAlpha OneMinusSrcAlpha , SrcAlpha OneMinusSrcAlpha
		
		GrabPass{ }
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#pragma target 3.0
		#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex);
		#else
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
		#endif
		#pragma surface surf Standard keepalpha noshadow 
		struct Input
		{
			float3 worldPos;
			float4 screenPos;
			float3 worldNormal;
		};

		uniform float _ViewBlend;
		uniform float _Pos;
		uniform float _Smooth;
		uniform float _NoiseStrength;
		uniform float _DistortionAmount;
		uniform float _DistortionScale;
		uniform float _NoiseScale;
		ASE_DECLARE_SCREENSPACE_TEXTURE( _GrabTexture )
		uniform float4 _TopColor;
		uniform float4 _BottomColor;
		uniform float _DepthSmooth;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _Depth;
		uniform float _Bias;
		uniform float _Scale;
		uniform float _Power;


		float3 mod3D289( float3 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 mod3D289( float4 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 permute( float4 x ) { return mod3D289( ( x * 34.0 + 1.0 ) * x ); }

		float4 taylorInvSqrt( float4 r ) { return 1.79284291400159 - r * 0.85373472095314; }

		float snoise( float3 v )
		{
			const float2 C = float2( 1.0 / 6.0, 1.0 / 3.0 );
			float3 i = floor( v + dot( v, C.yyy ) );
			float3 x0 = v - i + dot( i, C.xxx );
			float3 g = step( x0.yzx, x0.xyz );
			float3 l = 1.0 - g;
			float3 i1 = min( g.xyz, l.zxy );
			float3 i2 = max( g.xyz, l.zxy );
			float3 x1 = x0 - i1 + C.xxx;
			float3 x2 = x0 - i2 + C.yyy;
			float3 x3 = x0 - 0.5;
			i = mod3D289( i);
			float4 p = permute( permute( permute( i.z + float4( 0.0, i1.z, i2.z, 1.0 ) ) + i.y + float4( 0.0, i1.y, i2.y, 1.0 ) ) + i.x + float4( 0.0, i1.x, i2.x, 1.0 ) );
			float4 j = p - 49.0 * floor( p / 49.0 );  // mod(p,7*7)
			float4 x_ = floor( j / 7.0 );
			float4 y_ = floor( j - 7.0 * x_ );  // mod(j,N)
			float4 x = ( x_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 y = ( y_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 h = 1.0 - abs( x ) - abs( y );
			float4 b0 = float4( x.xy, y.xy );
			float4 b1 = float4( x.zw, y.zw );
			float4 s0 = floor( b0 ) * 2.0 + 1.0;
			float4 s1 = floor( b1 ) * 2.0 + 1.0;
			float4 sh = -step( h, 0.0 );
			float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
			float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
			float3 g0 = float3( a0.xy, h.x );
			float3 g1 = float3( a0.zw, h.y );
			float3 g2 = float3( a1.xy, h.z );
			float3 g3 = float3( a1.zw, h.w );
			float4 norm = taylorInvSqrt( float4( dot( g0, g0 ), dot( g1, g1 ), dot( g2, g2 ), dot( g3, g3 ) ) );
			g0 *= norm.x;
			g1 *= norm.y;
			g2 *= norm.z;
			g3 *= norm.w;
			float4 m = max( 0.6 - float4( dot( x0, x0 ), dot( x1, x1 ), dot( x2, x2 ), dot( x3, x3 ) ), 0.0 );
			m = m* m;
			m = m* m;
			float4 px = float4( dot( x0, g0 ), dot( x1, g1 ), dot( x2, g2 ), dot( x3, g3 ) );
			return 42.0 * dot( m, px);
		}


		inline float4 ASE_ComputeGrabScreenPos( float4 pos )
		{
			#if UNITY_UV_STARTS_AT_TOP
			float scale = -1.0;
			#else
			float scale = 1.0;
			#endif
			float4 o = pos;
			o.y = pos.w * 0.5f;
			o.y = ( pos.y - o.y ) * _ProjectionParams.x * scale + o.y;
			return o;
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float simplePerlin3D25 = snoise( ase_vertex3Pos*_DistortionAmount );
			simplePerlin3D25 = simplePerlin3D25*0.5 + 0.5;
			float simplePerlin3D18 = snoise( ( ase_vertex3Pos * ( simplePerlin3D25 * _DistortionScale ) )*_NoiseScale );
			simplePerlin3D18 = simplePerlin3D18*0.5 + 0.5;
			float smoothstepResult22 = smoothstep( -_NoiseStrength , _NoiseStrength , simplePerlin3D18);
			float temp_output_9_0 = ( ( ( 1.0 - _Smooth ) * smoothstepResult22 ) / 100.0 );
			float temp_output_3_0 = (-temp_output_9_0 + (( _Pos / 100.0 ) - 0.0) * (1.0 - -temp_output_9_0) / (1.0 - 0.0));
			float smoothstepResult5 = smoothstep( temp_output_3_0 , ( temp_output_3_0 + temp_output_9_0 ) , ase_vertex3Pos.y);
			float clampResult70 = clamp( smoothstepResult5 , 0.0 , 1.0 );
			float ColorBlend41 = clampResult70;
			float4 temp_cast_0 = (ColorBlend41).xxxx;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( ase_screenPos );
			float4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
			float4 screenColor35 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,ase_grabScreenPosNorm.xy/ase_grabScreenPosNorm.w);
			float4 lerpResult54 = lerp( _TopColor , _BottomColor , ColorBlend41);
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth43 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth43 = abs( ( screenDepth43 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( ( abs( _Depth ) / 5000.0 ) ) );
			float smoothstepResult51 = smoothstep( 0.0 , ( _DepthSmooth / 10.0 ) , distanceDepth43);
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV52 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode52 = ( ( _Bias / 100.0 ) + _Scale * pow( 1.0 - fresnelNdotV52, _Power ) );
			float clampResult62 = clamp( ( smoothstepResult51 + fresnelNode52 ) , 0.0 , 1.0 );
			float Depth80 = clampResult62;
			float4 lerpResult82 = lerp( screenColor35 , lerpResult54 , Depth80);
			float4 ifLocalVar73 = 0;
			if( _ViewBlend > 0.5 )
				ifLocalVar73 = temp_cast_0;
			else if( _ViewBlend < 0.5 )
				ifLocalVar73 = lerpResult82;
			o.Albedo = ifLocalVar73.rgb;
			o.Smoothness = 0.0;
			float lerpResult68 = lerp( _TopColor.a , _BottomColor.a , ColorBlend41);
			float ColorBlend_Alpha106 = lerpResult68;
			float ifLocalVar75 = 0;
			if( _ViewBlend > 0.5 )
				ifLocalVar75 = 1.0;
			else if( _ViewBlend < 0.5 )
				ifLocalVar75 = ( ColorBlend_Alpha106 * Depth80 );
			o.Alpha = ifLocalVar75;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18935
-1913;-11;1909;1051;1367.979;679.7458;1;True;False
Node;AmplifyShaderEditor.RangedFloatNode;30;-4159.741,1213.79;Inherit;False;Property;_DistortionAmount;Distortion Amount;12;0;Create;True;0;0;0;False;0;False;0;20;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;19;-4133.581,1065.218;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;31;-3946.742,1230.79;Inherit;False;Property;_DistortionScale;Distortion Scale;13;0;Create;True;0;0;0;False;0;False;0;3.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;25;-3946.562,1133.295;Inherit;False;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-3769.005,1130.351;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;20;-3621.526,1163.235;Inherit;False;Property;_NoiseScale;Noise Scale;11;0;Create;True;0;0;0;False;0;False;1;20;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;23;-3613.526,1245.235;Inherit;False;Property;_NoiseStrength;Noise Strength;10;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-3620.681,1067.141;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-3201.213,969.7738;Inherit;False;Property;_Smooth;Smooth;9;0;Create;True;0;0;0;False;0;False;0;8.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;24;-3351.141,1167.18;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;18;-3434.681,1063.141;Inherit;False;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;71;-3206.136,1038.347;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;22;-3225.681,1140.141;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-3063.68,1034.141;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;9;-2939.213,1033.774;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;100;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-2952.213,906.7739;Inherit;False;Property;_Pos;Pos;8;0;Create;True;0;0;0;False;0;False;0;-5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;6;-2826.213,911.7739;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;100;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;10;-2832.213,1012.774;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;3;-2707.213,909.7739;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;45;-1633.12,485.7468;Inherit;False;Property;_Depth;Depth;2;0;Create;True;0;0;0;False;0;False;0;36;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;1;-2704.413,759.6741;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;17;-2530.68,1008.141;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;48;-1487.121,490.7467;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;63;-1400.563,719.3249;Inherit;False;Property;_Bias;Bias;4;0;Create;True;0;0;0;False;0;False;0;-13.6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;46;-1376.121,491.7467;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;5000;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;49;-1309.12,591.7465;Inherit;False;Property;_DepthSmooth;Depth Smooth;3;0;Create;True;0;0;0;False;0;False;0;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;5;-2414.813,886.7739;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;50;-1143.12,588.7465;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;67;-1237.563,727.325;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;100;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;64;-1245.563,819.3253;Inherit;False;Property;_Scale;Scale;5;0;Create;True;0;0;0;False;0;False;0;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;65;-1245.563,900.3253;Inherit;False;Property;_Power;Power;6;0;Create;True;0;0;0;False;0;False;0;1.51;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;43;-1260.516,467.1641;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;70;-2264.587,891.256;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;41;-2125.833,884.3637;Inherit;False;ColorBlend;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;51;-1021.12,464.7468;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;52;-1091.82,752.0471;Inherit;False;Standard;TangentNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;55;-869.0433,-116.8013;Inherit;False;41;ColorBlend;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;53;-874.7787,-307.8589;Inherit;False;Property;_BottomColor;Bottom Color;1;0;Create;True;0;0;0;False;0;False;1,1,1,1;0.8509804,0,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;56;-873.0434,-482.8015;Inherit;False;Property;_TopColor;Top Color;0;0;Create;True;0;0;0;False;0;False;1,1,1,1;0.9058824,0.7882354,0.7764707,0.3490196;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;66;-836.9627,545.3248;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;62;-704.5627,467.3248;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;68;-627.5872,-197.744;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;80;-554.7289,456.0631;Inherit;False;Depth;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GrabScreenPosition;37;-916.4476,-677.2311;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;106;-429.8652,-84.94202;Inherit;False;ColorBlend_Alpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;79;-749.8785,-13.45905;Inherit;False;80;Depth;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;54;-627.8801,-325.0515;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ScreenColorNode;35;-662.6962,-680.1652;Inherit;False;Global;_GrabScreen1;Grab Screen 1;8;0;Create;True;0;0;0;False;0;False;Object;-1;False;True;False;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;105;-103.8652,316.058;Inherit;False;80;Depth;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;108;-150.8652,231.058;Inherit;False;106;ColorBlend_Alpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;69;90.57552,240.8078;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;72;-242.8745,-254.9095;Inherit;False;Property;_ViewBlend;View Blend;7;2;[Header];[Toggle];Create;True;1;Blend;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;82;-418.6384,-333.9739;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;74;-254.8745,-403.9095;Inherit;False;41;ColorBlend;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;76;48.6814,141.7565;Inherit;False;Constant;_Opacityconst1;Opacity const 1;16;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;83;-3267.451,411.3777;Inherit;False;Property;_NormalStrength;Normal Strength;14;0;Create;True;0;0;0;False;0;False;1;40;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;109;-808.0044,665.8483;Inherit;False;Fresnel;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;73;-51.87451,-470.9095;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.PosVertexDataNode;110;-988.3306,85.07703;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;78;8.034745,-164.8958;Inherit;False;Constant;_Smoothness;Smoothness;2;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;101;-425.8652,-189.942;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;75;240.6814,107.7565;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;239.5054,-329.9717;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;VRCCoffee/Ade;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;1;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;Opaque;;AlphaTest;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;2;5;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;15;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;25;0;19;0
WireConnection;25;1;30;0
WireConnection;29;0;25;0
WireConnection;29;1;31;0
WireConnection;26;0;19;0
WireConnection;26;1;29;0
WireConnection;24;0;23;0
WireConnection;18;0;26;0
WireConnection;18;1;20;0
WireConnection;71;0;8;0
WireConnection;22;0;18;0
WireConnection;22;1;24;0
WireConnection;22;2;23;0
WireConnection;21;0;71;0
WireConnection;21;1;22;0
WireConnection;9;0;21;0
WireConnection;6;0;7;0
WireConnection;10;0;9;0
WireConnection;3;0;6;0
WireConnection;3;3;10;0
WireConnection;17;0;3;0
WireConnection;17;1;9;0
WireConnection;48;0;45;0
WireConnection;46;0;48;0
WireConnection;5;0;1;2
WireConnection;5;1;3;0
WireConnection;5;2;17;0
WireConnection;50;0;49;0
WireConnection;67;0;63;0
WireConnection;43;0;46;0
WireConnection;70;0;5;0
WireConnection;41;0;70;0
WireConnection;51;0;43;0
WireConnection;51;2;50;0
WireConnection;52;1;67;0
WireConnection;52;2;64;0
WireConnection;52;3;65;0
WireConnection;66;0;51;0
WireConnection;66;1;52;0
WireConnection;62;0;66;0
WireConnection;68;0;56;4
WireConnection;68;1;53;4
WireConnection;68;2;55;0
WireConnection;80;0;62;0
WireConnection;106;0;68;0
WireConnection;54;0;56;0
WireConnection;54;1;53;0
WireConnection;54;2;55;0
WireConnection;35;0;37;0
WireConnection;69;0;108;0
WireConnection;69;1;105;0
WireConnection;82;0;35;0
WireConnection;82;1;54;0
WireConnection;82;2;79;0
WireConnection;109;0;52;0
WireConnection;73;0;72;0
WireConnection;73;2;74;0
WireConnection;73;4;82;0
WireConnection;101;0;68;0
WireConnection;101;1;79;0
WireConnection;75;0;72;0
WireConnection;75;2;76;0
WireConnection;75;4;69;0
WireConnection;0;0;73;0
WireConnection;0;4;78;0
WireConnection;0;9;75;0
ASEEND*/
//CHKSM=E9B46B701BB2A5FD68DCCE6D9A884EFCE8E2A366