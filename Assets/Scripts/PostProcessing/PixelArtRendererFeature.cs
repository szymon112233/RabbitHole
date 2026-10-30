using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

/// <summary>
/// URP renderer feature that renders the camera through a low-resolution, colour-quantised
/// "pixel art" filter. All tuning lives in <see cref="PixelArtVolume"/> so it can be driven
/// by Volume profiles (globally or per area) and animated from code via the volume stack.
/// </summary>
public sealed class PixelArtRendererFeature : ScriptableRendererFeature
{
    private const string ShaderName = "Hidden/RabbitHole/PixelArtPostProcess";

    [Tooltip("When the effect runs. Before post-processing lets Bloom/Tonemapping glow over the pixels; after keeps them perfectly crisp.")]
    [SerializeField] private RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

    [Tooltip("Also apply the effect in the Scene view.")]
    [SerializeField] private bool applyInSceneView = true;

    [SerializeField, HideInInspector] private Shader shader;

    private Material material;
    private PixelArtPass pass;

    public override void Create()
    {
        if (shader == null)
            shader = Shader.Find(ShaderName);

        if (shader == null)
        {
            Debug.LogWarning($"[PixelArtRendererFeature] Shader '{ShaderName}' not found. Effect disabled.");
            return;
        }

        material = CoreUtils.CreateEngineMaterial(shader);
        pass = new PixelArtPass(material)
        {
            renderPassEvent = renderPassEvent,
            requiresIntermediateTexture = true
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (pass == null || material == null)
            return;

        var cameraType = renderingData.cameraData.cameraType;
        if (cameraType == CameraType.Preview || cameraType == CameraType.Reflection)
            return;
        if (cameraType == CameraType.SceneView && !applyInSceneView)
            return;

        var volume = VolumeManager.instance.stack.GetComponent<PixelArtVolume>();
        if (volume == null || !volume.IsActive())
            return;

        pass.renderPassEvent = renderPassEvent;
        renderer.EnqueuePass(pass);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(material);
        material = null;
        pass = null;
    }

    private sealed class PixelArtPass : ScriptableRenderPass
    {
        private static readonly int PixelGridSizeId = Shader.PropertyToID("_PixelGridSize");
        private static readonly int ParamsId = Shader.PropertyToID("_PixelArtParams");
        private static readonly int Params2Id = Shader.PropertyToID("_PixelArtParams2");

        private readonly Material material;

        public PixelArtPass(Material material)
        {
            this.material = material;
            profilingSampler = new ProfilingSampler("Pixel Art");
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var resourceData = frameData.Get<UniversalResourceData>();
            if (resourceData.isActiveTargetBackBuffer)
                return;

            var volume = VolumeManager.instance.stack.GetComponent<PixelArtVolume>();
            if (volume == null || !volume.IsActive())
                return;

            TextureHandle source = resourceData.activeColorTexture;
            TextureDesc desc = renderGraph.GetTextureDesc(source);

            int screenWidth = Mathf.Max(1, desc.width);
            int screenHeight = Mathf.Max(1, desc.height);

            int lowHeight, lowWidth;
            if (volume.targetHeight.value > 0)
            {
                lowHeight = Mathf.Clamp(volume.targetHeight.value, 1, screenHeight);
                lowWidth = Mathf.Max(1, Mathf.RoundToInt(screenWidth * (lowHeight / (float)screenHeight)));
            }
            else
            {
                int pixelSize = volume.GetScaledPixelSize(screenHeight);
                lowWidth = Mathf.Max(1, screenWidth / pixelSize);
                lowHeight = Mathf.Max(1, screenHeight / pixelSize);
            }

            material.SetVector(PixelGridSizeId,
                new Vector4(lowWidth, lowHeight, 1f / lowWidth, 1f / lowHeight));
            material.SetVector(ParamsId,
                new Vector4(volume.colorLevels.value, volume.dither.value, volume.scanlines.value, volume.saturation.value));
            material.SetVector(Params2Id,
                new Vector4(volume.contrast.value, volume.brightness.value, screenHeight, lowHeight));

            desc.width = lowWidth;
            desc.height = lowHeight;
            desc.filterMode = FilterMode.Point;
            desc.wrapMode = TextureWrapMode.Clamp;
            desc.msaaSamples = MSAASamples.None;
            desc.depthBufferBits = 0;
            desc.clearBuffer = false;
            desc.name = "_PixelArtLowRes";
            TextureHandle lowRes = renderGraph.CreateTexture(desc);

            renderGraph.AddBlitPass(
                new RenderGraphUtils.BlitMaterialParameters(source, lowRes, material, 0),
                "Pixel Art Downsample");
            renderGraph.AddBlitPass(
                new RenderGraphUtils.BlitMaterialParameters(lowRes, source, material, 1),
                "Pixel Art Compose");
        }
    }
}
