Shader "Hidden/Shader/WarpEffect_RLPRO"
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

	// These are exposed properties. The warp assignment is the default value, which is overwritten. float2 means a 2d float vector
	float2 warp = float2(1.0 / 32.0, 1.0 / 24.0);
	float scale;
	float fade;

	float2 Warp(float2 pos)
	{
		float2 h = pos - float2(0.5, 0.5);
		float r2 = dot(h, h);
		float f = 1.0 + r2 * (warp.x + warp.y * sqrt(r2));
		return f * scale * h + 0.5;
	}
	float2 Warp1(float2 pos)
	{
		pos = pos * 2.0 - 1.0;
		pos *= float2(1.0 + (pos.y * pos.y) * warp.x, 1.0 + (pos.x * pos.x) * warp.y);
		return pos * scale + 0.5;
	}

	// SubShader calls Frag0 calls Warp1
	// Frag calls Warp
	float4 Frag0(Varyings i) : SV_Target
	{
		float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp,i.texcoord);
		float2 fragCoord = i.texcoord.xy * _ScreenParams.xy;
		float2 pos = Warp1(fragCoord.xy / _ScreenParams.xy);

		float4 col2 = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, pos  );
		if (_FadeMultiplier > 0)
		{
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, i.texcoord).a);
			fade *= alpha_Mask;
		}

		return lerp(col,col2,fade);
	}

	float4 Frag(Varyings i) : SV_Target
	{
		float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.texcoord);
		float2 fragCoord = i.texcoord.xy * _ScreenParams.xy;
		float2 pos = Warp(fragCoord.xy / _ScreenParams.xy);

		 if (_FadeMultiplier > 0)
		 {
#if ALPHA_CHANNEL
			 float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, i.texcoord).a);
#else
			 float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, i.texcoord).r);
#endif
			 pos *= alpha_Mask;
		 }

		 float4 col2 = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, pos  );

		return lerp(col,col2,fade);
	}

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "#NAME#"

			Cull Off ZWrite Off ZTest Always

            HLSLPROGRAM
                #pragma fragment Frag0
                #pragma vertex Vert
            ENDHLSL
        }
		
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