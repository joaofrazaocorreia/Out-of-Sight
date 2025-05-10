Shader "Custom/FixedTexturedOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        
        [Space(10)]
        [Header(Outline Settings)]
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0,0.1)) = 0.005
        [Toggle] _ScaleWithDistance ("Scale With Distance", Float) = 0
        _OutlineMinDistance ("Min Distance", Float) = 1
        _OutlineMaxDistance ("Max Distance", Float) = 10
    }
    
    // Universal fallback shader in case something breaks
    FallBack "Diffuse"
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        // First pass - Standard shader for main geometry
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        
        sampler2D _MainTex;
        fixed4 _Color;
        half _Glossiness;
        half _Metallic;
        
        struct Input
        {
            float2 uv_MainTex;
        };
        
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Sample the texture and apply tint
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            
            // Apply to output
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
        
        // Second pass - Outline
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "Always" }
            
            Cull Front
            ZWrite On
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord : TEXCOORD0;
            };
            
            struct v2f {
                float4 pos : SV_POSITION;
            };
            
            float _OutlineWidth;
            float4 _OutlineColor;
            float _ScaleWithDistance;
            float _OutlineMinDistance;
            float _OutlineMaxDistance;
            
            v2f vert(appdata v) {
                v2f o;
                
                // Convert to world position for distance check
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float distanceToCamera = distance(_WorldSpaceCameraPos, worldPos);
                
                // Apply distance scaling if enabled
                float scaleFactor = 1.0;
                if (_ScaleWithDistance > 0.5) {
                    scaleFactor = smoothstep(_OutlineMinDistance, _OutlineMaxDistance, distanceToCamera);
                }
                
                // Calculate outline width with distance adjustment
                float width = _OutlineWidth * (1 + scaleFactor);
                
                // Extrude vertices along normals
                float3 normal = normalize(v.normal);
                v.vertex.xyz += normal * width;
                
                // Transform to clip space
                o.pos = UnityObjectToClipPos(v.vertex);
                
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target {
                return _OutlineColor;
            }
            ENDCG
        }
    }
}
