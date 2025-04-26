// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "VRCCoffee/Iced_Macchiato"
{
	Properties
	{
		_ColorBase("Color Base", Color) = (1,1,1,1)
		_ColorBaseSub("Color Base Sub", Color) = (1,1,1,1)
		_ColorNoiseScale("Color Noise Scale", Float) = 0
		_ColorDistortionScale("Color Distortion Scale", Float) = 0
		_ColorBotm("Color Botm", Color) = (1,1,1,1)
		_Pos("Pos", Float) = 0
		_Grad("Grad", Float) = 0
		_NoiseScale("Noise Scale", Float) = 0
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

		uniform float4 _ColorBase;
		uniform float4 _ColorBaseSub;
		uniform float _ColorDistortionScale;
		uniform float _ColorNoiseScale;
		uniform float4 _ColorBotm;
		uniform float _Pos;
		uniform float _Grad;
		uniform float _NoiseScale;
		uniform float _Contact_Sharp;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _Contact_Distance;


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


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float simplePerlin3D28 = snoise( ase_vertex3Pos*_ColorDistortionScale );
			simplePerlin3D28 = simplePerlin3D28*0.5 + 0.5;
			float simplePerlin3D25 = snoise( ( ase_vertex3Pos * simplePerlin3D28 )*_ColorNoiseScale );
			simplePerlin3D25 = simplePerlin3D25*0.5 + 0.5;
			float4 lerpResult26 = lerp( _ColorBase , _ColorBaseSub , simplePerlin3D25);
			float simplePerlin3D15 = snoise( ase_vertex3Pos*_NoiseScale );
			simplePerlin3D15 = simplePerlin3D15*0.5 + 0.5;
			float temp_output_9_0 = ( pow( _Grad , simplePerlin3D15 ) / 100.0 );
			float temp_output_5_0 = (-temp_output_9_0 + (( _Pos / 100.0 ) - 0.0) * (1.0 - -temp_output_9_0) / (1.0 - 0.0));
			float smoothstepResult6 = smoothstep( temp_output_5_0 , ( temp_output_5_0 + temp_output_9_0 ) , ase_vertex3Pos.y);
			float4 lerpResult13 = lerp( lerpResult26 , _ColorBotm , ( 1.0 - smoothstepResult6 ));
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth35 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth35 = abs( ( screenDepth35 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( ( abs( _Contact_Distance ) / 5000.0 ) ) );
			float smoothstepResult38 = smoothstep( 0.0 , _Contact_Sharp , ( 1.0 - distanceDepth35 ));
			float clampResult39 = clamp( smoothstepResult38 , 0.0 , 1.0 );
			float Contact40 = clampResult39;
			float4 temp_cast_0 = (Contact40).xxxx;
			o.Albedo = ( lerpResult13 - temp_cast_0 ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18935
-1899;5;1873;1047;2295.216;887.3746;1;True;False
Node;AmplifyShaderEditor.PosVertexDataNode;18;-1716.514,255.8806;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;17;-1717.514,403.8806;Inherit;False;Property;_NoiseScale;Noise Scale;7;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;31;-2871.659,-1430.303;Inherit;False;1429.524;289.5157;Depth;9;40;39;38;37;36;35;34;33;32;;1,1,1,1;0;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;15;-1522.514,330.8807;Inherit;False;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-1478.5,244.5;Inherit;False;Property;_Grad;Grad;6;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;32;-2843.659,-1366.574;Inherit;False;Property;_Contact_Distance;Contact_Distance;8;1;[Header];Create;True;1;Ice Contact;0;0;False;0;False;1;7;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;21;-1339.226,271.8143;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;33;-2649.359,-1367.534;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;34;-2520.359,-1363.534;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;5000;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;9;-1196.687,269.4215;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;100;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;2;-1126.5,138.5;Inherit;False;Property;_Pos;Pos;5;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;10;-1000.687,231.4215;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;35;-2400.662,-1363.574;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;23;-1841.659,-369.519;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;30;-1840.326,-218.1857;Inherit;False;Property;_ColorDistortionScale;Color Distortion Scale;3;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;4;-993.5,138.5;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;100;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;5;-872.5,138.5;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-2286.324,-1235.787;Inherit;False;Property;_Contact_Sharp;Contact_Sharp;9;0;Create;True;0;0;0;False;0;False;0.5;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;37;-2164.203,-1363.574;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;28;-1644.326,-302.1857;Inherit;False;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;1;-873.5,-10.5;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;38;-1986.773,-1373.303;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;7;-701.5,234.5;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-1462.659,-261.519;Inherit;False;Property;_ColorNoiseScale;Color Noise Scale;2;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-1462.326,-368.1857;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;25;-1260.659,-337.519;Inherit;False;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;39;-1806.836,-1358.121;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;6;-592.5,113.5;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;27;-1264.326,-520.1856;Inherit;False;Property;_ColorBaseSub;Color Base Sub;1;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;11;-1270.787,-700.5784;Inherit;False;Property;_ColorBase;Color Base;0;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;12;-672.6869,-173.5785;Inherit;False;Property;_ColorBotm;Color Botm;4;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;14;-443.6869,117.4215;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;40;-1666.138,-1360.161;Inherit;False;Contact;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;26;-921.3257,-508.1856;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;42;-304.2158,14.62537;Inherit;False;40;Contact;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;13;-306.6869,-107.5785;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;41;-90.21582,-99.37463;Inherit;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;64,-101;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;VRCCoffee/Iced_Macchiato;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;False;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;15;0;18;0
WireConnection;15;1;17;0
WireConnection;21;0;8;0
WireConnection;21;1;15;0
WireConnection;33;0;32;0
WireConnection;34;0;33;0
WireConnection;9;0;21;0
WireConnection;10;0;9;0
WireConnection;35;0;34;0
WireConnection;4;0;2;0
WireConnection;5;0;4;0
WireConnection;5;3;10;0
WireConnection;37;0;35;0
WireConnection;28;0;23;0
WireConnection;28;1;30;0
WireConnection;38;0;37;0
WireConnection;38;2;36;0
WireConnection;7;0;5;0
WireConnection;7;1;9;0
WireConnection;29;0;23;0
WireConnection;29;1;28;0
WireConnection;25;0;29;0
WireConnection;25;1;24;0
WireConnection;39;0;38;0
WireConnection;6;0;1;2
WireConnection;6;1;5;0
WireConnection;6;2;7;0
WireConnection;14;0;6;0
WireConnection;40;0;39;0
WireConnection;26;0;11;0
WireConnection;26;1;27;0
WireConnection;26;2;25;0
WireConnection;13;0;26;0
WireConnection;13;1;12;0
WireConnection;13;2;14;0
WireConnection;41;0;13;0
WireConnection;41;1;42;0
WireConnection;0;0;41;0
ASEEND*/
//CHKSM=40405049D5EECB7BF7D9DC69C2BCFB4122897A58