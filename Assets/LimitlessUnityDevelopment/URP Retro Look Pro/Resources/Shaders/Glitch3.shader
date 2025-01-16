Shader "RetroLookPro/Glitch3"
{
    Properties
    {
        _BlitTexture("Texture", 2D) = "white" {}
    }

        HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // The Blit.hlsl file provides the vertex shader (Vert),
            // the input structure (Attributes), and the output structure (Varyings)
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
    TEXTURE2D(_Mask);
    SAMPLER(sampler_Mask);
    #pragma shader_feature ALPHA_CHANNEL

    float _FadeMultiplier;
    float speed;
    float fade;
    float blockSize;
    float maxOffsetX;
    float maxOffsetY;

    inline float rand(float2 seed)
    {
        return frac(sin(dot(seed * floor(_Time.y * speed), float2(127.1, 311.7))) * 43758.5453123);
    }

    inline float rand(float seed)
    {
        return rand(float2(seed, 1.0));
    }

    float4 Frag(Varyings i) : SV_Target
    {
        if (_FadeMultiplier > 0)
        {
            #if ALPHA_CHANNEL
                        float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, i.texcoord).a);
            #else
                        float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, i.texcoord).r);
            #endif
            maxOffsetX *= alpha_Mask;
            maxOffsetY *= alpha_Mask;
            blockSize *= alpha_Mask;
        }
        float2 block = rand(floor(i.texcoord * blockSize));
        float OffsetX = pow(block.x, 8.0) * pow(block.x, 3.0) - pow(rand(7.2341), 17.0) * maxOffsetX;
        float OffsetY = pow(block.x, 8.0) * pow(block.x, 3.0) - pow(rand(7.2341), 17.0) * maxOffsetY;
        float4 r = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.texcoord);
        float4 g = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.texcoord + half2(OffsetX * 0.05 * rand(7.0), OffsetY * 0.05 * rand(12.0)));
        float4 b = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.texcoord - half2(OffsetX * 0.05 * rand(13.0), OffsetY * 0.05 * rand(12.0)));
        return lerp(r,half4(r.x, g.g, b.z, (r.a + g.a + b.a)),fade);
    }

        ENDHLSL

        SubShader
    {
        Cull Off ZWrite Off ZTest Always

            Pass
        {
            HLSLPROGRAM

                #pragma vertex Vert
                #pragma fragment Frag

            ENDHLSL
        }
    }
}