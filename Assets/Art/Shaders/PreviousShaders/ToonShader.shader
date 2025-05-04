Shader "Custom/ToonShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
        [Header(Color Settings)]
        _ColorSteps ("Color Bands", Range(1, 10)) = 3
        _ColorSaturation ("Color Saturation", Range(0.0, 2.0)) = 1.1
        
        [Header(Lighting)]
        _LightBands ("Light Bands", Range(1, 5)) = 2
        _ShadowColor ("Shadow Color", Color) = (0.5, 0.5, 0.8, 1.0)
        _HighlightColor ("Highlight Color", Color) = (1.0, 0.9, 0.8, 1.0)
        
        [Header(Outline)]
        [Toggle] _UseOutline ("Use Outline", Float) = 1
        _OutlineColor ("Outline Color", Color) = (0.0, 0.0, 0.0, 1.0)
        _OutlineWidth ("Outline Width", Range(0.0, 0.1)) = 0.01
        _OutlineDetection ("Edge Detection Sensitivity", Range(0.0, 1.0)) = 0.5
        
        [Header(Special Effects)]
        [Toggle] _UseHalftone ("Use Halftone", Float) = 0
        _HalftonePattern ("Halftone Pattern", 2D) = "white" {}
        _HalftoneSize ("Halftone Size", Range(10, 100)) = 40
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        
        Pass
        {
            Name "2D Feel Main Pass"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : NORMAL;
                float3 positionWS : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            TEXTURE2D(_MainTex);
            TEXTURE2D(_HalftonePattern);
            SAMPLER(sampler_MainTex);
            SAMPLER(sampler_HalftonePattern);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _ColorSteps;
                float _ColorSaturation;
                float _LightBands;
                float4 _ShadowColor;
                float4 _HighlightColor;
                float _UseOutline;
                float4 _OutlineColor;
                float _OutlineWidth;
                float _OutlineDetection;
                float _UseHalftone;
                float4 _HalftonePattern_ST;
                float _HalftoneSize;
            CBUFFER_END
            
            // Quantize a value into steps
            float quantize(float value, float steps)
            {
                return floor(value * steps) / steps;
            }
            
            // Edge detection for outlines
            float detectEdge(float2 uv, float width)
            {
                float2 texelSize = 1.0 / _ScreenParams.xy;
                
                float4 center = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                float4 up = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0, texelSize.y * width));
                float4 down = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - float2(0, texelSize.y * width));
                float4 left = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - float2(texelSize.x * width, 0));
                float4 right = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(texelSize.x * width, 0));
                
                float diff = length(abs(up - center) + abs(down - center) + abs(left - center) + abs(right - center));
                
                return smoothstep(0.01, 0.01 + _OutlineDetection, diff);
            }
            
            // Halftone pattern
            float halftone(float2 uv, float value)
            {
                float2 patternUV = uv * _ScreenParams.xy / _HalftoneSize;
                float pattern = SAMPLE_TEXTURE2D(_HalftonePattern, sampler_HalftonePattern, patternUV).r;
                
                return step(pattern, value);
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                
                return output;
            }
            
            float4 frag(Varyings input) : SV_Target
            {
                // Sample the texture
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                // Get main light
                Light mainLight = GetMainLight();
                
                // Calculate simple diffuse lighting
                float ndotl = dot(normalize(input.normalWS), normalize(mainLight.direction));
                ndotl = ndotl * 0.5 + 0.5; // Remap to 0-1
                
                // Quantize lighting into bands
                float lightBand = quantize(ndotl, _LightBands);
                
                // Tint shadows and highlights
                float3 lightColor = lerp(_ShadowColor.rgb, _HighlightColor.rgb, lightBand);
                
                // Apply color banding to original texture
                float3 bandedColor;
                bandedColor.r = quantize(texColor.r, _ColorSteps);
                bandedColor.g = quantize(texColor.g, _ColorSteps);
                bandedColor.b = quantize(texColor.b, _ColorSteps);
                
                // Apply saturation
                float luminance = dot(bandedColor, float3(0.299, 0.587, 0.114));
                float3 saturatedColor = lerp(float3(luminance, luminance, luminance), bandedColor, _ColorSaturation);
                
                // Combine lighting with color
                float3 finalColor = saturatedColor * lightColor;
                
                // Apply halftone effect if enabled
                if (_UseHalftone > 0.5)
                {
                    float halftoneValue = halftone(input.uv, lightBand);
                    finalColor = lerp(_ShadowColor.rgb * saturatedColor, 
                                     _HighlightColor.rgb * saturatedColor, 
                                     halftoneValue);
                }
                
                // Apply outline if enabled
                if (_UseOutline > 0.5)
                {
                    float edge = detectEdge(input.uv, _OutlineWidth * 10.0);
                    finalColor = lerp(finalColor, _OutlineColor.rgb, edge);
                }
                
                return float4(finalColor, texColor.a);
            }
            ENDHLSL
        }
        
        // Shadow casting pass
        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}
            
            ZWrite On
            ZTest LEqual
            ColorMask 0
            
            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            // Include core URP headers
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            // Manually implement the shadow caster pass since the include file is missing
            
            float3 _LightDirection;
            
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 texcoord     : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float2 uv           : TEXCOORD0;
                float4 positionCS   : SV_POSITION;
            };
            
            // Properties required by shadow pass
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                // Other properties from main pass
            CBUFFER_END
            
            // Shadow vertex shader
            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
            
                output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);
                
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                // Calculate shadow position
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
                
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif
                
                output.positionCS = positionCS;
                return output;
            }
            
            // Shadow fragment shader
            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
    }
    
    Fallback "Universal Render Pipeline/Lit"
}