// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Omni/TechMatte/FogWhite"
{
	Properties
	{
		_BaseColor("Base Color", Color) = (0.09803922,0.1098039,0.1098039,0)
		_Smoothness("Smoothness", Float) = 0.23
		_FresnelPower("Fresnel Power", Float) = 5
		_SheenSaturation("Sheen Saturation", Float) = 0
		_SheenTexture("Sheen Texture", 2D) = "white" {}
		_SheenColorAdd("Sheen Color Add", Color) = (0.454902,0.7607843,0.8,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 5.0
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			float2 uv_texcoord;
		};

		uniform float4 _BaseColor;
		uniform float _FresnelPower;
		uniform sampler2D _SheenTexture;
		uniform float4 _SheenColorAdd;
		uniform float _SheenSaturation;
		uniform float _Smoothness;


		float4 CalculateContrast( float contrastValue, float4 colorTarget )
		{
			float t = 0.5 * ( 1.0 - contrastValue );
			return mul( float4x4( contrastValue,0,0,t, 0,contrastValue,0,t, 0,0,contrastValue,t, 0,0,0,1 ), colorTarget );
		}

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			o.Albedo = _BaseColor.rgb;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNDotV4 = dot( normalize( ase_worldNormal ), ase_worldViewDir );
			float fresnelNode4 = ( 0.0 + 3.0 * pow( 1.0 - fresnelNDotV4, _FresnelPower ) );
			float4 temp_cast_1 = (fresnelNode4).xxxx;
			float4 blendOpSrc6 = temp_cast_1;
			float4 blendOpDest6 = ( tex2D( _SheenTexture, i.uv_texcoord ) + _SheenColorAdd );
			float3 desaturateInitialColor11 = ( saturate( ( blendOpSrc6 * blendOpDest6 ) )).rgb;
			float desaturateDot11 = dot( desaturateInitialColor11, float3( 0.299, 0.587, 0.114 ));
			float3 desaturateVar11 = lerp( desaturateInitialColor11, desaturateDot11.xxx, ( 1.0 - _SheenSaturation ) );
			o.Specular = CalculateContrast(0.6,float4( desaturateVar11 , 0.0 )).rgb;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma only_renderers d3d9 d3d11 glcore gles gles3 d3d11_9x 
		#pragma surface surf StandardSpecular keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float3 worldNormal : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				fixed3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			fixed4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				fixed3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
				SurfaceOutputStandardSpecular o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandardSpecular, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=15301
2567;29;1618;1090;1517.115;504.1444;1.215;True;False
Node;AmplifyShaderEditor.TextureCoordinatesNode;5;-1083.289,322.7651;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-847.1801,291.94;Float;True;Property;_SheenTexture;Sheen Texture;4;0;Create;True;0;0;False;0;167a184aacde8f64d9b6711c9b6e75fe;167a184aacde8f64d9b6711c9b6e75fe;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;19;-862.2302,130.0855;Float;False;Property;_FresnelPower;Fresnel Power;2;0;Create;True;0;0;False;0;5;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;15;-853.7939,496.8861;Float;False;Property;_SheenColorAdd;Sheen Color Add;5;0;Create;True;0;0;False;0;0.454902,0.7607843,0.8,0;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;17;-516.189,422.4704;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-571.324,250.8306;Float;False;Property;_SheenSaturation;Sheen Saturation;3;0;Create;True;0;0;False;0;0;8;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;4;-651.6711,97.29496;Float;False;Tangent;4;0;FLOAT3;0,0,1;False;1;FLOAT;0;False;2;FLOAT;3;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;20;-350.7148,221.2104;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;6;-399.0802,90.30008;Float;False;Multiply;True;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DesaturateOpNode;11;-161.54,65.55501;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;-0.7;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleContrastOpNode;16;36.55606,45.65045;Float;False;2;1;COLOR;0,0,0,0;False;0;FLOAT;0.6;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-63.47997,242.005;Float;False;Property;_Smoothness;Smoothness;1;0;Create;True;0;0;False;0;0.23;0.15;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;9;-491.6948,-125.985;Float;False;Property;_BaseColor;Base Color;0;0;Create;True;0;0;False;0;0.09803922,0.1098039,0.1098039,0;0.09803908,0.1098037,0.1098037,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;256.99,-24;Float;False;True;7;Float;ASEMaterialInspector;0;0;StandardSpecular;Omni/TechMatte/FogWhite;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;0;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;False;True;False;False;False;False;False;False;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;-1;False;-1;-1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;0;0;False;0;0;0;False;-1;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;1;1;5;0
WireConnection;17;0;1;0
WireConnection;17;1;15;0
WireConnection;4;3;19;0
WireConnection;20;0;18;0
WireConnection;6;0;4;0
WireConnection;6;1;17;0
WireConnection;11;0;6;0
WireConnection;11;1;20;0
WireConnection;16;1;11;0
WireConnection;0;0;9;0
WireConnection;0;3;16;0
WireConnection;0;4;7;0
ASEEND*/
//CHKSM=B064CD4CCD539ACEBB03FA1013285A5F52A93C75