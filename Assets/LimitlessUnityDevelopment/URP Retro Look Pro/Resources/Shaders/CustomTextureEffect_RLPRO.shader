Shader "Hidden/Shader/CustomTextureEffect_RLPRO"
{
    Properties
    {
        _BlitTexture("Texture", 2D) = "white" {}
        _CustomTex("CTexture", 2D) = "white" {}
    }
    HLSLINCLUDE
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        // The Blit.hlsl file provides the vertex shader (Vert),
        // the input structure (Attributes), and the output structure (Varyings)
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        TEXTURE2D(_CustomTex);
        SAMPLER(sampler_CustomTex);
	    half fade;
        half alpha;

    float4 CustomPostProcess(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        float2 positionSS = input.texcoord;
        float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, positionSS);
        float4 col2 = SAMPLE_TEXTURE2D(_CustomTex, sampler_CustomTex, positionSS);
        return lerp(col, col2, col2.a * fade);
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "#CustomTexture#"

			Cull Off ZWrite Off ZTest Always

            HLSLPROGRAM
                #pragma fragment CustomPostProcess
                #pragma vertex Vert
            ENDHLSL
        }
    }
    Fallback Off
}