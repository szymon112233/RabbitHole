#ifndef TOON_GRASS_COMMON_INCLUDED
#define TOON_GRASS_COMMON_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

struct GrassInstance
{
    float3 position;
    float3 normal;
    float height;
    float width;
    float rotation;
    float random;
};

StructuredBuffer<GrassInstance> _Instances;
float4x4 _GrassTransform;

CBUFFER_START(UnityPerMaterial)
    half4 _BaseColor;
    half4 _TipColor;
    half4 _VariationColor;
    half _VariationStrength;
    half _RootDarkening;
    half _LightSteps;
    half _LightSmoothness;
    half4 _ShadowColor;
    half _ShadowSharpness;
    half _AmbientStrength;
    half _NormalBlend;
    half4 _SpecularColor;
    half _SpecularSize;
    half _SpecularStrength;
    half4 _RimColor;
    half _RimSize;
    half _RimStrength;
    half _Bend;
    half _BendVariation;
    float4 _WindDirection;
    half _WindStrength;
    half _WindSpeed;
    half _WindScale;
    half _WindGustScale;
    half _WindGustStrength;
CBUFFER_END

struct GrassVertex
{
    float3 positionWS;
    float3 bladeNormalWS;
    float3 surfaceNormalWS;
    float2 uv;
    float random;
};

// Cheap layered sine wind: small ripples plus a slow gust front travelling along the wind direction.
float SampleWind(float2 xz, float random)
{
    float t = _Time.y * _WindSpeed;
    float2 p = xz * _WindScale;
    float ripple = sin(p.x * 1.0 + p.y * 0.7 + t) * 0.5
                 + sin(p.x * 2.3 - p.y * 1.1 + t * 1.3 + random * 6.2831) * 0.3
                 + sin(p.x * 0.4 + p.y * 1.9 + t * 0.7) * 0.2;
    float2 dir = normalize(_WindDirection.xy + float2(1e-5, 0));
    float gust = sin(dot(xz, dir) * _WindGustScale - t * 0.45) * 0.5 + 0.5;
    gust = gust * gust;
    return (ripple * 0.5 + 0.5) * lerp(1.0, gust, _WindGustStrength) + 0.15;
}

GrassVertex ComputeGrassVertex(float3 positionOS, uint instanceID)
{
    GrassInstance inst = _Instances[instanceID];

    float3 root = mul(_GrassTransform, float4(inst.position, 1.0)).xyz;
    float3 up = normalize(mul((float3x3)_GrassTransform, inst.normal));

    // Tangent frame on the surface, rotated per blade around the surface normal.
    float3 reference = abs(up.y) < 0.999 ? float3(0, 1, 0) : float3(1, 0, 0);
    float3 t1 = normalize(cross(up, reference));
    float3 t2 = cross(t1, up);
    float s, c;
    sincos(inst.rotation, s, c);
    float3 facing = c * t1 + s * t2;      // width axis
    float3 forward = cross(up, facing);   // bend axis

    float h = positionOS.y;               // 0 at root, 1 at tip
    float h2 = h * h;
    float height = inst.height;

    float wind = SampleWind(root.xz, inst.random) * _WindStrength;
    float3 windDir = normalize(float3(_WindDirection.x, 0, _WindDirection.y) + float3(1e-5, 0, 0));
    float bend = _Bend + (inst.random - 0.5) * _BendVariation;

    // Tip leans away; shorten the vertical part a little so bent blades do not stretch.
    float lean = abs(bend) + wind;
    float3 offset = up * (h * height * (1.0 - 0.3 * lean * h2))
                  + forward * (bend * h2 * height)
                  + windDir * (wind * h2 * height);

    float3 along = up * height + forward * (2.0 * bend * h * height) + windDir * (2.0 * wind * h * height);

    GrassVertex v;
    v.positionWS = root + offset + facing * (positionOS.x * inst.width);
    v.bladeNormalWS = normalize(cross(along, facing));
    v.surfaceNormalWS = up;
    v.uv = float2(positionOS.x + 0.5, h);
    v.random = inst.random;
    return v;
}

#endif
