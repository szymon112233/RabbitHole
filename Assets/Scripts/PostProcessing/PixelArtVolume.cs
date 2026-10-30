using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Volume override for the retro pixel-art look. Add it to any Volume profile
/// (the project-wide one lives in Assets/Settings/SampleSceneProfile.asset)
/// and tweak the values; the <see cref="PixelArtRendererFeature"/> reads them every frame.
/// </summary>
[Serializable, VolumeComponentMenu("Post-processing/Pixel Art (Retro)")]
[SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
public sealed class PixelArtVolume : VolumeComponent, IPostProcessComponent
{
    [Tooltip("Turn the whole effect on or off.")]
    public BoolParameter enabled = new BoolParameter(false, true);

    [Header("Resolution")]
    [Tooltip("Size of one virtual pixel measured in screen pixels at Reference Height. 1 = no pixelation at the reference resolution.")]
    public ClampedIntParameter pixelSize = new ClampedIntParameter(4, 1, 32);

    [Tooltip("Screen height (in pixels) at which Pixel Size applies 1:1. Higher resolutions scale Pixel Size up so the effect looks the same, e.g. 360 -> pixel size 1 becomes 3 at 1080p. 0 disables scaling.")]
    public ClampedIntParameter referenceHeight = new ClampedIntParameter(360, 0, 2160);

    [Tooltip("If set (> 0), overrides Pixel Size and renders at a fixed vertical resolution, e.g. 240 for a PS1/Genesis look.")]
    public ClampedIntParameter targetHeight = new ClampedIntParameter(0, 0, 1080);

    [Header("Colour")]
    [Tooltip("Number of shades per colour channel. 0 disables quantisation. 8 = NES-ish, 32 = SNES/Genesis-ish.")]
    public ClampedIntParameter colorLevels = new ClampedIntParameter(16, 0, 256);

    [Tooltip("Strength of 4x4 ordered (Bayer) dithering applied before quantisation.")]
    public ClampedFloatParameter dither = new ClampedFloatParameter(0.5f, 0f, 1f);

    [Tooltip("Colour saturation. 1 = unchanged.")]
    public ClampedFloatParameter saturation = new ClampedFloatParameter(1.1f, 0f, 2f);

    [Tooltip("Contrast. 1 = unchanged.")]
    public ClampedFloatParameter contrast = new ClampedFloatParameter(1.05f, 0.5f, 2f);

    [Tooltip("Brightness offset added after contrast.")]
    public ClampedFloatParameter brightness = new ClampedFloatParameter(0f, -0.5f, 0.5f);

    [Header("CRT")]
    [Tooltip("Darkening of every other virtual pixel row. 0 disables scanlines.")]
    public ClampedFloatParameter scanlines = new ClampedFloatParameter(0f, 0f, 1f);

    /// <summary>Pixel size in screen pixels for the given screen height, scaled relative to <see cref="referenceHeight"/>.</summary>
    public int GetScaledPixelSize(int screenHeight)
    {
        int size = Mathf.Max(1, pixelSize.value);
        if (referenceHeight.value <= 0 || screenHeight <= 0)
            return size;
        return Mathf.Max(1, Mathf.RoundToInt(size * (screenHeight / (float)referenceHeight.value)));
    }

    public bool IsActive()
    {
        if (!enabled.value)
            return false;

        bool pixelates = GetScaledPixelSize(Screen.height) > 1 || targetHeight.value > 0;
        bool grades = colorLevels.value > 0 || scanlines.value > 0f
                      || !Mathf.Approximately(saturation.value, 1f)
                      || !Mathf.Approximately(contrast.value, 1f)
                      || !Mathf.Approximately(brightness.value, 0f);
        return pixelates || grades;
    }
}
