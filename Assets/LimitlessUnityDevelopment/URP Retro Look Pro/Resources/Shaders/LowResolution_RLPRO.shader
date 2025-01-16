Shader "Hidden/Shader/LowResolution_RLPRO"
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
	float _FadeMultiplier;
	#pragma shader_feature ALPHA_CHANNEL
	half Width;
    half Height;

	float4 Frag(Varyings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        float2 uv = i.texcoord;
        uv.x *= Width;
        uv.y *= Height;
        uv.x = round(uv.x);
        uv.y = round(uv.y);
        uv.x /= Width;
        uv.y /= Height;

		float2 pos = uv;
		float2 centerTextureCoordinate = pos;

		float4 fragmentColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, centerTextureCoordinate);
		half4 colIn = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.texcoord);
		float fd = 1;

		if (_FadeMultiplier > 0)
		{
			#if ALPHA_CHANNEL
						float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, i.texcoord).a);
			#else
						float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, i.texcoord).r);
			#endif
						fd *= alpha_Mask;
		}

		return lerp(colIn, fragmentColor, fd);

	}

    ENDHLSL

    SubShader
    {
			Pass
		{
			Name "#Blit#"

			Cull Off ZWrite Off ZTest Always

			HLSLPROGRAM
				#pragma fragment Frag
				#pragma vertex Vert
			ENDHLSL
		}

    }
    Fallback Off
}