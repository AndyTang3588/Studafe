// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "VRCCoffee/Mocha_Iced"
{
	Properties
	{
		_Color_Top("Color_Top", Color) = (1,1,1,1)
		_Color_Blend("Color_Blend", Color) = (1,1,1,1)
		_Color_Botm("Color_Botm", Color) = (1,1,1,1)
		_BotmPos("Botm Pos", Float) = 0
		_BotmGrad("Botm Grad", Range( 0 , 0.5)) = 0.1
		_BlendGrad("Blend Grad", Float) = 0.1
		_Smooth("Smooth", Range( 0 , 1)) = 0
		[Header(Ice Contact)]_Contact_Distance("Contact_Distance", Float) = 1
		_Contact_Sharp("Contact_Sharp", Float) = 0.5
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha noshadow 
		struct Input
		{
			float3 worldPos;
			float4 screenPos;
		};

		uniform float4 _Color_Top;
		uniform float4 _Color_Blend;
		uniform float _BotmPos;
		uniform float _BotmGrad;
		uniform float _BlendGrad;
		uniform float4 _Color_Botm;
		uniform float _Contact_Sharp;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _Contact_Distance;
		uniform float _Smooth;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float temp_output_90_0 = ( _BotmPos / 100.0 );
			float temp_output_92_0 = (_BotmGrad + (temp_output_90_0 - 0.0) * (1.0 - _BotmGrad) / (1.0 - 0.0));
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float smoothstepResult96 = smoothstep( temp_output_92_0 , ( temp_output_92_0 + -_BotmGrad ) , ase_vertex3Pos.y);
			float smoothstepResult103 = smoothstep( temp_output_92_0 , ( temp_output_90_0 + ( _BlendGrad / 100.0 ) ) , ase_vertex3Pos.y);
			float clampResult114 = clamp( ( smoothstepResult96 + smoothstepResult103 ) , 0.0 , 1.0 );
			float4 lerpResult111 = lerp( _Color_Top , _Color_Blend , clampResult114);
			float4 lerpResult112 = lerp( lerpResult111 , _Color_Botm , smoothstepResult96);
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth56 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth56 = abs( ( screenDepth56 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( ( abs( _Contact_Distance ) / 5000.0 ) ) );
			float smoothstepResult59 = smoothstep( 0.0 , _Contact_Sharp , ( 1.0 - distanceDepth56 ));
			float clampResult60 = clamp( smoothstepResult59 , 0.0 , 1.0 );
			float Contact61 = clampResult60;
			float4 temp_cast_0 = (Contact61).xxxx;
			o.Albedo = ( lerpResult112 - temp_cast_0 ).rgb;
			o.Smoothness = _Smooth;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18935
-1896;-19;1873;1053;2508.375;1266.727;1.996996;True;False
Node;AmplifyShaderEditor.CommentaryNode;52;-1483.436,-1106.578;Inherit;False;1429.524;289.5157;Depth;9;61;60;59;58;57;56;55;54;53;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;53;-1455.435,-1042.848;Inherit;False;Property;_Contact_Distance;Contact_Distance;7;1;[Header];Create;True;1;Ice Contact;0;0;False;0;False;1;393.4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;89;-1746.157,-376.3397;Inherit;False;Property;_BotmPos;Botm Pos;3;0;Create;True;0;0;0;False;0;False;0;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;90;-1605.094,-378.6024;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;100;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;91;-1758.834,-279.3172;Inherit;False;Property;_BotmGrad;Botm Grad;4;0;Create;True;0;0;0;False;0;False;0.1;0.5;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;125;-1485.34,-68.6087;Inherit;False;Property;_BlendGrad;Blend Grad;5;0;Create;True;0;0;0;False;0;False;0.1;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;54;-1261.135,-1043.808;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;92;-1351.26,-377.9403;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;93;-1346.734,-211.7044;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;129;-1429.548,-156.7762;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;127;-1336.34,-74.6087;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;100;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;55;-1132.135,-1039.808;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;5000;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;95;-1161.644,-239.5143;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;126;-1161.34,-139.6087;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;56;-1012.438,-1039.848;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;94;-1348.594,-532.7035;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;96;-1031.682,-427.1352;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;58;-898.0996,-912.0623;Inherit;False;Property;_Contact_Sharp;Contact_Sharp;8;0;Create;True;0;0;0;False;0;False;0.5;302.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;103;-1019.562,-295.1017;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;57;-775.979,-1039.848;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;113;-811.7294,-288.2223;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;59;-598.5488,-1049.578;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;108;-747.5957,-666.9691;Inherit;False;Property;_Color_Top;Color_Top;0;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;60;-418.6115,-1034.395;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;114;-688.7294,-290.2223;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;109;-751.4957,-475.8695;Inherit;False;Property;_Color_Blend;Color_Blend;1;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;111;-447.2947,-535.6695;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;110;-495.3954,-170.3685;Inherit;False;Property;_Color_Botm;Color_Botm;2;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;61;-277.9137,-1036.435;Inherit;False;Contact;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;112;-274.395,-252.4688;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;124;-146.3403,-65.6087;Inherit;False;61;Contact;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;123;95.23608,-124.7134;Inherit;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;49;-281.548,244.9188;Inherit;False;Property;_Smooth;Smooth;6;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;313.5511,-103.3685;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;VRCCoffee/Mocha_Iced;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;False;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;90;0;89;0
WireConnection;54;0;53;0
WireConnection;92;0;90;0
WireConnection;92;3;91;0
WireConnection;93;0;91;0
WireConnection;129;0;90;0
WireConnection;127;0;125;0
WireConnection;55;0;54;0
WireConnection;95;0;92;0
WireConnection;95;1;93;0
WireConnection;126;0;129;0
WireConnection;126;1;127;0
WireConnection;56;0;55;0
WireConnection;96;0;94;2
WireConnection;96;1;92;0
WireConnection;96;2;95;0
WireConnection;103;0;94;2
WireConnection;103;1;92;0
WireConnection;103;2;126;0
WireConnection;57;0;56;0
WireConnection;113;0;96;0
WireConnection;113;1;103;0
WireConnection;59;0;57;0
WireConnection;59;2;58;0
WireConnection;60;0;59;0
WireConnection;114;0;113;0
WireConnection;111;0;108;0
WireConnection;111;1;109;0
WireConnection;111;2;114;0
WireConnection;61;0;60;0
WireConnection;112;0;111;0
WireConnection;112;1;110;0
WireConnection;112;2;96;0
WireConnection;123;0;112;0
WireConnection;123;1;124;0
WireConnection;0;0;123;0
WireConnection;0;4;49;0
ASEEND*/
//CHKSM=EFC133964866F6F8DC33CD72F2C6048E16CD1142