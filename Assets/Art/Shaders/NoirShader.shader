Shader "Custom/NoirShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        
        // Lighting Control
        _ShadowColor ("Shadow Color", Color) = (0.1, 0.1, 0.2, 1)
        _LightColor ("Light Color", Color) = (1.0, 0.95, 0.8, 1)
        _ShadowThreshold ("Shadow Threshold", Range(0, 1)) = 0.3
        _ShadowSharpness ("Shadow Sharpness", Range(0.001, 0.5)) = 0.01
        
        // Comic Book Style Controls
        _ContrastAmount ("Contrast", Range(0, 2)) = 1.2
        _SaturationAmount ("Saturation", Range(0, 2)) = 0.8
        
        // Outline
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0.0001, 0.01)) = 0.003
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _BaseColor;
            float4 _ShadowColor;
            float4 _LightColor;
            float _ShadowThreshold;
            float _ShadowSharpness;
            float _ContrastAmount;
            float _SaturationAmount;
            float4 _OutlineColor;
            float _OutlineWidth;
        CBUFFER_END

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        // Common vertex attributes structure for main passes
        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float2 uv : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varyings
        {
            float2 uv : TEXCOORD0;
            float4 positionCS : SV_POSITION;
            float3 normalWS : TEXCOORD1;
            float3 positionWS : TEXCOORD2;
            UNITY_VERTEX_OUTPUT_STEREO
        };
        ENDHLSL
        
        // Main Pass - Stylized Lighting
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }
            
            // Apply color adjustments
            float3 ApplyTelltaleStyle(float3 color)
            {
                // Convert to grayscale
                float luminance = dot(color, float3(0.299, 0.587, 0.114));
                
                // Apply saturation control
                color = lerp(luminance.xxx, color, _SaturationAmount);
                
                // Apply contrast enhancement
                color = ((color - 0.5) * _ContrastAmount) + 0.5;
                
                return saturate(color);
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Sample the texture with better error handling
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                // Debug: If texture is missing or black, use base color instead
                if (all(texColor.rgb < 0.01))
                {
                    texColor = half4(1,1,1,1); // Fallback to white if texture is missing
                }
                
                // Apply base color tint
                texColor *= _BaseColor;
                
                // Get main light
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                
                // Calculate lighting
                float3 normalWS = normalize(input.normalWS);
                float NdotL = dot(normalWS, mainLight.direction);
                
                // Calculate light intensity with shadows
                float lightIntensity = max(0, NdotL) * mainLight.shadowAttenuation;
                
                // Inverted the shadow factor calculation to fix lighting       
                // Use the lightIntensity directly to interpolate between shadow and light
                float shadowFactor = smoothstep(_ShadowThreshold - _ShadowSharpness, 
                                               _ShadowThreshold + _ShadowSharpness, 
                                               lightIntensity);
                
                // Swap the order in the lerp function to fix lighting
                float3 shadedColor = lerp(_LightColor.rgb, _ShadowColor.rgb, 1.0 - shadowFactor);
                float3 finalColor = texColor.rgb * shadedColor;
                
                // Apply the Telltale style post-processing
                finalColor = ApplyTelltaleStyle(finalColor);
                
                // Modified blue tint in shadows to make it more visible in lit areas
                finalColor = lerp(finalColor, finalColor * float3(0.9, 0.95, 1.1), shadowFactor);
                
                return half4(finalColor, texColor.a);
            }
            ENDHLSL
        }
        
        // Outline Pass
        Pass
        {
            Name "Outline"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Front
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                // Calculate scale-independent outline width
                float3 positionOS = input.positionOS.xyz;
                float3 normalOS = normalize(input.normalOS);
                
                // Get object scale to adjust outline width
                float3 objectScale = float3(
                    length(unity_ObjectToWorld._m00_m10_m20),
                    length(unity_ObjectToWorld._m01_m11_m21),
                    length(unity_ObjectToWorld._m02_m12_m22)
                );
                float scaleAvg = (objectScale.x + objectScale.y + objectScale.z) / 3.0;
                
                // Apply the outline offset in object space
                float adjustedWidth = _OutlineWidth / scaleAvg;
                positionOS += normalOS * adjustedWidth;
                
                // Transform to clip space
                VertexPositionInputs vertexInput = GetVertexPositionInputs(positionOS);
                output.positionCS = vertexInput.positionCS;
                
                // We don't use these in the fragment shader for outline, but need to set them
                output.normalWS = float3(0, 0, 0);
                output.positionWS = float3(0, 0, 0);
                output.uv = float2(0, 0);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
        
        // Shadow casting pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            // We include our common Attributes structure via HLSLINCLUDE
            
            struct ShadowVaryings
            {
                float4 positionCS   : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            
            float3 _LightDirection;
            
            ShadowVaryings ShadowPassVertex(Attributes input)
            {
                ShadowVaryings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                // Apply shadow bias and light direction
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
                
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif
                
                output.positionCS = positionCS;
                return output;
            }
            
            half4 ShadowPassFragment(ShadowVaryings input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
        
        // Depth pass
        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}
            
            ZWrite On
            ColorMask 0
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            
            struct DepthVaryings
            {
                float2 uv           : TEXCOORD0;
                float4 positionCS   : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            DepthVaryings DepthOnlyVertex(Attributes input)
            {
                DepthVaryings output = (DepthVaryings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }
            
            half4 DepthOnlyFragment(DepthVaryings input) : SV_TARGET
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}