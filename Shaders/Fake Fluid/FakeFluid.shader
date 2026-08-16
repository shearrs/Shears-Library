Shader "Custom/Fake Fluid"
{
    Properties
    {
        [MainColor] _OuterColor("Outer Color", Color) = (1, 1, 1, 1)
        _TopColor("Top Color", Color) = (0.8, 0.8, 0.8, 1)
        _HeightCutoff("Height Cutoff", Float) = 0.75
        _Wobbliness("Wobbliness", Float) = 0.5
        _WobbleVelocity("Wobble Velocity", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags { "RenderType" = "TransparentCutout" "Queue" = "AlphaTest" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "Front"
            Cull Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normal : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionOffset : TEXCOORD0;
                float3 normal : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _OuterColor;
                float4 _TopColor;
                float _HeightCutoff;
                float _Wobbliness;
                float3 _WobbleVelocity;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs positions = GetVertexPositionInputs(IN.positionOS.xyz);

                OUT.positionHCS = positions.positionCS;

                float3x3 worldRotationAndScale = (float3x3)UNITY_MATRIX_M;

                float3x3 pureRotationMatrix;
                pureRotationMatrix[0] = normalize(worldRotationAndScale[0]);
                pureRotationMatrix[1] = normalize(worldRotationAndScale[1]);
                pureRotationMatrix[2] = normalize(worldRotationAndScale[2]);

                float3 rotatedObjectPosition = mul(pureRotationMatrix, IN.positionOS.xyz);

                OUT.positionOffset = rotatedObjectPosition;
                OUT.normal = TransformObjectToWorldNormal(IN.normal);

                return OUT;
            }

            float4 frag(Varyings IN, bool isFrontFace : SV_IsFrontFace) : SV_Target
            {
                float wobbleAmount = 0.5 * (dot(IN.positionOffset, _WobbleVelocity * _Wobbliness) + 1);
                float clipValue = step(IN.positionOffset.y + wobbleAmount, _HeightCutoff) - 1;

                clip(clipValue);

                float4 albedo = lerp(_TopColor, _OuterColor, (float)isFrontFace);
                float3 normal = lerp(float3(0, 1, 0), IN.normal, (float)isFrontFace);
                Light mainLight = GetMainLight();
                float ndotl = 0.5 * (dot(normal, mainLight.direction) + 1);

                float3 diffuse = ndotl * albedo.rgb;

                return float4(diffuse, albedo.a);
            }
            ENDHLSL
        }
    }
}
