Shader "Custom/WhiteMask"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite Texture", 2D) = "white" {}
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
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;

                return OUT;
            }

            void frag(Varyings IN)
            {
                float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                clip(color.a - 0.1);
            }
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
