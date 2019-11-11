Shader "Unlit/Equirectangular" 
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull front // Flip the surfaces. From sample shader from Bernie Roehl: http://bernieroehl.com/360stereoinunity/
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma glsl
            #pragma target 3.0

            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
               float4 vertex : POSITION;
               float3 normal : NORMAL;
            };

            struct v2f
            {
                float3    normal : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4    pos : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            #define PI 3.141592653589793

            inline float2 RadialCoords(float3 a_coords)
            {
                float3 a_coords_n = normalize(a_coords);
                float lon = atan2(a_coords_n.z, a_coords_n.x);
                float lat = acos(a_coords_n.y);
                float2 sphereCoords = float2(lon, lat) * (1.0 / PI);
                return float2(1 - (sphereCoords.x * 0.5 + 0.5), sphereCoords.y);
            }

            float4 frag(v2f IN) : COLOR
            {
                float2 equiUV = RadialCoords(IN.normal) * _MainTex_ST.xy + _MainTex_ST.zw;
                return tex2D(_MainTex, equiUV);
            }

            v2f vert (appdata v)
            {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.normal = v.normal;
                    return o;

            }
            ENDCG
        }
    }
}