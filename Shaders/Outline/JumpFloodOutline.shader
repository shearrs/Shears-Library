Shader "Hidden/JumpFloodOutline"
{
    Properties
    {
        _StencilRef ("Stencil Reference", Integer) = 1
        _OutlineWidth("Outline Width", Float) = 4.0
        _FirstOutlineColor("Outline Color", Color) = (1, 1, 1, 1)
        _SecondOutlineColor("Second Outline Color", Color) = (1, 1, 1, 1)
        _ThirdOutlineColor("Third Outline Color", Color) = (1, 1, 1, 1)
        _OutlineHardness("Outline Hardness", Range(0, 1)) = 1.0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Source Blend", Integer) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Destination Blend", Integer) = 10
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
                Ref [_StencilRef]
                Comp NotEqual
                Pass Zero
                Fail Zero
            }

            Blend [_SrcBlend] [_DstBlend]
            ZTest Always

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #pragma multi_compile_local _ IGNORE_DEPTH
            #pragma multi_compile_local _ SECOND_COLOR THIRD_COLOR

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            #define BLEND_ADD (_SrcBlend == 1 && _DstBlend == 1) || (_SrcBlend == 4 && _DstBlend == 1)
            #define BLEND_MULTIPLY (_SrcBlend == 2 && _DstBlend == 0)
            #define NOISE_OCTAVES 5

            CBUFFER_START(UnityPerMaterial)
            float4 _FirstOutlineColor;
            float4 _SecondOutlineColor;
            float4 _ThirdOutlineColor;
            float _OutlineWidth;
            float _OutlineHardness;
            int _SrcBlend;
            int _DstBlend;
            CBUFFER_END

            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453123);
            }

            float Noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(lerp(Hash(i + float2(0.0, 0.0)), 
                                Hash(i + float2(1.0, 0.0)), u.x),
                            lerp(Hash(i + float2(0.0, 1.0)), 
                                Hash(i + float2(1.0, 1.0)), u.x), u.y);
            }
            
            float FractalBrownianMotion(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;

                for (int i = 0; i < NOISE_OCTAVES; i++)
                {
                    value += amplitude * Noise(p * frequency);
                    frequency *= 2.0;
                    amplitude *= 0.5;
                }

                return value;
            }

            float2 CartesianToPolar(float2 cartesianCoordinates)
            {
                float radius = sqrt(cartesianCoordinates.x * cartesianCoordinates.x + cartesianCoordinates.y * cartesianCoordinates.y);
                float theta = atan(cartesianCoordinates.y / cartesianCoordinates.x);

                return float2(radius, theta);
            }

            float4 Frag (Varyings input) : SV_Target
            {
                float2 positionCS = input.positionCS.xy;
                int2 pixel = int2(positionCS);
                float2 encodedPosition = _BlitTexture.Load(int3(pixel, 0)).xy;

                if (encodedPosition.y == FLOOD_NULL_POSITION.y)
                    return BLEND_MULTIPLY ? float4(1, 1, 1, 1) : float4(0, 0, 0, 0);
                
                float2 nearestPosition = DECODE(encodedPosition);
                float distance = length(nearestPosition - positionCS);

                float2 nearestUV = nearestPosition / _ScreenParams.xy;
                float nearestDepth = SampleSceneDepth(nearestUV);
                float linearNearestDepth = LinearEyeDepth(nearestDepth, _ZBufferParams);
                float hardness;
                float outlineWidth;

                #if IGNORE_DEPTH
                hardness = _OutlineHardness / (0.05 * _OutlineWidth);
                outlineWidth = _OutlineWidth;
                #else
                hardness = (_OutlineHardness * linearNearestDepth) / (0.05 *_OutlineWidth);
                outlineWidth = _OutlineWidth / linearNearestDepth;
                #endif

                float noise = 1;

                // FIRE EFFECT
                // float2 uv = input.texcoord;
                // float3 worldPos = ComputeWorldSpacePosition(uv, nearestDepth, UNITY_MATRIX_I_VP);      

                // float2 noiseUV = 4.0 * worldPos.xy;
                // noiseUV.y -= 3.0 * _Time.y;

                // noise = FractalBrownianMotion(noiseUV);

                // float freq = .5;
                // noise = step(0.5, sin(positionCS.x * freq) * sin(positionCS.y * freq));

                // can also do this with screen position
                // float dashCount = 100;
                // float dashUV = sin(uv.x) * sin(uv.y);
                // float dashPattern = sin(dashUV * dashCount);
                // float dashMask = step(0.0, dashPattern);
                // noise = dashMask;

                float outline = saturate(hardness * (outlineWidth * noise - (distance + 1.0)));

                float4 outlineColor;

                #if THIRD_COLOR
                float t = distance / outlineWidth;
                
                if (t <= 0.5)
                    outlineColor = lerp(_FirstOutlineColor, _SecondOutlineColor, t * 2.0);
                else
                    outlineColor = lerp(_SecondOutlineColor, _ThirdOutlineColor, (t - 0.5) * 2.0);
                #elif SECOND_COLOR
                float t = distance / outlineWidth;

                outlineColor = lerp(_FirstOutlineColor, _SecondOutlineColor, t);
                #else
                outlineColor = _FirstOutlineColor;
                #endif
                
                float4 color = outlineColor;

                if (BLEND_ADD)
                    color *= outline;
                else if (BLEND_MULTIPLY)
                    color = lerp(float4(1, 1, 1, 1), color, outline);
                else
                    color.a *= outline;

                return color;
            }
            ENDHLSL
        }
    }
}
