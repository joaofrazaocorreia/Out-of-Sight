Shader "Custom/NoirShader"
{
    Properties
    {
        [Toggle(_USE_TEXTURE)] _UseTexture ("Use Texture", Float) = 1
        [MainTexture] _MainTex ("Texture", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Color", Color) = (1,1,1,1)
        [MainColor] _ColorTint ("Color Tint", Color) = (1,1,1,1)
        _ColorBrightness ("Color Brightness", Range(0, 2)) = 1.0
        _ColorContrast ("Color Contrast", Range(0, 2)) = 1.0
        _ColorSaturation ("Color Saturation", Range(0, 2)) = 1.0
        
        // Cel-shading properties
        _CelShadingLevels ("Cel Shading Levels", Range(1, 8)) = 3
        _CelSpecularLevels ("Cel Specular Levels", Range(1, 8)) = 2
        _SpecularPower ("Specular Power", Range(1, 100)) = 30
        
        // Outline properties
        [HDR] _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0.0001, 0.01)) = 0.003
        
        // Hard shadow properties
        [HDR] _ShadowColor ("Shadow Color", Color) = (0.1,0.1,0.2,1)
        _ShadowThreshold ("Shadow Threshold", Range(0, 1)) = 0.5
    }
    
    SubShader
    {
        // Main pass for the model
        Pass
        {
            Tags 
            { 
                "LightMode" = "UniversalForward" 
                "RenderType"="Opaque"
                "Queue"="Geometry"
            }
            
            CGPROGRAM
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma shader_feature_local _USE_TEXTURE
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _BaseColor;
            float4 _ColorTint;
            float _ColorBrightness;
            float _ColorContrast;
            float _ColorSaturation;
            int _CelShadingLevels;
            int _CelSpecularLevels;
            float _SpecularPower;
            float4 _ShadowColor;
            float _ShadowThreshold;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            
            float3 ApplyColorAdjustments(float3 col)
            {
                // Apply brightness
                col *= _ColorBrightness;
                
                // Apply contrast
                float3 contrast = (col - 0.5) * _ColorContrast + 0.5;
                col = contrast;
                
                // Apply saturation
                float luminance = dot(col, float3(0.299, 0.587, 0.114));
                col = lerp(luminance, col, _ColorSaturation);
                
                return col;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Sample texture or use base color
                fixed4 col;
                #ifdef _USE_TEXTURE
                    col = tex2D(_MainTex, i.uv) * _ColorTint;
                #else
                    col = _BaseColor * _ColorTint;
                #endif
                
                // Calculate lighting
                float3 worldNormal = normalize(i.worldNormal);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float NdotL = dot(worldNormal, lightDir);
                
                // Apply cel-shading to diffuse lighting
                float celDiffuse = ceil(NdotL * _CelShadingLevels) / _CelShadingLevels;
                
                // Apply hard shadows
                float shadow = step(_ShadowThreshold, NdotL);
                float3 diffuseLight = lerp(_ShadowColor.rgb, _LightColor0.rgb, shadow) * celDiffuse;
                
                // Calculate specular lighting
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 halfVector = normalize(lightDir + viewDir);
                float NdotH = max(0, dot(worldNormal, halfVector));
                float specular = pow(NdotH, _SpecularPower);
                
                // Apply cel-shading to specular
                float celSpecular = ceil(specular * _CelSpecularLevels) / _CelSpecularLevels;
                celSpecular *= step(0.1, celDiffuse);
                
                // Combine lighting with texture color
                float3 finalColor = col.rgb * diffuseLight + celSpecular * _LightColor0.rgb;
                
                // Apply color style adjustments
                finalColor = ApplyColorAdjustments(finalColor);
                
                return fixed4(finalColor, col.a);
            }
            ENDCG
        }
        
        // Outline pass
        Pass
        {
            Name "Outline"
            Tags
            {
                "LightMode" = "SRPDefaultUnlit"
                "RenderType"="Opaque"
                "Queue"="Geometry+1"
            }
            Cull Front
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
            };
            
            float4 _OutlineColor;
            float _OutlineWidth;
            
            v2f vert(appdata v)
            {
                v2f o;
                // Expand vertex along normal direction
                float3 normal = normalize(v.normal);
                v.vertex.xyz += normal * _OutlineWidth;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDCG
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
