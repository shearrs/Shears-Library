Shader "Custom/WhiteMask"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "StencilMask"

            Stencil {
                Ref 2
                Comp NotEqual
                Pass Replace
            }

            ColorMask 0
            Cull Off
            ZWrite Off
            ZTest LEqual
            Blend Zero One

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            float4 vert(Attributes IN) : SV_POSITION
            {
                return TransformObjectToHClip(IN.positionOS.xyz);
            }

            void frag() {}
            ENDHLSL
        }
        Pass
        {
            Stencil {
                Ref 2
                Comp Equal
                Pass Keep
            }

            Name "WhiteMask"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            float frag(Varyings IN) : SV_Target
            {
                return 1.0;
            }
            ENDHLSL
        }
    }
}
