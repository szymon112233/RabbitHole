Shader "RabbitHole/Toon Grass (Indirect)"
{
    Properties
    {
        [Header(Colors)]
        _BaseColor ("Root Color", Color) = (0.16, 0.40, 0.12, 1)
        _TipColor ("Tip Color", Color) = (0.57, 0.86, 0.41, 1)
        _VariationColor ("Variation Color", Color) = (0.80, 0.78, 0.30, 1)
        _VariationStrength ("Variation Strength", Range(0, 1)) = 0.35
        _RootDarkening ("Root Darkening", Range(0, 1)) = 0.45

        [Header(Cel Shading)]
        _LightSteps ("Light Steps", Range(1, 8)) = 3
        _LightSmoothness ("Step Smoothness", Range(0, 0.5)) = 0.04
        _ShadowColor ("Shadow Tint", Color) = (0.42, 0.50, 0.62, 1)
        _ShadowSharpness ("Shadow Edge Sharpness", Range(0, 1)) = 0.85
        _AmbientStrength ("Ambient Strength", Range(0, 2)) = 1
        _NormalBlend ("Normal Blend (blade to ground)", Range(0, 1)) = 0.65
        _SpecularColor ("Specular Color", Color) = (1, 1, 0.9, 1)
        _SpecularSize ("Specular Size", Range(0, 1)) = 0.12
        _SpecularStrength ("Specular Strength", Range(0, 1)) = 0.35
        _RimColor ("Rim Color", Color) = (0.9, 1, 0.7, 1)
        _RimSize ("Rim Size", Range(0, 1)) = 0.3
        _RimStrength ("Rim Strength", Range(0, 1)) = 0.25

        [Header(Shape)]
        _Bend ("Bend", Range(-1, 1)) = 0.35
        _BendVariation ("Bend Variation", Range(0, 2)) = 0.6

        [Header(Wind)]
        _WindDirection ("Wind Direction (XZ)", Vector) = (1, 0.35, 0, 0)
        _WindStrength ("Wind Strength", Range(0, 1)) = 0.25
        _WindSpeed ("Wind Speed", Range(0, 10)) = 1.6
        _WindScale ("Wind Ripple Scale", Range(0.01, 3)) = 0.35
        _WindGustScale ("Wind Gust Scale", Range(0.001, 1)) = 0.07
        _WindGustStrength ("Wind Gust Strength", Range(0, 1)) = 0.7
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" "IgnoreProjector" = "True" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull Off
            ZWrite On

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"

            #include "ToonGrassCommon.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                uint instanceID : SV_InstanceID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 bladeNormalWS : TEXCOORD1;
                float3 surfaceNormalWS : TEXCOORD2;
                float3 uvRandom : TEXCOORD3;
                float fogFactor : TEXCOORD4;
            };

            Varyings Vert(Attributes input)
            {
                GrassVertex g = ComputeGrassVertex(input.positionOS, input.instanceID);
                Varyings o;
                o.positionWS = g.positionWS;
                o.positionCS = TransformWorldToHClip(g.positionWS);
                o.bladeNormalWS = g.bladeNormalWS;
                o.surfaceNormalWS = g.surfaceNormalWS;
                o.uvRandom = float3(g.uv, g.random);
                o.fogFactor = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            half ToonRamp(half value)
            {
                half q = value * _LightSteps;
                half i = floor(q);
                half f = q - i;
                half s = max(_LightSmoothness, 0.001);
                return saturate((i + smoothstep(0.5 - s, 0.5 + s, f)) / _LightSteps);
            }

            half4 Frag(Varyings input, FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC) : SV_Target
            {
                bool isFrontFace = IS_FRONT_VFACE(cullFace, true, false);
                float2 uv = input.uvRandom.xy;
                half random = input.uvRandom.z;

                float3 bladeN = normalize(input.bladeNormalWS);
                bladeN = isFrontFace ? bladeN : -bladeN;
                float3 N = normalize(lerp(bladeN, normalize(input.surfaceNormalWS), _NormalBlend));
                float3 V = GetWorldSpaceNormalizeViewDir(input.positionWS);

                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light light = GetMainLight(shadowCoord);

                half shadow = light.shadowAttenuation * light.distanceAttenuation;
                half edge = 0.5 * (1.0 - _ShadowSharpness) + 0.002;
                shadow = smoothstep(0.5 - edge, 0.5 + edge, shadow);

                half ndl = saturate(dot(N, light.direction));
                half lit = ToonRamp(ndl) * shadow;

                #if defined(_SCREEN_SPACE_OCCLUSION)
                    AmbientOcclusionFactor ao = GetScreenSpaceAmbientOcclusion(GetNormalizedScreenSpaceUV(input.positionCS));
                    lit *= ao.directAmbientOcclusion;
                #endif

                half3 albedo = lerp(_BaseColor.rgb, _TipColor.rgb, uv.y);
                albedo = lerp(albedo, albedo * _VariationColor.rgb * 1.6, random * _VariationStrength);
                albedo *= lerp(1.0 - _RootDarkening, 1.0, uv.y);

                half3 ambient = SampleSH(N) * _AmbientStrength;
                #if defined(_SCREEN_SPACE_OCCLUSION)
                    ambient *= ao.indirectAmbientOcclusion;
                #endif
                half3 diffuse = lerp(_ShadowColor.rgb, half3(1, 1, 1), lit) * light.color;
                half3 color = albedo * (diffuse + ambient);

                // Toon specular near the tips.
                float3 H = normalize(light.direction + V);
                half spec = smoothstep(1.0 - _SpecularSize - 0.02, 1.0 - _SpecularSize + 0.02, saturate(dot(N, H)));
                color += _SpecularColor.rgb * light.color * (spec * _SpecularStrength * lit * uv.y);

                // Rim light on the lit side only.
                half rim = 1.0 - saturate(dot(N, V));
                rim = smoothstep(1.0 - _RimSize - 0.05, 1.0 - _RimSize + 0.05, rim) * lit;
                color += _RimColor.rgb * (rim * _RimStrength);

                color = MixFog(color, input.fogFactor);
                return half4(color, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Off

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "ToonGrassCommon.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes
            {
                float3 positionOS : POSITION;
                uint instanceID : SV_InstanceID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings Vert(Attributes input)
            {
                GrassVertex g = ComputeGrassVertex(input.positionOS, input.instanceID);
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - g.positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif
                Varyings o;
                o.positionCS = TransformWorldToHClip(ApplyShadowBias(g.positionWS, g.surfaceNormalWS, lightDirectionWS));
                o.positionCS = ApplyShadowClamping(o.positionCS);
                return o;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R
            Cull Off

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag

            #include "ToonGrassCommon.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                uint instanceID : SV_InstanceID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings Vert(Attributes input)
            {
                GrassVertex g = ComputeGrassVertex(input.positionOS, input.instanceID);
                Varyings o;
                o.positionCS = TransformWorldToHClip(g.positionWS);
                return o;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On
            Cull Off

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag

            #include "ToonGrassCommon.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                uint instanceID : SV_InstanceID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                GrassVertex g = ComputeGrassVertex(input.positionOS, input.instanceID);
                Varyings o;
                o.positionCS = TransformWorldToHClip(g.positionWS);
                o.normalWS = normalize(lerp(g.bladeNormalWS, g.surfaceNormalWS, _NormalBlend));
                return o;
            }

            half4 Frag(Varyings input, FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC) : SV_Target
            {
                bool isFrontFace = IS_FRONT_VFACE(cullFace, true, false);
                float3 n = normalize(input.normalWS);
                return half4(isFrontFace ? n : -n, 0);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
