// Raymarch based volumetric lighting, implementation based on:
//  - http://www.alexandre-pestana.com/volumetric-lights/ 
//  - https://fr.slideshare.net/BenjaminGlatzel/volumetric-lighting-for-many-lights-in-lords-of-the-fallen
//  - https://valeriomarty.medium.com/raymarched-volumetric-lighting-in-unity-urp-e7bc84d31604
Shader "Custom/Screen Space/Volumetric Lighting"
{
    Properties
    {
//        [Toggle(_SDF_MODE)] _SDFMode("SDF Mode", Float) = 0
    }
    
    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
    ENDHLSL
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off 
        ZTest Always
        Blend Off 
        Cull Off

        Pass
        {
            Name "Raymarch"
            ColorMask RGB
            
            HLSLPROGRAM

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex Vert
            #pragma fragment frag
            
            #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _FORWARD_PLUS
            // #pragma multi_compile_fragment _ _SDF_MODE
            
            #define PI_4 4.0h * PI
            
            half _Scattering;
            half _ScatteringPower;
            int _MaxSteps;
            half _MaxDistance;
            half _Jitter;
            half _GlobalIntensity;
            half3 _LightColor;

        #if _SDF_MODE
           // #include "Assets/Rendering/Shaders/HLSL/Library/SDF.hlsl"
        #endif
            
            // Mie scattering approximated with Henyey-Greenstein phase function.
            half ComputeScattering(half lightDotView)
            {
                half result = 1.0h - _Scattering * _Scattering;
                result /= PI_4 * pow(abs(1.0h + _Scattering * _Scattering + (2.0h * _Scattering) * lightDotView), _ScatteringPower);
                return saturate(result);
            }
            
            half random01(half2 p)
            {
                return frac(sin(dot(p, half2(41, 289))) * 45758.5453h);
            }

            half EaseInQuart(half x): number
            {
                x = saturate(x);
                return x * x * x * x;
            }
            
            half3 frag(Varyings input) : SV_Target
            {
#if UNITY_REVERSED_Z
                half depth = SampleSceneDepth(input.texcoord);
#else
                // Adjust z to match NDC for OpenGL
                half depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(uv));
#endif
                
                float3 worldPos = ComputeWorldSpacePosition(input.texcoord, depth, UNITY_MATRIX_I_VP);
                float3 startPosition = _WorldSpaceCameraPos;
                float3 rayVector = worldPos - startPosition;
                half3 rayDirection = normalize(rayVector);
                half rayLength = clamp(length(rayVector), 0, _MaxDistance);
                
                int steps = _MaxSteps;
                half minStepLength = rayLength / steps;
                half rayStartOffset = random01(input.texcoord) * minStepLength * _Jitter;
                half3 step = rayDirection * minStepLength;

#ifdef _SDF_MODE
                const half scattering = 0.025h;
#else
                half3 lightDir = _MainLightPosition.xyz; // _MainLightPosition is the forward direction
                half rayDot = dot(rayDirection, - lightDir); 
                half scattering = ComputeScattering(rayDot);
#endif

                int litCount = 0;
                half3 accumulatedLight = half3(0, 0, 0);
                float3 currentPosition = startPosition + rayStartOffset * rayDirection;
       
                for (int i = 0; i < steps - 1; i++)
                {
#ifdef _SDF_MODE
                    float sd;
                    half attenuation;
                    half3 color;
                    SDScene(currentPosition, MakeScene(), sd, attenuation, color);
                    bool lit = sd < 0;
                    // bool lit = sd < 0 && attenuation < 1;
                    // half lightIntensity = lit ? EaseInQuart(1 - attenuation) : 0;

                    // Sphere tracing: jump straight to the next SDF
                    // TODO: something's not right here... the distance isn't in the forward line, so it doesn't work
                    // step = rayDirection * max(minStepLength, sd);

                    // TODO: something's not working here, always gets max lit
                    // if (lit)
                    // {
                    //     int lightIndex = 0; // TODO: pass in with the SDF data
                    //     Light light = GetAdditionalPerObjectLight(lightIndex, currentPosition);
                    //     half lighting = AdditionalLightRealtimeShadow(lightIndex, currentPosition, light.direction);
                    //     lit = lighting == 1;
                    // }
                    half lightIntensity = lit ? 1 : 0;
#else
                    half lighting = MainLightRealtimeShadow(TransformWorldToShadowCoord(currentPosition));
                    half3 color = half3(1, 0, 0); // In non-SDF mode we only care about the red channel, color is applied in the composite pass
                    bool lit = lighting == 1;
                    half lightIntensity = 1;
#endif

                    accumulatedLight += scattering * lightIntensity * color;
                    litCount += lit;
                    currentPosition += step;

#ifdef _SDF_MODE
                    if (distance(currentPosition, startPosition) > rayLength)
                        break;
#endif
                }
                accumulatedLight /= steps;
                
                // filters out noise, particularly in caves, where only one sample hits and it causes artifacts
                int MIN_LIGHT_COUNT = max(2, _MaxSteps * 0.05);
                accumulatedLight = litCount >= MIN_LIGHT_COUNT ? accumulatedLight : 0;

#if _SDF_MODE
                // avoid blowing character/objects in the foreground when they're in a volumetric light
                half nearPlaneFactor = EaseInQuart(EaseInQuart(EaseInQuart(1 - depth)));
#else
                half nearPlaneFactor = 1;
#endif
                
                return accumulatedLight * _GlobalIntensity * nearPlaneFactor;
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "Volumetric Lighting Blur Horizontal"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragBlurH

            half3 FragBlurH(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float texelSize = _BlitTexture_TexelSize.x * 2.0;
                float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);

                // 9-tap gaussian blur on the downsampled source
                half3 c0 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv - float2(texelSize * 4.0, 0.0)).rgb;
                half3 c1 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv - float2(texelSize * 3.0, 0.0)).rgb;
                half3 c2 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv - float2(texelSize * 2.0, 0.0)).rgb;
                half3 c3 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv - float2(texelSize * 1.0, 0.0)).rgb;
                half3 c4 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv                               ).rgb;
                half3 c5 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(texelSize * 1.0, 0.0)).rgb;
                half3 c6 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(texelSize * 2.0, 0.0)).rgb;
                half3 c7 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(texelSize * 3.0, 0.0)).rgb;
                half3 c8 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(texelSize * 4.0, 0.0)).rgb;

                half3 color = c0 * 0.01621622 + c1 * 0.05405405 + c2 * 0.12162162 + c3 * 0.19459459
                            + c4 * 0.22702703
                            + c5 * 0.19459459 + c6 * 0.12162162 + c7 * 0.05405405 + c8 * 0.01621622;

                return color;
            }
            ENDHLSL
        }

        Pass
        {
            Name "Volumetric Lighting Blur Vertical"
            ColorMask RGB

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragBlurV

            half3 FragBlurV(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float texelSize = _BlitTexture_TexelSize.y;
                float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);

                // Optimized bilinear 5-tap gaussian on the same-sized source (9-tap equivalent)
                half3 c0 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv - float2(0.0, texelSize * 3.23076923)).rgb;
                half3 c1 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv - float2(0.0, texelSize * 1.38461538)).rgb;
                half3 c2 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv                                      ).rgb;
                half3 c3 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(0.0, texelSize * 1.38461538)).rgb;
                half3 c4 = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(0.0, texelSize * 3.23076923)).rgb;

                half3 color = c0 * 0.07027027 + c1 * 0.31621622
                            + c2 * 0.22702703
                            + c3 * 0.31621622 + c4 * 0.07027027;

                return color;
            }
            ENDHLSL
        }

        Pass
        {
            Name "Volumetric Lighting Upsample"
            ColorMask RGB
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragUpsample

            TEXTURE2D(_SourceTexLowMip);
            half _UpsampleBlend;
            
            half3 FragUpsample(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                half3 highMip = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord).rgb;
                half3 lowMip = SAMPLE_TEXTURE2D_X(_SourceTexLowMip, sampler_LinearClamp, input.texcoord).rgb;
                return lerp(highMip, lowMip, _UpsampleBlend);
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "Compositing"
            ColorMask RGB

            HLSLPROGRAM
            
            #pragma vertex Vert
            #pragma fragment frag
            
            TEXTURE2D(_RaymarchTexture);
            half _LightIntensity;
            half3 _LightColor;
            
            #pragma multi_compile_fragment _ _SDF_MODE

            half3 frag(Varyings input) : SV_Target
            {
                half3 raymarch = _RaymarchTexture.Sample(sampler_LinearClamp, input.texcoord).rgb;
                half3 color = FragNearest(input).rgb;
                
#if _SDF_MODE
                half3 result = raymarch;
#else
                half3 result = raymarch.r * _LightColor;
#endif
                
                half3 finalLight = result * _LightIntensity;
                return color + finalLight;
            }
            ENDHLSL
        }
    }
}