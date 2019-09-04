Shader "Omni/VRIME/Key"
{
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_SideColor ("Secondary Color", Color) = (1,1,1,1)
		_Clip ("Clip", Range(0, 1)) = 0.5
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_fog

				#include "UnityCG.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f {
					float4 vertex : SV_POSITION;
	                float2 texcoord : TEXCOORD0;
					UNITY_FOG_COORDS(0)
					UNITY_VERTEX_OUTPUT_STEREO
				};

				fixed4 _Color;
				fixed4 _SideColor;
				float _Clip;

				v2f vert (appdata_t v)
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.texcoord = v.texcoord;
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag (v2f i) : COLOR
				{
					float s = (sign(i.texcoord.x - _Clip) + 1) / 2;
					fixed4 col = lerp(_Color, _SideColor, s);
					UNITY_APPLY_FOG(i.fogCoord, col);
					UNITY_OPAQUE_ALPHA(col.a);
					return col;
				}
			ENDCG
		}
	}
}
