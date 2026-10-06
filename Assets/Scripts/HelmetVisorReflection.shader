Shader "UI/HelmetVisorReflection"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _RefTex1 ("Reflection BG1 (Skybox 1)", 2D) = "black" {}
        _RefTex2 ("Reflection BG2 (Skybox 2)", 2D) = "black" {}
        
        _ReflectionStrength ("Reflection Strength", Range(0, 1)) = 0.25
        _Curvature ("Glass Curvature", Range(0, 5)) = 1.2
        _GlassTint ("Glass Tint Color", Color) = (0.8, 0.9, 1.0, 1.0)
        
        // Stencil properties for UI
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            sampler2D _RefTex1;
            sampler2D _RefTex2;
            float _ReflectionStrength;
            float _Curvature;
            float _GlobalSkyboxBlend;
            fixed4 _GlassTint;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(o.worldPosition);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            float2 ToRadialCoords(float3 coords)
            {
                float3 normalizedCoords = normalize(coords);
                float latitude = acos(normalizedCoords.y);
                float longitude = atan2(normalizedCoords.z, normalizedCoords.x);
                float2 sphereCoords = float2(longitude, latitude) * float2(0.1591549, 0.3183099);
                return float2(0.5, 1.0) - sphereCoords;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // UI Sprite Texture (Kaskın sınırları, kir, vs)
                half4 color = tex2D(_MainTex, i.texcoord) * i.color;

                // Ekran koordinatları (0 to 1) -> Merkez (-1 to 1)
                float2 screenUV = i.vertex.xy / _ScreenParams.xy;
                float2 centerUV = screenUV * 2.0 - 1.0;
                float dist = length(centerUV);
                
                // Cam bombesini simüle etmek için bakış vektörünü büküyoruz
                // Kamera ileriye bakıyor (Z=1), kenarlara doğru bükülme artar
                float3 viewDirViewSpace = normalize(float3(centerUV * _Curvature, 1.0 - (dist * 0.4 * _Curvature)));
                
                // View space'ten World space'e çeviriyoruz
                // Böylece oyuncu kafasını çevirdiğinde yansıma da doğru yönü gösterir
                float3 viewDirWorld = mul((float3x3)UNITY_MATRIX_I_V, viewDirViewSpace);
                
                // Panoramik skybox dokularından o yönü örnekle
                float2 tc = ToRadialCoords(viewDirWorld);
                half4 ref1 = tex2D(_RefTex1, tc);
                half4 ref2 = tex2D(_RefTex2, tc);
                
                // İki arkaplan arasındaki global geçişi (SkyboxBlender.cs'den gelen) al
                half4 reflection = lerp(ref1, ref2, _GlobalSkyboxBlend) * _GlassTint;
                
                // Cam maskesi: Kaskın ortasında tam yansıma, kenarlara doğru yansıma artar (veya azalır)
                // dist 0 (merkez), dist 1 (kenar). Kenarlarda bombeli yansıma daha yoğun olur
                float glassMask = smoothstep(0.0, 1.5, dist + 0.5);
                
                // Yansıma rengi (Ne kadar güçlü olacağı maskeye bağlı)
                half3 refColor = reflection.rgb * _ReflectionStrength * glassMask;
                
                // SORUNUN ÇÖZÜMÜ: 
                // Unity şeffaf piksellerin RGB kanallarına kenardaki renkleri yayar (Alpha Bleeding / Dilation).
                // Eğer direkt color.a değerini yükseltirsek, bu taşan bozuk renkleri (görseldeki mavi uzantılar) görürüz.
                // Bu yüzden şeffaf yerlerde orijinal UI rengini tamamen YOK SAYIP sadece yansımayı göstermeliyiz.
                
                half finalAlpha = color.a + (1.0 - color.a) * (glassMask * _ReflectionStrength);
                
                // Orijinal UI ile Yansımayı Alpha üzerinden harmanla
                // Eğer color.a = 1 ise (örneğin butonlar), tamamen orijinal rengi (color.rgb) kullan.
                // Eğer color.a = 0 ise (boş cam kısımları), tamamen yansımayı (refColor) kullan.
                half3 finalRGB = lerp(refColor, color.rgb, color.a);

                return half4(finalRGB, finalAlpha);
            }
            ENDCG
        }
    }
}
