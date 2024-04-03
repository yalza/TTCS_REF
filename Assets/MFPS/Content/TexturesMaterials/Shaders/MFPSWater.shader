// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MFPS/MFPSWater"
{
	Properties
	{
		_Depth("Depth", Float) = 1
		_Streght("Streght", Range( 0 , 2)) = 0
		_DepthColor("DepthColor", Color) = (0.4575472,0.8941401,1,0.6)
		_ShallowColor("ShallowColor", Color) = (0.365655,0.8946645,0.9339623,0.4901961)
		[SingleLineTexture]_MainPattern("MainPattern", 2D) = "bump" {}
		[SingleLineTexture]_SecondaryPattern("SecondaryPattern", 2D) = "bump" {}
		_PatternStreght("PatternStreght", Range( 0 , 1)) = 0.5
		_Smoothness("Smoothness", Range( 0 , 1)) = 0.5
		_Layer1Speed("Layer 1 Speed", Float) = 85
		_Layer2Speed("Layer 2 Speed", Float) = -120
		_Displassment("Displassment", Float) = 0
		_NoiseScale("NoiseScale", Float) = 20
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#pragma surface surf Standard alpha:fade keepalpha noshadow vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_MainPattern;
			float2 uv_SecondaryPattern;
			float4 screenPos;
		};

		uniform float _Displassment;
		uniform float _Layer1Speed;
		uniform float _Layer2Speed;
		uniform float _NoiseScale;
		uniform sampler2D _MainPattern;
		uniform sampler2D _SecondaryPattern;
		uniform float _PatternStreght;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _Depth;
		uniform float _Streght;
		uniform float4 _ShallowColor;
		uniform float4 _DepthColor;
		uniform float _Smoothness;


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float4 matrixToPos29 = float4( float4x4( 1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1 )[3][0],float4x4( 1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1 )[3][1],float4x4( 1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1 )[3][2],float4x4( 1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1 )[3][3]);
			float2 temp_cast_0 = (( _Time.y / 100.0 )).xx;
			float2 uv_TexCoord35 = v.texcoord.xy + temp_cast_0;
			float simplePerlin2D31 = snoise( uv_TexCoord35*_NoiseScale );
			float4 appendResult33 = (float4(matrixToPos29.x , ( _Displassment * simplePerlin2D31 ) , matrixToPos29.z , 0.0));
			v.vertex.xyz += appendResult33.xyz;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 temp_cast_0 = (( _Time.y / _Layer1Speed)).xx;
			float2 uv_TexCoord18 = i.uv_MainPattern + temp_cast_0;
			float2 temp_cast_1 = (( _Time.y / _Layer2Speed)).xx;
			float2 uv_TexCoord19 = i.uv_SecondaryPattern + temp_cast_1;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float eyeDepth7 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float clampResult10 = clamp( ( ( eyeDepth7 - ( ase_screenPos.w + _Depth ) ) * _Streght ) , 0.0 , 1.0 );
			float lerpResult26 = lerp( 0.0 , _PatternStreght , clampResult10);
			o.Normal = UnpackScaleNormal( float4( ( UnpackNormal( tex2D( _MainPattern, uv_TexCoord18 ) ) + UnpackNormal( tex2D( _SecondaryPattern, uv_TexCoord19 ) ) ) , 0.0 ), lerpResult26 );
			float4 lerpResult13 = lerp( _ShallowColor , _DepthColor , clampResult10);
			o.Albedo = lerpResult13.rgb;
			o.Smoothness = _Smoothness;
			o.Alpha = (lerpResult13).a;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18100
1927;7;1906;1004;2328.468;372.3818;1.41245;True;True
Node;AmplifyShaderEditor.RangedFloatNode;5;-1900.614,701.7088;Inherit;False;Property;_Depth;Depth;0;0;Create;True;0;0;False;0;False;1;0.4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;3;-1935.343,513.4385;Float;False;1;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;20;-2223.848,-543.1511;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;36;-2123.943,-98.00996;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenDepthNode;7;-1933.644,389.3867;Inherit;False;0;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;4;-1675.236,620.3552;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;37;-1933.905,-70.4866;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;100;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-1568.457,723.5465;Inherit;False;Property;_Streght;Streght;1;0;Create;True;0;0;False;0;False;0;1.48;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;21;-1962.042,-473.4578;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;-70;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;22;-1936.437,-642.652;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;100;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;6;-1507.631,500.9945;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-1289.895,566.0258;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;35;-1781.87,-147.8142;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;18;-1746.251,-660.522;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;30,30;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;38;-1748.848,-12.75094;Inherit;False;Property;_NoiseScale;NoiseScale;9;0;Create;True;0;0;False;0;False;20;12;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;19;-1747.194,-491.3737;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;20,20;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;10;-1110.819,567.684;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-1532.225,-257.4465;Inherit;False;Property;_Displassment;Displassment;8;0;Create;True;0;0;False;0;False;0;0.007;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;31;-1526.224,-165.496;Inherit;False;Simplex2D;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;20;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-974.1615,-426.1134;Inherit;False;Property;_PatternStreght;PatternStreght;6;0;Create;True;0;0;False;0;False;0.5;0.119;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;12;-1357.877,161.4467;Inherit;False;Property;_ShallowColor;ShallowColor;3;0;Create;True;0;0;False;0;False;0.365655,0.8946645,0.9339623,0.4901961;0,0.01590518,0.03773582,0.4117647;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;16;-1323.381,-745.0962;Inherit;True;Property;_MainPattern;MainPattern;4;1;[SingleLineTexture];Create;True;0;0;False;0;False;-1;None;0d51f6e805dbeaf4dbe241170664f613;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;11;-1359.536,318.9677;Inherit;False;Property;_DepthColor;DepthColor;2;0;Create;True;0;0;False;0;False;0.4575472,0.8941401,1,0.6;0,0.1190885,0.254717,0.8862745;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;17;-1320.892,-526.1982;Inherit;True;Property;_SecondaryPattern;SecondaryPattern;5;1;[SingleLineTexture];Create;True;0;0;False;0;False;-1;None;0d51f6e805dbeaf4dbe241170664f613;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;26;-693.6328,-447.6063;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-1306.195,-145.7264;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;23;-933.1246,-583.8357;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;13;-958.2731,388.6077;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.PosFromTransformMatrix;29;-1528.754,-56.55008;Inherit;False;1;0;FLOAT4x4;1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.UnpackScaleNormalNode;24;-774.9297,-601.4113;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;33;-1122.895,-147.0266;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;27;-733.7954,-174.9428;Inherit;False;Property;_Smoothness;Smoothness;7;0;Create;True;0;0;False;0;False;0.5;0.879;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;15;-787.4875,590.8975;Inherit;False;FLOAT;3;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-325.6589,-285.2213;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;MFPS/MFPSWater;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;4;0;3;4
WireConnection;4;1;5;0
WireConnection;37;0;36;0
WireConnection;21;0;20;0
WireConnection;22;0;20;0
WireConnection;6;0;7;0
WireConnection;6;1;4;0
WireConnection;8;0;6;0
WireConnection;8;1;9;0
WireConnection;35;1;37;0
WireConnection;18;1;22;0
WireConnection;19;1;21;0
WireConnection;10;0;8;0
WireConnection;31;0;35;0
WireConnection;31;1;38;0
WireConnection;16;1;18;0
WireConnection;17;1;19;0
WireConnection;26;1;25;0
WireConnection;26;2;10;0
WireConnection;32;0;30;0
WireConnection;32;1;31;0
WireConnection;23;0;16;0
WireConnection;23;1;17;0
WireConnection;13;0;12;0
WireConnection;13;1;11;0
WireConnection;13;2;10;0
WireConnection;24;0;23;0
WireConnection;24;1;26;0
WireConnection;33;0;29;1
WireConnection;33;1;32;0
WireConnection;33;2;29;3
WireConnection;15;0;13;0
WireConnection;0;0;13;0
WireConnection;0;1;24;0
WireConnection;0;4;27;0
WireConnection;0;9;15;0
WireConnection;0;11;33;0
ASEEND*/
//CHKSM=1F6FD65780EBBEBBE9249F7D9934AC66828C158F