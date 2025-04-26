// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "VRCCoffee/Iced_Espresso"
{
	Properties
	{
		_TopTexture("Top Texture", 2D) = "white" {}
		_ColorBase1("Color Base 1", Color) = (0,0,0,0)
		_ColorBaseMix("Color Base Mix", Color) = (0,0,0,0)
		_Mixpos("Mix pos", Float) = 0
		_MixGrad("Mix Grad", Range( 0 , 2)) = 0
		_MixNoiseScale("Mix Noise Scale", Float) = 0
		_ColorRim1("Color Rim 1", Color) = (0,0,0,0)
		_ColorRim2("Color Rim 2", Color) = (0,0,0,0)
		_RimNoiseScale("Rim Noise Scale", Float) = 0
		_RimNoiseStrength("Rim Noise Strength", Float) = 0
		_RimPos("Rim Pos", Float) = 0
		_RimGrad("Rim Grad", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha noshadow 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
			float3 worldNormal;
		};

		uniform float4 _ColorBaseMix;
		uniform float4 _ColorRim1;
		uniform float4 _ColorRim2;
		uniform float _RimNoiseScale;
		uniform float _RimNoiseStrength;
		uniform float _RimPos;
		uniform float _RimGrad;
		uniform float4 _ColorBase1;
		uniform float _Mixpos;
		uniform float _MixGrad;
		uniform float _MixNoiseScale;
		uniform sampler2D _TopTexture;
		uniform float4 _TopTexture_ST;


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
			float simplePerlin3D27 = snoise( ase_vertex3Pos*_RimNoiseScale );
			simplePerlin3D27 = simplePerlin3D27*0.5 + 0.5;
			float4 lerpResult30 = lerp( _ColorRim1 , _ColorRim2 , ( simplePerlin3D27 * _RimNoiseStrength ));
			float temp_output_18_0 = ( _RimGrad / 50.0 );
			float temp_output_11_0 = (-temp_output_18_0 + (( _RimPos / 100.0 ) - 0.0) * (1.0 - -temp_output_18_0) / (1.0 - 0.0));
			float smoothstepResult15 = smoothstep( temp_output_11_0 , ( temp_output_11_0 + temp_output_18_0 ) , ase_vertex3Pos.y);
			float4 lerpResult21 = lerp( _ColorBaseMix , lerpResult30 , smoothstepResult15);
			float simplePerlin3D57 = snoise( ase_vertex3Pos*_MixNoiseScale );
			simplePerlin3D57 = simplePerlin3D57*0.5 + 0.5;
			float temp_output_50_0 = ( ( _MixGrad * simplePerlin3D57 ) / 50.0 );
			float temp_output_46_0 = (temp_output_50_0 + (( ( _RimPos + _Mixpos ) / 100.0 ) - 0.0) * (1.0 - temp_output_50_0) / (1.0 - 0.0));
			float smoothstepResult54 = smoothstep( temp_output_46_0 , ( temp_output_46_0 + -temp_output_50_0 ) , ase_vertex3Pos.y);
			float4 lerpResult56 = lerp( lerpResult21 , _ColorBase1 , smoothstepResult54);
			float2 uv_TopTexture = i.uv_texcoord * _TopTexture_ST.xy + _TopTexture_ST.zw;
			float3 ase_worldNormal = i.worldNormal;
			float3 ase_vertexNormal = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) );
			ase_vertexNormal = normalize( ase_vertexNormal );
			float4 lerpResult62 = lerp( lerpResult56 , tex2D( _TopTexture, uv_TopTexture ) , ( smoothstepResult15 * saturate( ase_vertexNormal.y ) ));
			o.Albedo = lerpResult62.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18935
-1911;7;1898;1041;1315.22;-448.8564;1;True;True
Node;AmplifyShaderEditor.RangedFloatNode;12;-1830.604,868.2766;Inherit;False;Property;_RimGrad;Rim Grad;11;0;Create;True;0;0;0;False;0;False;0;0.101;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;58;-1778.735,1193.393;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;59;-1777.612,1341.861;Inherit;False;Property;_MixNoiseScale;Mix Noise Scale;5;0;Create;True;0;0;0;False;0;False;0;70;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-1638.775,739.3796;Inherit;False;Property;_RimPos;Rim Pos;10;0;Create;True;0;0;0;False;0;False;0;-4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;18;-1566.604,869.2766;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;50;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;52;-1666.451,1117.822;Inherit;False;Property;_MixGrad;Mix Grad;4;0;Create;True;0;0;0;False;0;False;0;1.266;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;47;-1524.203,1028.874;Inherit;False;Property;_Mixpos;Mix pos;3;0;Create;True;0;0;0;False;0;False;0;-1.7;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;57;-1581.567,1203.357;Inherit;False;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;26;-2577.1,652.9249;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;49;-1380.117,1004.823;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;-1391.612,1115.861;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;28;-2581.1,793.9245;Inherit;False;Property;_RimNoiseScale;Rim Noise Scale;8;0;Create;True;0;0;0;False;0;False;0;1200;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;17;-1448.604,747.2766;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;100;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;13;-1451.604,840.2766;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;48;-1262.032,1004.771;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;100;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;27;-2382.1,698.9249;Inherit;False;Simplex3D;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;32;-2391.839,808.3297;Inherit;False;Property;_RimNoiseStrength;Rim Noise Strength;9;0;Create;True;0;0;0;False;0;False;0;1.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;50;-1263.451,1114.822;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;50;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;11;-1324.604,748.2766;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;46;-1141.117,1005.823;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;29;-2255.1,517.9246;Inherit;False;Property;_ColorRim2;Color Rim 2;7;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.4666665,0.3058823,0.1254901,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;14;-1139.604,846.2766;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;-2187.839,707.33;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;51;-1139.451,1171.822;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;5;-2260.454,342.9616;Inherit;False;Property;_ColorRim1;Color Rim 1;6;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.5294118,0.3607842,0.1607842,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PosVertexDataNode;16;-1323.604,606.2767;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalVertexDataNode;1;-459.4268,1100.938;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;53;-949.6162,1117.822;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;33;-841.1267,367.8445;Inherit;False;Property;_ColorBaseMix;Color Base Mix;2;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.2769999,0.06633136,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;30;-2009.101,499.9246;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;15;-1016.603,723.2766;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;22;-627.9967,623.0223;Inherit;False;Property;_ColorBase1;Color Base 1;1;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.09999991,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;21;-579.162,486.623;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;54;-832.6162,982.8225;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;61;-289.3489,1149.229;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;56;-355.5718,651.9066;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;3;-370.3684,803.8953;Inherit;True;Property;_TopTexture;Top Texture;0;0;Create;True;0;0;0;False;0;False;-1;None;3b80f09a04cf0b94c882abbd02e76438;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;63;-71.57911,1065.126;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;62;130.6423,651.8315;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;338.725,648.4763;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;VRCCoffee/Iced_Espresso;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;False;0;False;Opaque;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;18;0;12;0
WireConnection;57;0;58;0
WireConnection;57;1;59;0
WireConnection;49;0;9;0
WireConnection;49;1;47;0
WireConnection;60;0;52;0
WireConnection;60;1;57;0
WireConnection;17;0;9;0
WireConnection;13;0;18;0
WireConnection;48;0;49;0
WireConnection;27;0;26;0
WireConnection;27;1;28;0
WireConnection;50;0;60;0
WireConnection;11;0;17;0
WireConnection;11;3;13;0
WireConnection;46;0;48;0
WireConnection;46;3;50;0
WireConnection;14;0;11;0
WireConnection;14;1;18;0
WireConnection;31;0;27;0
WireConnection;31;1;32;0
WireConnection;51;0;50;0
WireConnection;53;0;46;0
WireConnection;53;1;51;0
WireConnection;30;0;5;0
WireConnection;30;1;29;0
WireConnection;30;2;31;0
WireConnection;15;0;16;2
WireConnection;15;1;11;0
WireConnection;15;2;14;0
WireConnection;21;0;33;0
WireConnection;21;1;30;0
WireConnection;21;2;15;0
WireConnection;54;0;16;2
WireConnection;54;1;46;0
WireConnection;54;2;53;0
WireConnection;61;0;1;2
WireConnection;56;0;21;0
WireConnection;56;1;22;0
WireConnection;56;2;54;0
WireConnection;63;0;15;0
WireConnection;63;1;61;0
WireConnection;62;0;56;0
WireConnection;62;1;3;0
WireConnection;62;2;63;0
WireConnection;0;0;62;0
ASEEND*/
//CHKSM=EE2B3CE6573387CE749E34FBAE8D1A0CB10E96C7