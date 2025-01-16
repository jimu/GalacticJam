Shader "Hidden/Shader/Noise2Effect_RLPRO"
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


	float hash(float2 _v) {
		return frac(sin(dot(_v, float2(89.44, 19.36))) * 22189.22);
	}

	float iHash(float2 _v, float2 _r) {
		float h00 = hash(float2(floor(_v * _r + float2(0.0, 0.0)) / _r));
		float h10 = hash(float2(floor(_v * _r + float2(1.0, 0.0)) / _r));
		float h01 = hash(float2(floor(_v * _r + float2(0.0, 1.0)) / _r));
		float h11 = hash(float2(floor(_v * _r + float2(1.0, 1.0)) / _r));
		float2 ip = float2(smoothstep(float2(0.0, 0.0), float2(1.0, 1.0), fmod(_v * _r, 1.)));
		return (h00 * (1. - ip.x) + h10 * ip.x) * (1. - ip.y) + (h01 * (1. - ip.x) + h11 * ip.x) * ip.y;
	}

	float noise(float2 _v) {
		float sum = 0.;
		for (int i = 1; i < 9; i++)
		{
			sum += iHash(_v + float2(i,i), float2(2. * pow(2., float(i)), 2. * pow(2., float(i)))) / pow(2., float(i));
		}
		return sum;
	}
	TEXTURE2D(_Mask);
	SAMPLER(sampler_Mask);
	float _FadeMultiplier;
#pragma shader_feature ALPHA_CHANNEL

	float Smoother;
	float threshold;
	float Fade;
	half waveAmount;
	half tapeLinesAmount;
	half tapeIntensity;
	half tapeSpeed;
	int NoiseVal;
	float getNoise(float2 uv) {
		uv += float2(sin(_Time.y), sin(_Time.y));
		return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
	}

	float blurNoise(float2 uv) {
		float noise = getNoise(uv);
		for (int i = 1; i < 3; ++i) {
			noise += getNoise(uv + float2(1.0, 0.0));
			noise += getNoise(uv + float2(0.0, 1.0));
			noise += getNoise(uv - float2(1.0, 0.0));
			noise += getNoise(uv - float2(0.0, 1.0));
		}
		return noise * 0.083;
	}

	half3 rand(half2 coord) {
		half3 a = frac(cos(coord.x * 8.3e-3 + coord.y) * half3(1.3e5, 4.7e5, 2.9e5));
		half3 b = frac(sin(coord.x * 8.3e-3 + coord.y) * half3(8.1e5, 1.0e5, 0.1e5));
		half3 c = lerp(a, b, .5);
		return c;
	}


	float4 Frag(Varyings i) : SV_Target
	{
		float2 uv = i.texcoord;

		float2 uvn = uv;

		uvn.x += (noise(float2(uvn.y, _Time.y)) - 0.5) * 0.005* waveAmount;
		uvn.x += (noise(float2(uvn.y * 100.0, _Time.y * 10.0)) - 0.5) * 0.01* waveAmount;

		float tcPhase = clamp((sin(uvn.y * 8.0* tapeLinesAmount - _Time.y*tapeSpeed * PI * 1.2) - 0.92) * noise(float2(_Time.y, _Time.y)), 0.0, 0.01) * 10.0* tapeIntensity;
		float tcNoise = max(noise(float2(uvn.y * 100.0, _Time.y * 5.0)) - 0.5, 0.0);
		uvn.x = uvn.x - tcNoise * tcPhase;

		float4 col1 = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv);


		float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uvn);

		float noise1 = smoothstep(0.9, 1.0, blurNoise(uv) * 1.9);

		half4 color = half4(rand(uv * sin(10.0 * _Time.y)), 1) * 2. - 1.;

		half4 nVal = half4(0, 0, 0, 0);
		if (NoiseVal)
			nVal = color;
		else
			nVal = half4(noise1, noise1, noise1, noise1);
		if (threshold < 1) {

			if (Smoother > 0)
				col = lerp(nVal, col, smoothstep(max(col.r, max(col.g, col.b)), 1.0, clamp(threshold + 0.135, 0.0, 1.0)));
			else
				col = max(col.r, max(col.g, col.b)) >= threshold ? nVal : col;
		}

		if (0.5 < abs(uvn.x - 0.5)) 
		{
					col = float4(0.1, 0.1, 0.1, 1);
				}

		col *= 1.0 - tcPhase;

		col *= 1.0 + clamp(noise(float2(0.0, uv.y + _Time.y * 0.2)) * 0.06 - 0.25, 0.0, 0.1);
		if (_FadeMultiplier > 0)
		{
#if ALPHA_CHANNEL
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uv).a);
#else
			float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uv).r);
#endif
			Fade *= alpha_Mask;
		}

		return lerp(col1, col, Fade);

	}

		ENDHLSL

		SubShader
	{
		Pass
		{
			Name "#GuillotineEffect_RLPRO#"

		Cull Off ZWrite Off ZTest Always

			HLSLPROGRAM
				#pragma fragment Frag
				#pragma vertex Vert
			ENDHLSL
		}
	}
	Fallback Off
}