// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "VRCCoffee/Americano"
{
	Properties
	{
		_BaseColor("Base Color", Color) = (1,1,1,0)
		_ContactColor("Contact Color", Color) = (1,1,1,0)
		_Smooth("Smooth", Range( 0 , 1)) = 1
		_Distance("Distance", Float) = 1
		_EdgeSharp("Edge Sharp", Float) = 1
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IgnoreProjector" = "True" }
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha , SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#include "UnityCG.cginc"
		#pragma target 2.0
		#pragma surface surf Standard keepalpha noshadow 
		struct Input
		{
			float4 screenPos;
		};

		uniform float4 _BaseColor;
		uniform float4 _ContactColor;
		uniform float _EdgeSharp;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _Distance;
		uniform float _Smooth;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth17 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth17 = abs( ( screenDepth17 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( ( abs( _Distance ) / 5000.0 ) ) );
			float smoothstepResult43 = smoothstep( 0.0 , ( _EdgeSharp / 10.0 ) , ( 1.0 - distanceDepth17 ));
			float4 lerpResult38 = lerp( _BaseColor , _ContactColor , smoothstepResult43);
			o.Albedo = lerpResult38.rgb;
			o.Smoothness = _Smooth;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18935
-1920;-14;1920;1059;2512.925;662.5521;1.652835;True;False
Node;AmplifyShaderEditor.RangedFloatNode;16;-1489.69,172.8239;Inherit;False;Property;_Distance;Distance;3;0;Create;True;0;0;0;False;0;False;1;70;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;34;-1338.39,172.8639;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;30;-1223.39,176.8639;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;5000;False;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;17;-1103.693,176.8239;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;42;-1023.355,283.6104;Inherit;False;Property;_EdgeSharp;Edge Sharp;4;0;Create;True;0;0;0;False;0;False;1;20;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;18;-867.234,176.8239;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;44;-844.5198,256.7563;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;36;-763.7207,-179.2816;Inherit;False;Property;_BaseColor;Base Color;0;0;Create;True;0;0;0;False;0;False;1,1,1,0;0.2879997,0.05279978,0,0.5607843;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;37;-758.0769,-0.948122;Inherit;False;Property;_ContactColor;Contact Color;1;0;Create;True;0;0;0;False;0;False;1,1,1,0;0.539,0.1911347,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;43;-714.5199,180.7563;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-382.0767,157.5693;Inherit;False;Property;_Smooth;Smooth;2;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;38;-506.3781,-59.64012;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-5,8;Float;False;True;-1;0;ASEMaterialInspector;0;0;Standard;VRCCoffee/Americano;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;False;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;2;5;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;34;0;16;0
WireConnection;30;0;34;0
WireConnection;17;0;30;0
WireConnection;18;0;17;0
WireConnection;44;0;42;0
WireConnection;43;0;18;0
WireConnection;43;2;44;0
WireConnection;38;0;36;0
WireConnection;38;1;37;0
WireConnection;38;2;43;0
WireConnection;0;0;38;0
WireConnection;0;4;8;0
ASEEND*/
//CHKSM=131F8FCA899FBECEECBCCB0980C13B554B5E9795