 Shader "Custom/FullscreenDither"
{
    Properties
    {
        _ColorPalette("Color Palette", 3D) = "white" {}
        _DitherStrength("Dither Strength", Range(0, 0.5)) = 0.02
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            ZTest Always
            ZWrite Off

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE3D(_ColorPalette);

            float _DitherStrength;

            inline float Bayer8x8(uint2 pixelPos)
            {
                uint x = pixelPos.x & 7;
                uint y = pixelPos.y & 7;
                
                uint v0 = (y >> 2) & 1;
                uint v1 = ((x >> 2) & 1) ^ v0;
                uint v2 = (y >> 1) & 1;
                uint v3 = ((x >> 1) & 1) ^ v2;
                uint v4 = y & 1;
                uint v5 = (x & 1) ^ v4;
                
                uint index = (v5 << 5) | (v4 << 4) | (v3 << 3) | (v2 << 2) | (v1 << 1) | v0;
                
                return (float)index / 64.0f - 0.5f;
            }

            float4 Frag(Varyings IN) : SV_Target
            {
                uint2 pixel = uint2(IN.positionCS.xy);
                float4 blitSource = FragBlit(IN, sampler_PointClamp);
                float dither = Bayer8x8(pixel) * _DitherStrength;
                float3 ditheredColor = clamp(blitSource.rgb + dither, 0.0f, 1.0f);

                float4 paletteColor = SAMPLE_TEXTURE3D(_ColorPalette, sampler_PointClamp, ditheredColor.rgb);

                return paletteColor;
            }
            ENDHLSL
        }
    }
}
