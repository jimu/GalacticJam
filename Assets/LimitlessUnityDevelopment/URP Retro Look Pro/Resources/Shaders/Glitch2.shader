Shader "Hidden/Shader/Glitch2" 
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
        sampler2D _NoiseTex;
        sampler2D _TrashTex;
        float _ColorIntensity;

        float4 Frag(Varyings i) : SV_Target
        {
           half fade = 1;
               if (_FadeMultiplier > 0)
    {
        #if ALPHA_CHANNEL
                float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, i.texcoord).a);
        #else
                float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, i.texcoord).r);
        #endif
        fade *= alpha_Mask;
    }
        float4 glitch = tex2D(_NoiseTex, i.texcoord);
        float thresh = 1.001 - 0 * 1.001;
        float w_d = step(thresh*_ColorIntensity, pow(abs(glitch.z), 2.5)); 
        float w_f = step(thresh, pow(abs(glitch.w), 2.5)); 
        float w_c = step(thresh, pow(abs(glitch.z), 3.5)); 
        float2 uv = frac(i.texcoord + glitch.xy * w_d);
        float4 source = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv);
        float4 source2 = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.texcoord);
        float3 color = lerp(source, tex2D(_TrashTex, uv), w_f).rgb;
        float3 neg = saturate(color.grb + (1 - dot(color, 1)) * 0.1);
        color = lerp(color, neg, w_c);

        return lerp(source2,float4(color, source.a),fade);
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