// refered to:
//     https://github.com/keijiro/KinoGlitch.git
//     Assets/Kino/Glitch/Shader/AnalogGlitch.shader
Shader "URPGlitch/Analog"
{
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
        }

        Pass
        {
            Name "AnalogGlitchPass"

            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float2 _ScanLineJitter;
            float2 _VerticalJump;
            float _HorizontalShake;
            float2 _ColorDrift;

            float nrand(float x, float y)
            {
                return frac(sin(dot(float2(x, y), float2(12.9898, 78.233))) * 43758.5453);
            }

            half4 Fragment(Varyings i) : SV_Target
            {
                float u = i.texcoord.x;
                float v = i.texcoord.y;

                // Scan line jitter
                float jitter = nrand(v, _Time.x) * 2 - 1;
                jitter *= step(_ScanLineJitter.y, abs(jitter)) * _ScanLineJitter.x;

                // Vertical jump
                float jump = lerp(v, frac(v + _VerticalJump.y), _VerticalJump.x);

                // Horizontal shake
                float shake = (nrand(_Time.x, 2) - 0.5) * _HorizontalShake;

                // Color drift
                float drift = sin(jump + _ColorDrift.y) * _ColorDrift.x;

                half4 src1 = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearRepeat, frac(float2(u + jitter + shake, jump)));
                half4 src2 = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearRepeat, frac(float2(u + jitter + shake + drift, jump)));
                return half4(src1.r, src2.g, src1.b, 1);
            }
            ENDHLSL
        }
    }
}