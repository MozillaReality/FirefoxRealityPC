Shader "Omni/VRIME/Panel"
{
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_ShadowDir ("Shadow Direction", Vector) = (0,1,0,1)
		_ShadowIntensity ("Shadow Intensity", Range(0, 1)) = 0.5
		_ACRect ("Ambient Occlusion Rect", Vector) = (0,0,1,1)
		_ACIntensity ("Ambient Occlusion Intensity", Range(0, 1)) = 0.5
		_ACEdgeFeather("Ambient Occlusion Edge Feather", Range(0, 1)) = 0
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
					float4 normal : NORMAL;
					float2 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f {
					float4 vertex : SV_POSITION;
	                float2 texcoord : TEXCOORD0;
	                fixed4 color : COLOR0;
					UNITY_FOG_COORDS(0)
					UNITY_VERTEX_OUTPUT_STEREO
				};

				fixed4 _Color;
				fixed4 _ShadowDir;
				float _ShadowIntensity;
				half4 _ACRect;
				fixed _ACIntensity;
				fixed _ACEdgeFeather;

				fixed map_ac(half value)
				{
					return smoothstep(0, _ACEdgeFeather, 1 - value);
				}

				v2f vert (appdata_t v)
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.texcoord = v.texcoord;

					fixed shadow = lerp(1 - _ShadowIntensity, 1, -clamp(dot(v.normal, normalize(_ShadowDir.xyz)), -1, 0));
					o.color = _Color * shadow;
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag (v2f i) : COLOR
				{
					// fixed2 acCoord = clamp(-abs(i.texcoord - _ACRect.xy) * 2 / _ACRect.zw + 1, 0, 1);
					// half ac = acCoord.x * acCoord.y;
					fixed2 acCoord = abs(i.texcoord - _ACRect.xy) * 2 / _ACRect.zw;
					fixed ac = max(acCoord.x, acCoord.y);

					fixed4 col = i.color * lerp(1, 1 - _ACIntensity, map_ac(ac));
					UNITY_APPLY_FOG(i.fogCoord, col);
					UNITY_OPAQUE_ALPHA(col.a);
					return col;
				}
			ENDCG
		}
	}
}
