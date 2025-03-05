Shader "Custom/PaintShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BrushTex ("Brush Texture", 2D) = "white" {}
        _WatercolorTex ("Watercolor Texture", 2D) = "white" {}
        _ColorRamp ("Color Ramp", 2D) = "white" {}
        
        [Header(Performance Options)]
        [Toggle] _LowQualityMode ("Low Quality Mode", Float) = 0
        
        [Header(Painting Effect)]
        _BrushStrength ("Brush Strength", Range(0, 1)) = 0.7
        _WatercolorStrength ("Watercolor Strength", Range(0, 1)) = 0.5
        _ColorEnhance ("Color Enhancement", Range(0, 3)) = 1.5
        _ColorContrast ("Color Contrast", Range(0, 2)) = 1.2
        _ColorSaturation ("Color Saturation", Range(0, 2)) = 1.3
        
        [Header(Brush Texture Control)]
        [Toggle] _UseBrushColor ("Use Original Brush Texture Color", Float) = 0
        _BrushDarkColor ("Brush Dark Areas Color", Color) = (0.3, 0.25, 0.2, 1)
        _BrushLightColor ("Brush Light Areas Color", Color) = (0.9, 0.85, 0.8, 1)
        _BrushContrast ("Brush Contrast", Range(0, 2)) = 1.2
        
        [Header(Watercolor Texture Control)]
        [Toggle] _UseWatercolorColor ("Use Original Watercolor Texture Color", Float) = 0
        _WatercolorDarkColor ("Watercolor Dark Areas Color", Color) = (0.2, 0.25, 0.3, 1)
        _WatercolorLightColor ("Watercolor Light Areas Color", Color) = (0.8, 0.85, 0.9, 1)
        _WatercolorContrast ("Watercolor Contrast", Range(0, 2)) = 1.0
        
        [Header(Outline Effect)]
        _OutlineColor ("Outline Color", Color) = (0.1, 0.08, 0.07, 1)
        _OutlineIntensity ("Outline Intensity", Range(0, 1)) = 0.8
        
        [Header(Color Effects)]
        _ShadowColor ("Shadow Color", Color) = (0.2, 0.18, 0.25, 1)
        _MidtoneColor ("Midtone Tint", Color) = (1.02, 0.99, 0.86, 1)
        _HighlightColor ("Highlight Color", Color) = (1.05, 1.02, 0.9, 1)
        _ColorBleed ("Color Bleed", Range(0, 1)) = 0.3
        
        [Header(Texture Settings)]
        _TriplanarScale ("Texture Scale", Range(0.01, 5)) = 0.5
        _TriplanarBlendSharpness ("Blend Sharpness", Range(1, 10)) = 5
        
        [Header(Noise Settings)]
        _NoiseStrength ("Noise Strength", Range(0, 0.5)) = 0.15
        _NoiseScale ("Noise Scale", Range(0.1, 50)) = 15
        _NoiseDetail ("Noise Detail", Range(1, 5)) = 2
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _LOWQUALITYMODE_ON
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 color : COLOR;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
                float4 screenPos : TEXCOORD3;
                float3 worldPos : TEXCOORD4;
                float4 color : COLOR;
            };
            
            sampler2D _MainTex;
            sampler2D _BrushTex;
            sampler2D _WatercolorTex;
            sampler2D _ColorRamp;
            float4 _MainTex_ST;
            
            // Performance options
            float _LowQualityMode;
            
            // Painting effect parameters
            float _BrushStrength;
            float _WatercolorStrength;
            float _ColorEnhance;
            float _ColorContrast;
            float _ColorSaturation;
            
            // Brush texture control parameters
            float _UseBrushColor;
            float4 _BrushDarkColor;
            float4 _BrushLightColor;
            float _BrushContrast;
            
            // Watercolor texture control parameters
            float _UseWatercolorColor;
            float4 _WatercolorDarkColor;
            float4 _WatercolorLightColor;
            float _WatercolorContrast;
            
            // Outline parameters
            float4 _OutlineColor;
            float _OutlineIntensity;
            
            // Color effect parameters
            float4 _ShadowColor;
            float4 _MidtoneColor;
            float4 _HighlightColor;
            float _ColorBleed;
            
            // Texture parameters
            float _TriplanarScale;
            float _TriplanarBlendSharpness;
            
            // Noise parameters
            float _NoiseStrength;
            float _NoiseScale;
            float _NoiseDetail;
            
            // Vertex shader - transforms vertices and passes data to fragment shader
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = WorldSpaceViewDir(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.color = v.color;
                
                return o;
            }
            
            // Fast hash function for noise generation
            float hashNoise(float2 p)
            {
                p = frac(p * float2(123.4, 234.5));
                p += dot(p, p + 23.45);
                return frac(p.x * p.y);
            }
            
            // Fast noise function using a grid-based
            float fastNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                
                // Smoother interpolation curve
                f = f * f * (3.0 - 2.0 * f);
                
                // Sample the four corners
                float a = hashNoise(i);
                float b = hashNoise(i + float2(1.0, 0.0));
                float c = hashNoise(i + float2(0.0, 1.0));
                float d = hashNoise(i + float2(1.0, 1.0));
                
                // Bilinear interpolation
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }
            
            // Multi-layered noise function
            float layeredNoise(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;
                
                // Adjustable detail level based on quality setting
                int iterations = _LowQualityMode > 0.5 ? min(2, _NoiseDetail) : min(4, _NoiseDetail);
                
                for (int i = 0; i < iterations; i++)
                {
                    value += amplitude * fastNoise(p * frequency);
                    amplitude *= 0.5;
                    frequency *= 2.0;
                }
                
                return value;
            }
            
            // Combined triplanar mapping function for multiple textures
            // Sampling both textures in one function call with shared blend weights
            void combinedTriplanar(float3 worldPos, float3 worldNormal, 
                                  sampler2D brushTex, sampler2D watercolorTex, 
                                  float brushScale, float watercolorScale,
                                  out float4 brushResult, out float4 watercolorResult)
            {
                // Calculate blend weights once and reuse for both textures
                float3 blendWeights = pow(abs(worldNormal), _TriplanarBlendSharpness);
                blendWeights /= (blendWeights.x + blendWeights.y + blendWeights.z); // Normalize
                
                // X plane (YZ)
                float4 brushX = tex2D(brushTex, worldPos.yz * brushScale);
                float4 waterX = tex2D(watercolorTex, worldPos.yz * watercolorScale);
                
                // Y plane (XZ)
                float4 brushY = tex2D(brushTex, worldPos.xz * brushScale);
                float4 waterY = tex2D(watercolorTex, worldPos.xz * watercolorScale);
                
                // Z plane (XY)
                float4 brushZ = tex2D(brushTex, worldPos.xy * brushScale);
                float4 waterZ = tex2D(watercolorTex, worldPos.xy * watercolorScale);
                
                // Blend results using the same weights
                brushResult = brushX * blendWeights.x + brushY * blendWeights.y + brushZ * blendWeights.z;
                watercolorResult = waterX * blendWeights.x + waterY * blendWeights.y + waterZ * blendWeights.z;
            }
            
            // Processes texture samples to control their color contribution
            float3 processTextureColor(float4 textureSample, float useOriginalColor, float4 darkColor, float4 lightColor, float contrastValue)
            {
                // Early exit if using original color
                if (useOriginalColor > 0.5)
                {
                    return textureSample.rgb;
                }
                
                // Extract luminance 
                float luminance = dot(textureSample.rgb, float3(0.299, 0.587, 0.114));
                
                // Apply contrast adjustment
                luminance = saturate(((luminance - 0.5) * contrastValue) + 0.5);
                
                // Blend between dark and light colors based on luminance
                return lerp(darkColor.rgb, lightColor.rgb, luminance);
            }
            
            // Applies a painting style with shadow, midtone, and highlight zones
            float3 applyPaintingStyle(float3 color, float noise, float luminance)
            {
                // Apply contrast
                color = saturate((color - 0.5) * _ColorContrast + 0.5);
                
                // Create smooth transition weights between shadow, midtone, and highlight zones
                float shadowWeight = 1.0 - smoothstep(0.0, 0.3, luminance);
                float midtoneWeight = smoothstep(0.0, 0.3, luminance) * (1.0 - smoothstep(0.3, 0.7, luminance));
                float highlightWeight = smoothstep(0.3, 0.7, luminance);
                
                // Blend all three color zones using weights
                float3 processedColor = (_ShadowColor.rgb * shadowWeight +
                                       _MidtoneColor.rgb * midtoneWeight +
                                       _HighlightColor.rgb * highlightWeight) * color;
                
                // Add color bleeding effect
                float3 complementColor = (1.0 - color) * 0.5;
                float bleedFactor = _ColorBleed * noise * (1.0 - luminance);
                processedColor = lerp(processedColor, complementColor, bleedFactor);
                
                // Adjust saturation
                float3 grayscale = float3(luminance, luminance, luminance);
                return lerp(grayscale, processedColor, _ColorSaturation);
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the base texture
                fixed4 baseColor = tex2D(_MainTex, i.uv);
                
                // Generate noise once and reuse throughout the shader
                float noise = layeredNoise(i.worldPos.xy * _NoiseScale);
                
                // Sample brush and watercolor textures with shared triplanar mapping
                float4 brushTexSample, watercolorTexSample;
                combinedTriplanar(i.worldPos, i.worldNormal, 
                                _BrushTex, _WatercolorTex, 
                                _TriplanarScale, _TriplanarScale * 0.7,
                                brushTexSample, watercolorTexSample);
                
                // Process texture colors
                float3 brushColor = processTextureColor(brushTexSample, _UseBrushColor, 
                                                    _BrushDarkColor, _BrushLightColor, _BrushContrast);
                                                    
                float3 watercolorColor = processTextureColor(watercolorTexSample, _UseWatercolorColor, 
                                                        _WatercolorDarkColor, _WatercolorLightColor, _WatercolorContrast);
                
                // Apply painted look by blending base color with processed textures
                float3 paintedColor = baseColor.rgb;
                paintedColor = lerp(paintedColor, paintedColor * brushColor, _BrushStrength);
                paintedColor = lerp(paintedColor, paintedColor * watercolorColor, _WatercolorStrength * noise);
                
                // Calculate luminance once and reuse
                float luminance = dot(paintedColor, float3(0.299, 0.587, 0.114));
                
                // Apply painting color styling
                paintedColor = applyPaintingStyle(paintedColor, noise, luminance);
                
                // Color ramp enhancement
                float4 enhancedColor = tex2D(_ColorRamp, float2(luminance, 0.5));
                paintedColor = lerp(paintedColor, paintedColor * enhancedColor.rgb, _ColorEnhance * 0.5);
                
                // Add noise variation for brush stroke effect
                paintedColor += (noise - 0.5) * _NoiseStrength;
                
                // Silhouette edge detection
                float ndotv = saturate(dot(normalize(i.worldNormal), normalize(i.viewDir)));
                float edge = smoothstep(0.1, 0.6, pow(1.0 - ndotv, 3)) * _OutlineIntensity;
                
                // Apply outline
                paintedColor = lerp(paintedColor, _OutlineColor.rgb, edge);
                
                return fixed4(paintedColor, 1.0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
