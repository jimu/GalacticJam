Shader "Hidden/Shader/ColormapPaletteEffect_RLPRO"
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

    float _Intensity;
	float4 downsample;
	sampler3D _Colormap;
	float4 _Colormap_TexelSize;
	sampler2D _Palette;
	sampler2D _BlueNoise;
	float4 _BlueNoise_TexelSize;
	float _Opacity;
	float _Dither;
	float width;
	float height;
	float2 Resolution;
	half CalcLuminance(float3 color)
	{
		return dot(color, float3(0.299f, 0.587f, 0.114f));
	}

	float4 Frag0(Varyings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
		float2 uv = i.texcoord;

		if (_FadeMultiplier > 0)
		{
			#if ALPHA_CHANNEL
						float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uv).a);
			#else
						float alpha_Mask = step(0.0001, SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uv).r);
			#endif
			_Opacity *= alpha_Mask;
		}

		float2 uv2 = uv;
		uv2.x *= width;
		uv2.y *= height;
		uv2.x = round(uv2.x);
		uv2.y = round(uv2.y);
		uv2.x /= width;
		uv2.y /= height;
		float4 inputColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv2);
		float4 inputColor1 = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv);

		inputColor = saturate(inputColor);
		float4 colorInColormap = tex3D(_Colormap, inputColor.rgb);
		float random = tex2D(_BlueNoise, i.positionCS.xy  / _BlueNoise_TexelSize.z).r;
		random = saturate(random);
		if (CalcLuminance(colorInColormap.r) > CalcLuminance(colorInColormap.g))
		{
			random = 1 - random;
		}
		float paletteIndex;
		float blend = colorInColormap.b;
		float threshold = saturate((1 / _Dither) * (blend - 0.5 + (_Dither / 2)));
		if (random < threshold)
		{
			paletteIndex = colorInColormap.g;
		}
		else
		{
			paletteIndex = colorInColormap.r;
		}


		float4 result = tex2D(_Palette, float2(paletteIndex, 0));
		result.a = inputColor.a;
		result = lerp(inputColor1, result, _Opacity);
		return result;
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