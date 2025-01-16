Shader "Hidden/Shader/NegativeEffect_RLPRO"
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
	uniform float T;
	uniform float Luminosity;
	uniform float Vignette;
	uniform float Negative;
	uniform float Contrast;
	uniform float fade;

	float4 linearLight(float4 s, float4 d)
	{
		return 2.0 * s + d - 1.0 * Luminosity;
	}

	float4 Frag(Varyings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

	float2 uv = i.texcoord ;
	float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv);
	col = lerp(col, 1 - col, Negative*1.5);
	float4 oldfilm = float4(1, 1, 1, 1);
	col *= pow(abs(0.1 * uv.x * (1.0 - uv.x) * uv.y * (1.0 - uv.y)), Contrast) * 1 + Vignette;
	col = dot(float4(0.2126, 0.7152, 0.0722, 1), col);
	col = linearLight(oldfilm, col);

	half4 colIn = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.texcoord);
	float fd = 1;

	if (_FadeMultiplier > 0)
	{
#if ALPHA_CHANNEL
		float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uv).a);
#else
		float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask,uv).r);
#endif
		fd *= alpha_Mask;
	}

	return lerp(colIn, float4(col.rgb,colIn.a), fd* fade);
	}

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "#NAME#"

			Cull Off ZWrite Off ZTest Always

            HLSLPROGRAM
                #pragma fragment Frag
                #pragma vertex Vert
            ENDHLSL
        }

    }
    Fallback Off
}