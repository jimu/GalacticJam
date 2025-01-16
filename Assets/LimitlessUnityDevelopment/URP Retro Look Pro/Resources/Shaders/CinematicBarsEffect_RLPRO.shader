Shader "Hidden/Shader/CinematicBarsEffect_RLPRO"
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
	half _Stripes;
	half _Fade;

    float4 CustomPostProcess(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        half2 positionSS = input.texcoord;
        float4 outColor = float4(0,0,0,1);
		float4 outColor2 = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, positionSS);
		outColor = lerp(outColor, outColor2, (1 - ceil(saturate(abs(input.texcoord.y - 0.5) - _Stripes))));
		return lerp(outColor2, outColor, _Fade);        
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "#CinematicBars#"
			Cull Off ZWrite Off ZTest Always

            HLSLPROGRAM
                #pragma fragment CustomPostProcess
                #pragma vertex Vert
            ENDHLSL
        }
    }
    Fallback Off
}