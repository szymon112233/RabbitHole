Shader "Hidden/RabbitHole/PixelArtPostProcess"
{
    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

    // x = low-res width, y = low-res height, z = 1/width, w = 1/height
    float4 _PixelGridSize;
    // x = color levels (0 = off), y = dither strength, z = scanline strength, w = saturation
    float4 _PixelArtParams;
    // x = contrast, y = brightness, z = screen height in pixels, w = scanline count
    float4 _PixelArtParams2;

    // sampler_PointClamp comes from GlobalSamplers.hlsl via Blit.hlsl.

    // Pass 0: downsample the camera color to the pixel grid using nearest-neighbour sampling.
    half4 FragDownsample(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float2 uv = input.texcoord;
        // Snap to the centre of the low-res cell we are rendering so the source is point-sampled.
        float2 snapped = (floor(uv * _PixelGridSize.xy) + 0.5) * _PixelGridSize.zw;
        return SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, snapped, 0);
    }

    // 4x4 ordered Bayer matrix, values in [0,1) centred around 0.5.
    float Bayer4x4(float2 pixelPos)
    {
        int2 p = int2(fmod(pixelPos, 4.0));
        const float m[16] =
        {
             0.0,  8.0,  2.0, 10.0,
            12.0,  4.0, 14.0,  6.0,
             3.0, 11.0,  1.0,  9.0,
            15.0,  7.0, 13.0,  5.0
        };
        return (m[p.y * 4 + p.x] + 0.5) / 16.0;
    }

    // Pass 1: upscale the pixel grid back to screen, applying posterisation, dithering and scanlines.
    half4 FragCompose(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float2 uv = input.texcoord;
        half3 color = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, uv, 0).rgb;

        float levels = _PixelArtParams.x;
        float dither = _PixelArtParams.y;
        float scanlines = _PixelArtParams.z;
        float saturation = _PixelArtParams.w;
        float contrast = _PixelArtParams2.x;
        float brightness = _PixelArtParams2.y;

        // Contrast / brightness / saturation grading.
        color = (color - 0.5) * contrast + 0.5 + brightness;
        half luma = dot(color, half3(0.299, 0.587, 0.114));
        color = lerp(luma.xxx, color, saturation);
        color = saturate(color);

        // Colour quantisation with optional ordered dithering, computed in the low-res pixel grid.
        if (levels > 0.5)
        {
            float2 cell = floor(uv * _PixelGridSize.xy);
            float threshold = (Bayer4x4(cell) - 0.5) * dither / levels;
            color = floor((color + threshold) * levels + 0.5) / levels;
            color = saturate(color);
        }

        // Scanlines in screen pixels (one dark line per scanline row).
        if (scanlines > 0.0)
        {
            float row = uv.y * _PixelArtParams2.w;
            float lineMask = 0.5 + 0.5 * cos(row * TWO_PI);
            color *= 1.0 - scanlines * lineMask;
        }

        return half4(color, 1.0);
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off Blend Off

        Pass
        {
            Name "PixelArt Downsample"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragDownsample
            ENDHLSL
        }

        Pass
        {
            Name "PixelArt Compose"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragCompose
            ENDHLSL
        }
    }
    Fallback Off
}
