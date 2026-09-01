Shader "Hidden/JumpFloodOutline"
{
    Properties
    {
        _OutlineWidth("Outline Width", Float) = 4.0
        _OutlineColor("Outline Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        #define SNORM16_MAX_FLOAT_MINUS_EPSILON ((float)(32768-2) / (float)(32768-1))
        #define FLOOD_ENCODE_OFFSET float2(1.0, SNORM16_MAX_FLOAT_MINUS_EPSILON)
        #define FLOOD_ENCODE_SCALE float2(2.0, 1.0 + SNORM16_MAX_FLOAT_MINUS_EPSILON)
        #define FLOOD_NULL_POSITION float2(-1.0, -1.0)
        #define ENCODE(position) ((position) * abs(_BlitTexture_TexelSize.xy) * FLOOD_ENCODE_SCALE - FLOOD_ENCODE_OFFSET)
        #define DECODE(position) (((position) + FLOOD_ENCODE_OFFSET) * abs(_BlitTexture_TexelSize.zw) / FLOOD_ENCODE_SCALE)
        ENDHLSL

        Tags { "RenderType"="Opaque" }
        LOD 100
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Name "InitializeBuffer"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            float2 Frag (Varyings input) : SV_Target
            {
                float2 positionCS = input.positionCS.xy;
                int2 pixel = int2(positionCS);

                float3x3 kernelValues = 0;
                UNITY_UNROLL
                for (int u = 0; u < 3; u++)
                {
                    UNITY_UNROLL
                    for (int v = 0; v < 3; v++)
                    {
                        int2 sampleUV = clamp(pixel + int2(u - 1, v - 1), int2(0, 0), (int2)_BlitTexture_TexelSize.zw - 1);
                        kernelValues[u][v] = _BlitTexture.Load(int3(sampleUV, 0)).r;
                    }
                }

                float2 encodedPosition = ENCODE(positionCS);
                float centerValue = kernelValues._m11;

                if (centerValue > 0.99) // We are fully inside, no need to get direction
                    return encodedPosition;
                else if (centerValue < 0.01) // We are fully outside, no need to get direction
                    return FLOOD_NULL_POSITION;
                // Else, we are partially inside

                float2 direction = -float2(
                    kernelValues[0][0] + kernelValues[0][1] * 2.0 + kernelValues[0][2] - kernelValues[2][0] - kernelValues[2][1] * 2.0 - kernelValues[2][2],
                    kernelValues[0][0] + kernelValues[1][0] * 2.0 + kernelValues[2][0] - kernelValues[0][2] - kernelValues[1][2] * 2.0 - kernelValues[2][2]
                );

                if (abs(direction.x) < 0.005 && abs(direction.y) < 0.005)
                    return encodedPosition;
                
                direction = normalize(direction);

                float2 offset = direction * (1.0 - centerValue);

                return ENCODE(positionCS + offset);
            }
            ENDHLSL
        }
        Pass
        {
            Name "JumpFlood"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            int2 _AxisWidth;
            
            float2 Frag (Varyings input) : SV_Target
            {
                float2 positionCS = input.positionCS.xy;
                int2 pixel = int2(positionCS);

                float bestDistance = 1.#INF;
                float2 bestCoordinate;

                UNITY_UNROLL
                for (int u = -1; u <= 1; u++)
                {
                    int2 sampleUV = pixel + _AxisWidth * u;
                    sampleUV = clamp(sampleUV, int2(0, 0), (int2)_BlitTexture_TexelSize.zw - 1);

                    float2 encodedPosition = _BlitTexture.Load(int3(sampleUV, 0)).xy;
                    float2 samplePosition = DECODE(encodedPosition);
                    float2 displacement = positionCS - samplePosition;
                    float sqrDistance = dot(displacement, displacement);

                    if (samplePosition.x != FLOOD_NULL_POSITION.x && sqrDistance < bestDistance)
                    {
                        bestDistance = sqrDistance;
                        bestCoordinate = samplePosition;
                    }
                }

                return isinf(bestDistance) ? FLOOD_NULL_POSITION : ENCODE(bestCoordinate);
            }
            ENDHLSL
        }
        Pass
        {
            Name "Outline"

            Stencil {
                Ref 2
                Comp NotEqual
                Pass Zero
                Fail Zero
            }

            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            float4 _OutlineColor;
            float _OutlineWidth;

            float4 Frag (Varyings input) : SV_Target
            {
                float2 positionCS = input.positionCS.xy;
                int2 pixel = int2(positionCS);
                float2 encodedPosition = _BlitTexture.Load(int3(pixel, 0)).xy;

                if (encodedPosition.y == FLOOD_NULL_POSITION.y)
                    return float4(0, 0, 0, 0);
               
                float2 nearestPosition = DECODE(encodedPosition);
                float distance = length(nearestPosition - positionCS);
                float2 nearestUV = nearestPosition / _BlitTexture_TexelSize.zw;


                float rawDepth = SampleSceneDepth(nearestUV);
                float linearDepth = LinearEyeDepth(rawDepth, _ZBufferParams);

                float outline = saturate(((_OutlineWidth / linearDepth) - distance + 1.0));

                float4 color = _OutlineColor;
                color.a *= outline;

                return color;
            }
            ENDHLSL
        }
    }
}
