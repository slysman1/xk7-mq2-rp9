Shader "AlexDev/FadeFromCenter"
{
    Properties
    {
        [MainTexture] _MainTex ("Crack Texture", 2D) = "white" {}
        [MainColor] _Color ("Tint Color", Color) = (1,1,1,1)
        _RevealRadius ("Reveal Radius (0 = hidden, 10 = full)", Range(0,10)) = 10
        [Toggle(_INVERT_ALPHA)] _InvertAlpha ("Invert Alpha", Float) = 0

        // Emission
        [HDR] _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        _EmissionStrength ("Emission Strength", Range(0,10)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "RenderPipeline"="UniversalRenderPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            float4 _Color;
            float _RevealRadius;
            float _InvertAlpha;

            float4 _EmissionColor;
            float _EmissionStrength;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                // Auto alpha based on brightness (white crack = visible)
                float brightness = max(max(tex.r, tex.g), tex.b);
                tex.a = brightness;
                if (_InvertAlpha > 0.5)
                    tex.a = 1 - tex.a;

                // --- Radial reveal from center ---
                float2 centeredUV = IN.uv - 0.5;
                float dist = length(centeredUV) * 10.0; // scale to match radius range

                // Reveal only if inside radius (hard edge)
                clip(_RevealRadius - dist);

                // Apply color and emission
                tex.rgb *= _Color.rgb;
                tex.a *= _Color.a;

                float emissiveMask = tex.a;
                float3 emissive = _EmissionColor.rgb * _EmissionStrength * emissiveMask;
                tex.rgb += emissive;

                return tex;
            }
            ENDHLSL
        }
    }

    FallBack Off
}