Shader "Hidden/Shader/OldFilm2Effect_RLPRO"
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
		
		SAMPLER(_MainTex);
	TEXTURE2D(_Mask);
	SAMPLER(sampler_Mask);
	float _FadeMultiplier;
	#pragma shader_feature ALPHA_CHANNEL

	float Fade;
	float rand(float2 n) {
		return frac(sin(dot(n.xy, float2(12.9898, 78.233))) * 43758.5453);
	}
	float rand(float2 uv, float t) {
		return frac(sin(dot(uv, float2(1225.6548, 321.8942))) * 4251.4865 + t);
	}
	float SepiaValue;
	float _Grain;
	float NoiseValue;
	float ScratchValue;
	float ScratchSize;
	float ScratchResolution;
	float OuterVignetting;
	float speed;

	float3 Overlay(float3 src, float3 dst)
	{
		return float3((dst.x <= 0.5) ? (2.0 * src.x * dst.x) : (1.0 - 2.0 * (1.0 - dst.x) * (1.0 - src.x)),
			(dst.y <= 0.5) ? (2.0 * src.y * dst.y) : (1.0 - 2.0 * (1.0 - dst.y) * (1.0 - src.y)),
			(dst.z <= 0.5) ? (2.0 * src.z * dst.z) : (1.0 - 2.0 * (1.0 - dst.z) * (1.0 - src.z)));
	}

	float3 mod289(float3 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
	float2 mod289(float2 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
	float3 permute(float3 x) { return mod289(((x * 34.0) + 1.0) * x); }
	float nrand(float2 n) { return frac(sin(dot(n.xy, float2(12.9898, 78.233))) * 43758.5453); }
	float snoise(float2 v)
	{
		const float4 C = float4(0.211324865405187,
			0.366025403784439,
			-0.577350269189626,
			0.024390243902439);

		float2 i = floor(v + dot(v, C.yy));
		float2 x0 = v - i + dot(i, C.xx);

		float2 i1;
		i1 = (x0.x > x0.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
		float4 x12 = x0.xyxy + C.xxzz;
		x12.xy -= i1;

		i = mod289(i);
		float3 p = permute(permute(i.y + float3(0.0, i1.y, 1.0))
			+ i.x + float3(0.0, i1.x, 1.0));

		float3 m = max(0.5 - float3(dot(x0, x0), dot(x12.xy, x12.xy), dot(x12.zw, x12.zw)), 0.0);
		m = m * m;
		m = m * m;
		float3 x = 2.0 * frac(p * C.www) - 1.0;
		float3 h = abs(x) - 0.5;
		float3 ox = floor(x + 0.5);
		float3 a0 = x - ox;
		m *= 1.79284291400159 - 0.85373472095314 * (a0 * a0 + h * h);
		float3 g;
		g.x = a0.x * x0.x + h.x * x0.y;
		g.yz = a0.yz * x12.xz + h.yz * x12.yw;
		return 130.0 * dot(m, g);
	}


		float4 Frag0(Varyings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		float2 uv = i.texcoord;
		float2 ps = float2(1.0, 1.0) / _ScreenParams.xy;
		float scale = 10.0 * _Grain;
		float2 offset = (rand(uv, _Time.y) - 0.5) * 2.0 * ps * scale;
		float4 noise1 = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + offset);
		float4 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv);
		float amount = 1 * _Grain;
		float4 colour = lerp(color, noise1, amount);

		float2 vUv = i.texcoord;
		float TimeLapse = floor(_Time.y * speed * 1000. / 50.);
		float3 sepia = float3(112.0 / 255.0, 66.0 / 255.0, 20.0 / 255.0);

		float gray = (colour.x + colour.y + colour.z) / 3.0;
		float3 grayscale = float3(gray, gray, gray);

		float RandomValue = rand(vUv * _Time.y);

		float3 finalColour = Overlay(sepia, grayscale);

		finalColour = grayscale + SepiaValue * (finalColour - grayscale);

		float noise = snoise(vUv * float2(1024.0 + RandomValue * 512.0, 1024.0 + RandomValue * 512.0)) * 0.5;
		finalColour += noise * NoiseValue;

		if (RandomValue < ScratchValue)
		{
			float dist = 1.0 / ScratchValue;
			float d = distance(vUv, float2(RandomValue * dist, RandomValue * dist));
			if (d < 0.6)
			{
				float xPeriod = 20.0 * ScratchSize;
				float yPeriod = 20.0 * ScratchSize;
				float pi = 3.141592;
				float phase = TimeLapse;
				float turbulence = snoise(vUv * 20 * ScratchResolution);
				float vScratch = 0.5 + (sin(((vUv.x * xPeriod + vUv.y * yPeriod + turbulence)) * pi + phase) * 0.5);
				vScratch = clamp((vScratch * 10000.0) + 0.35, 0.0, 1.0);

				finalColour.xyz /= vScratch;
			}
		}
		if (_FadeMultiplier > 0)
		{
#if ALPHA_CHANNEL
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uv).a);
#else
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uv).r);
#endif
			Fade *= alpha_Mask;
		}

		return lerp(color, float4(finalColour, color.a), Fade);
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
    }
    Fallback Off
}