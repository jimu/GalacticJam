Shader "Hidden/Shader/EdgeStretchEffect_RLPRO"
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

    //SAMPLER(_MainTex);
	float amplitude;
	float frequency;
	half _NoiseBottomHeight;
	float Time;
	half speed;
	#pragma shader_feature top_ON
	#pragma shader_feature bottom_ON
	#pragma shader_feature left_ON
	#pragma shader_feature right_ON

	float onOff(float a, float b, float c, float t)
	{
		return step(c, sin(t + a * cos(t * b)));
	}
	float2 wobble(float2 uv, float amplitude, float frequence, float speed)
	{
		float offset = amplitude * sin(uv.y * frequence * 20.0 + Time * speed);
		return float2((uv.x+(20 * _NoiseBottomHeight)) + offset, uv.y);
	}	
	float2 wobbleR(float2 uv, float amplitude, float frequence, float speed)
	{
		float offset = amplitude * onOff(2.1, 4.0, 0.3, Time * speed) * sin(uv.y * frequence * 20.0 + Time * speed);
		return float2((uv.x+(20 * _NoiseBottomHeight)) + offset, uv.y);
	}
	float2 wobbleV(float2 uv, float amplitude, float frequence, float speed)
	{
		float offset = amplitude * sin(uv.x * frequence * 20.0 + Time * speed);
		return float2((uv.y + (20 * _NoiseBottomHeight)) + offset, uv.x);
	}	
	float2 wobbleVR(float2 uv, float amplitude, float frequence, float speed)
	{
		float offset = amplitude * onOff(2.1, 4.0, 0.3, Time * speed) * sin(uv.x * frequence * 20.0 + Time * speed);
		return float2((uv.y + (20 * _NoiseBottomHeight)) + offset, uv.x);
	}
	float4 FragDist(Varyings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
		#if top_ON
			i.texcoord.y = min(i.texcoord.y, 1 - (wobble(i.texcoord, amplitude, frequency, speed).x * (_NoiseBottomHeight / 20)));

		#endif
		#if bottom_ON
			i.texcoord.y = max(i.texcoord.y, wobble(i.texcoord,amplitude, frequency, speed).x * (_NoiseBottomHeight/20));
		#endif
		#if left_ON
			i.texcoord.x = max(i.texcoord.x, wobbleV(i.texcoord, amplitude, frequency, speed).x * (_NoiseBottomHeight / 20));
		#endif
		#if right_ON
			i.texcoord.x = min(i.texcoord.x, 1 - (wobbleV(i.texcoord, amplitude, frequency, speed).x * (_NoiseBottomHeight / 20)));
		#endif
		half2 positionSS = i.texcoord ;
		half4 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, positionSS);
		float exp = 1.0;
		return float4(pow(color.xyz, float3(exp, exp, exp)), color.a);
	}
	float4 FragDistRand(Varyings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);	
		#if top_ON
			i.texcoord.y = min(i.texcoord.y, 1 - (wobbleR(i.texcoord, amplitude, frequency, speed).x * (_NoiseBottomHeight / 20)));
		#endif
		#if bottom_ON
			i.texcoord.y = max(i.texcoord.y, wobbleR(i.texcoord, amplitude, frequency, speed).x * (_NoiseBottomHeight / 20));
		#endif
		#if left_ON
			i.texcoord.x = max(i.texcoord.x, wobbleVR(i.texcoord, amplitude, frequency, speed).x * (_NoiseBottomHeight / 20));
		#endif
		#if right_ON
			i.texcoord.x = min(i.texcoord.x, 1 - (wobbleVR(i.texcoord, amplitude, frequency, speed).x * (_NoiseBottomHeight / 20)));
		#endif
		half2 positionSS = i.texcoord  ;
		half4 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, positionSS);
		float exp = 1.0;
		return float4(pow(color.xyz, float3(exp, exp, exp)), color.a);
	}
	float4 Frag(Varyings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
		 half2 positionSS = i.texcoord  ;
		#if top_ON
				 positionSS.y = min(positionSS.y, 1-(_NoiseBottomHeight / 2) - 0.01);
		#endif
		#if bottom_ON
				 positionSS.y = max(positionSS.y, (_NoiseBottomHeight / 2) - 0.01);
		#endif
		#if left_ON
				 positionSS.x = max(positionSS.x, (_NoiseBottomHeight / 2) - 0.01);
		#endif
		#if right_ON
				 positionSS.x = min(positionSS.x, 1-(_NoiseBottomHeight / 2) - 0.01);
		#endif

		half4 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, positionSS);
		float exp = 1.0;
		return float4(pow(color.xyz, float3(exp, exp, exp)), color.a);
	}
    ENDHLSL

    SubShader
    {
		Cull Off ZWrite Off ZTest Always

			Pass
		{
			HLSLPROGRAM

				#pragma vertex Vert
				#pragma fragment FragDist

			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM

				#pragma vertex Vert
				#pragma fragment FragDistRand

			ENDHLSL
		}
			Pass
		{
			HLSLPROGRAM

				#pragma vertex Vert
				#pragma fragment Frag

			ENDHLSL
		}
    }
    Fallback Off
}