// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "VRCCoffee/Ice"
{
	Properties
	{
		_Normal("Normal", 2D) = "bump" {}
		_NormalStrength("Normal Strength", Float) = 1
		_Smooth("Smooth", Range( 0 , 1)) = 0
		_RefractionScale("Refraction Scale", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		ZWrite On
		Blend SrcAlpha OneMinusSrcAlpha
		
		GrabPass{ }
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex);
		#else
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
		#endif
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows noshadow 
		struct Input
		{
			float4 screenPos;
			float2 uv_texcoord;
		};

		ASE_DECLARE_SCREENSPACE_TEXTURE( _GrabTexture )
		uniform sampler2D _Normal;
		uniform half4 _Normal_ST;
		uniform half _NormalStrength;
		uniform half _RefractionScale;
		uniform half _Smooth;


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
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( ase_screenPos );
			half4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			half4 screenColor26 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,( ase_grabScreenPosNorm + half4( ( UnpackScaleNormal( tex2D( _Normal, uv_Normal ), _NormalStrength ) * _RefractionScale ) , 0.0 ) ).xy/( ase_grabScreenPosNorm + half4( ( UnpackScaleNormal( tex2D( _Normal, uv_Normal ), _NormalStrength ) * _RefractionScale ) , 0.0 ) ).w);
			o.Emission = screenColor26.rgb;
			o.Smoothness = _Smooth;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18935
-1913;-11;1909;1051;1645.075;222.3707;1;True;False
Node;AmplifyShaderEditor.RangedFloatNode;5;-1232,352;Inherit;False;Property;_NormalStrength;Normal Strength;1;0;Create;True;0;0;0;False;0;False;1;40;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-1024,272;Inherit;True;Property;_Normal;Normal;0;0;Create;True;0;0;0;False;0;False;-1;None;1d366143962abf94985391189265f435;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;39.8;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;30;-855.7876,56.63216;Inherit;False;Property;_RefractionScale;Refraction Scale;3;0;Create;True;0;0;0;False;0;False;0;0.015;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GrabScreenPosition;43;-1077.387,-38.9177;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-531.9305,47.79207;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;28;-364.0905,-38.27208;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ScreenColorNode;26;-222.3004,-118.0464;Inherit;False;Global;_GrabScreen0;Grab Screen 0;4;0;Create;True;0;0;0;True;0;False;Object;-1;False;True;False;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;33;-828.8977,-249.1816;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;35;-509.5939,-252.9772;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.OneMinusNode;36;-678.6319,-319.5656;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;3;-1008,480;Inherit;False;Property;_Smooth;Smooth;2;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;391.3,3.9;Half;False;True;-1;2;ASEMaterialInspector;0;0;Standard;VRCCoffee/Ice;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;1;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;3;Opaque;0.5;True;True;0;True;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;4;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;2;5;5;0
WireConnection;29;0;2;0
WireConnection;29;1;30;0
WireConnection;28;0;43;0
WireConnection;28;1;29;0
WireConnection;26;0;28;0
WireConnection;33;0;43;0
WireConnection;35;0;33;0
WireConnection;35;1;33;1
WireConnection;35;2;36;0
WireConnection;35;3;33;3
WireConnection;36;0;33;2
WireConnection;0;2;26;0
WireConnection;0;4;3;0
ASEEND*/
//CHKSM=AE3AD160C299D456CEA7E5D15CA587B663DDED4A