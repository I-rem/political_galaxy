Shader "Skybox/PanoramicBlend"
{
    Properties
    {
        _Tint ("Tint Color", Color) = (.5, .5, .5, .5)
        [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
        _Rotation ("Rotation", Range(0, 360)) = 0
        [NoScaleOffset] _MainTex ("Texture 1 (BG1)", 2D) = "grey" {}
        [NoScaleOffset] _Tex2 ("Texture 2 (BG2)", 2D) = "grey" {}
        _Blend ("Blend", Range(0.0, 1.0)) = 0.5
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _Tex2;
            half4 _Tint;
            half _Exposure;
            float _Rotation;
            float _Blend;

            struct appdata_t {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float3 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float3 RotateAroundYInDegrees (float3 vertex, float degrees)
            {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float3(mul(m, vertex.xz), vertex.y).xzy;
            }

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float3 rotated = RotateAroundYInDegrees(v.vertex.xyz, _Rotation);
                o.vertex = UnityObjectToClipPos(rotated);
                o.texcoord = v.vertex.xyz;
                return o;
            }

            float2 ToRadialCoords(float3 coords)
            {
                float3 normalizedCoords = normalize(coords);
                float latitude = acos(normalizedCoords.y);
                float longitude = atan2(normalizedCoords.z, normalizedCoords.x);
                float2 sphereCoords = float2(longitude, latitude) * float2(0.1591549, 0.3183099);
                return float2(0.5,1.0) - sphereCoords;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 tc = ToRadialCoords(i.texcoord);
                half4 tex1 = tex2D(_MainTex, tc);
                half4 tex2 = tex2D(_Tex2, tc);
                
                // İki dokuyu Blend değeri ile karıştır
                half4 c = lerp(tex1, tex2, _Blend);
                c = c * _Tint * _Exposure;
                return c;
            }
            ENDCG
        }
    }
    Fallback Off
}
