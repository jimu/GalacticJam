Shader "Hidden/Shader/UltimateVignetteEffect_RLPRO"
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
    
    half4 _Params;
	half3 _InnerColor;
	half4 _Center;
#pragma shader_feature VIGNETTE_CIRCLE
#pragma shader_feature VIGNETTE_SQUARE
#pragma shader_feature VIGNETTE_ROUNDEDCORNERS
	half2 _Params1;


		float4 Frag(Varyings i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
	half4 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.texcoord);

#if VIGNETTE_CIRCLE
	half d = distance(i.texcoord, _Center.xy);
	half multiplier = smoothstep(0.8, _Params.x * 0.799, d * (_Params.y + _Params.x));
#elif VIGNETTE_ROUNDEDCORNERS
	half2 uv = -i.texcoord * i.texcoord + i.texcoord;
	half v = saturate(uv.x * uv.y * _Params1.x + _Params1.y);
	half multiplier = smoothstep(0.8, _Params.x * 0.799, v * (_Params.y + _Params.x));
#else
	half multiplier = 1.0;
#endif
	_InnerColor = -_InnerColor;
	color.rgb = (color.rgb - _InnerColor) * max((1.0 - _Params.z * (multiplier - 1.0) - _Params.w), 1.0) + _InnerColor;
	color.rgb *= multiplier;

	return color;
	}

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "#UltimateVignetteEffect_RLPRO#"

		Cull Off ZWrite Off ZTest Always

            HLSLPROGRAM
                #pragma fragment Frag
                #pragma vertex Vert
            ENDHLSL
        }
    }
    Fallback Off
}