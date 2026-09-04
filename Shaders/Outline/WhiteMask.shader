Shader "Custom/WhiteMask"
{
    Properties
    {
        _StencilRef ("Stencil Reference", Integer) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Integer) = 4
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        ZTest [_ZTest]
        ZWrite Off

        Pass
        {
            Name "StencilMask"

            Stencil {
                Ref [_StencilRef]
                Comp Greater
                Pass Replace
            }

            ColorMask 0
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
            Name "WhiteMask"

            Stencil {
                Ref [_StencilRef]
                Comp Equal
                Pass Keep
            }

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

            float frag() : SV_Target
            {
                return 1.0;
            }
            ENDHLSL
        }
    }
}
