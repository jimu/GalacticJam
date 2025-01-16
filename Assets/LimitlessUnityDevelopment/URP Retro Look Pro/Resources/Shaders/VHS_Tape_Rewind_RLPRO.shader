Shader "Hidden/Shader/VHS_Tape_Rewind"
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

	half fade;
	half intencity;

	half4 Frag(Varyings i) : COLOR
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

		float2 uv = i.texcoord;
		float2 displacementSampleUV = float2(uv.x + (_Time.y+20)*70, uv.y);

		float da = intencity;

		float displacement = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, displacementSampleUV).x * da;

		float2 displacementDirection = float2(cos(displacement * 6.28318530718), sin(displacement * 6.28318530718));
		float2 displacedUV = (uv + displacementDirection * displacement);
		float4 shade = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, displacedUV);
		float4 main = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv);
		return float4(lerp(main,shade,fade));
	}
		ENDHLSL

		Subshader
	{
		Pass
		{
			ZTest Always Cull Off ZWrite Off
			 HLSLPROGRAM
			 #pragma fragmentoption ARB_precision_hint_fastest
			 #pragma vertex Vert
			 #pragma fragment Frag
			 ENDHLSL
		}


	}
	Fallback off
}

