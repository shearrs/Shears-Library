Shader "Shears/Toon"
{
    Properties
    {
        [MainColor] _Color("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _MainTex("Base Map", 2D) = "white" {}
        [Toggle] _ALPHA_CLIP("Alpha Clip", Integer) = 0
        _AlphaClipThreshold("Alpha Clip Threshold", Range(0, 1)) = 0.5
        _ColorBands("Color Bands", Range(1, 8)) = 4
        _Smoothness("Smoothness", Range(0, 1)) = 0
        [Toggle] _RECEIVE_SHADOWS("Receive Shadows", Integer) = 1
        [Toggle] _RIM_LIGHTING("Rim Lighting", Integer) = 1
        _RimLightRadius("Rim Light Radius", Range(0, 16)) = 12
        _RimLightSmoothness("Rim Light Smoothness", Range(0, 1)) = 0.01
        _RimLightStrength("Rim Light Strength", Range(0, 1)) = 1
        _RimLightCutoff("Rim Light Cutoff", Range(0, 1)) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2
        [HideInInspector] _SurfaceType("Surface Type", Integer) = 0
        [HideInInspector] _SrcBlend("__src", Integer) = 1
        [HideInInspector] _DstBlend("__dst", Integer) = 0
        [HideInInspector] _ZWrite("__zw", Integer) = 1
    }

    SubShader
    {

        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }

            Blend [_SrcBlend] [_DstBlend]
            Cull [_Cull]
            ZWrite [_ZWrite]

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _ALPHA_CLIP_ON
            #pragma shader_feature _RECEIVE_SHADOWS_ON
            #pragma shader_feature _RIM_LIGHTING_ON
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS  : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normal : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _AlphaClipThreshold;
                int _ColorBands;
                float _Smoothness;
                float _RimLightRadius;
                float _RimLightSmoothness;
                float _RimLightStrength;
                float _RimLightCutoff;
            CBUFFER_END

            struct ToonLightData
            {
                float3 directLight;
                float3 specularLight;
                float directBrightness;
                float specularBrightness;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs positions = GetVertexPositionInputs(IN.positionOS.xyz);

                OUT.positionCS = positions.positionCS;
                OUT.positionWS = positions.positionWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.normal = TransformObjectToWorldNormal(IN.normal);

                return OUT;
            }

            float3 RGBtoHSV(float3 c)
            {
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }

            float3 HSVtoRGB(float3 c)
            {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }

            void GetMainLighting(InputData inputData, inout ToonLightData data)
            {
                float3 normal = inputData.normalWS;
                float4 shadowCoords = TransformWorldToShadowCoord(inputData.positionWS);
                Light light = GetMainLight(shadowCoords);
                float shadowAttenuation = 1.0;

                #if _RECEIVE_SHADOWS_ON
                shadowAttenuation = light.shadowAttenuation;
                #endif

                float ndotl = dot(normal, light.direction);
                float remappedNDotL = saturate(ndotl);
                float brightness = saturate(remappedNDotL * light.distanceAttenuation * shadowAttenuation);
                brightness += RGBtoHSV(SampleSH(normal)).z;
                brightness = floor(_ColorBands * brightness) / _ColorBands;
                
                data.directLight += light.color * brightness;
                data.directBrightness += brightness;

                float3 halfVector = normalize(light.direction + inputData.viewDirectionWS);
                float specularBrightness = lerp(0, saturate(dot(halfVector, normal)), step(0, ndotl * shadowAttenuation));

                float specularExponent = exp2(_Smoothness * 11) + 2;
                specularBrightness = pow(specularBrightness, specularExponent) * _Smoothness;
                specularBrightness = floor(_ColorBands * specularBrightness) / _ColorBands;

                data.specularLight += light.color * specularBrightness;
                data.specularBrightness += specularBrightness;
            }

            void GetAdditionalLighting(int lightIndex, InputData inputData, inout ToonLightData data)
            {
                Light additionalLight = GetAdditionalLight(lightIndex, inputData.positionWS, inputData.shadowMask);
                float shadowAttenuation = 1.0;

                #if _RECEIVE_SHADOWS_ON
                shadowAttenuation = additionalLight.shadowAttenuation;
                #endif

                float lightIntensity = max(additionalLight.color.r, max(additionalLight.color.g, additionalLight.color.b));
                float distanceAttenuation = additionalLight.distanceAttenuation * lightIntensity;
                float ndotl = dot(inputData.normalWS, additionalLight.direction);
                float brightness = 0.5 * (ndotl + 1);
                brightness *= distanceAttenuation * shadowAttenuation;
                brightness = saturate(brightness);
                brightness = floor(_ColorBands * brightness) / _ColorBands;

                data.directLight += brightness * additionalLight.color;
                data.directBrightness += brightness;

                float3 halfVector = normalize(additionalLight.direction + inputData.viewDirectionWS);
                float specularBrightness = lerp(0, saturate(dot(halfVector, inputData.normalWS)), step(0, ndotl));

                float specularExponent = exp2(_Smoothness * 11) + 2;
                specularBrightness = pow(abs(specularBrightness), specularExponent) * _Smoothness;
                specularBrightness = floor(_ColorBands * specularBrightness) / _ColorBands;

                data.specularLight += additionalLight.color * specularBrightness;
                data.specularBrightness += specularBrightness;
            }

            void GetRimLight(InputData inputData, inout ToonLightData data)
            {
                float fresnel = pow(abs(1.0 - dot(inputData.normalWS, inputData.viewDirectionWS)), _RimLightRadius);
                float maxDirect = data.directBrightness;
                float directValue = min(data.directBrightness * data.directBrightness, maxDirect);
                fresnel = _RimLightStrength * directValue * smoothstep(0.01, 0.01 + _RimLightSmoothness, fresnel);
                fresnel = step(_RimLightCutoff, fresnel) * fresnel;

                data.specularLight += fresnel;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * _Color;
                float3 normal = normalize(IN.normal);

                #if _ALPHA_CLIP_ON
                clip(albedo.a - _AlphaClipThreshold);
                #endif

                InputData inputData = (InputData)0;
                inputData.positionWS = IN.positionWS;
                inputData.normalWS = normal;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(IN.positionWS);;
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionCS);

                ToonLightData lightData = (ToonLightData)0;

                float3 directLight;
                float3 specularLight;
                GetMainLighting(inputData, lightData);

                #if _ADDITIONAL_LIGHTS
                    #if USE_CLUSTER_LIGHT_LOOP
                        UNITY_LOOP for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
                        {
                            GetAdditionalLighting(lightIndex, inputData, lightData);
                        }
                    #endif

                    uint pixelLightCount = GetAdditionalLightsCount();
                    LIGHT_LOOP_BEGIN(pixelLightCount)
                        GetAdditionalLighting(lightIndex, inputData, lightData);
                    LIGHT_LOOP_END
                #endif

                #if _RIM_LIGHTING_ON
                GetRimLight(inputData, lightData);
                #endif

                float3 lighting = lightData.directLight + lightData.specularLight;

                float3 diffuse = albedo.rgb * lighting;

                return float4(diffuse, albedo.a);
            }
            
            ENDHLSL
        }
        Pass
        {
	        Name "DepthOnly"
	        Tags { "LightMode" = "DepthOnly" }

	        ColorMask 0
	        ZWrite On
	        ZTest LEqual

	        HLSLPROGRAM
	        #pragma vertex DepthOnlyVertex
	        #pragma fragment DepthOnlyFragment

	        // Material Keywords
	        #pragma shader_feature _ALPHATEST_ON

	        // GPU Instancing
	        #pragma multi_compile_instancing

	        #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
	        ENDHLSL
        }

        Pass
        {
            Name "Shadow Caster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #pragma shader_feature_local_fragment _ALPHATEST_ON

            #pragma multi_compile_instancing

            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
    CustomEditor "Shears.Shaders.Editor.ToonShaderEditor"
}