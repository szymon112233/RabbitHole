using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Dense, GPU-driven cel-shaded grass that grows on the colliders of this object (box, mesh, terrain...).
/// Blades are scattered once on the CPU (raycast against the surface, so any geometry works), then every
/// frame a compute shader frustum/distance culls them per camera and the survivors are drawn with a single
/// indirect draw call per LOD. Nothing is allocated per frame.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
[AddComponentMenu("Rendering/Grass Renderer (Toon)")]
public class GrassRenderer : MonoBehaviour
{
    [StructLayout(LayoutKind.Sequential)]
    struct GrassInstance
    {
        public Vector3 position;
        public Vector3 normal;
        public float height;
        public float width;
        public float rotation;
        public float random;
    }

    const int InstanceStride = 10 * sizeof(float);
    const int ThreadGroupSize = 64;

    [Header("Rendering")]
    [SerializeField] Material material;
    [SerializeField] ComputeShader cullShader;
    [Tooltip("Shadow casting is a big cost with hundreds of thousands of blades. Off is recommended; the toon shadow tint fakes it well.")]
    [SerializeField] ShadowCastingMode castShadows = ShadowCastingMode.Off;
    [SerializeField] bool receiveShadows = true;

    [Header("Placement")]
    [Tooltip("Blades per square meter of surface.")]
    [Min(0.1f)] [SerializeField] float density = 50f;
    [Tooltip("Hard cap on the number of blades (memory / safety). Density is reduced to fit.")]
    [Min(1)] [SerializeField] int maxInstances = 1_000_000;
    [SerializeField] int seed = 1234;
    [Tooltip("Surface steeper than this (degrees from world up) gets no grass.")]
    [Range(0f, 90f)] [SerializeField] float maxSlope = 50f;
    [Tooltip("Colliders to grow grass on. Leave empty to use every non-trigger collider on this object and its children. Works with Box/Mesh/Terrain colliders.")]
    [SerializeField] Collider[] surfaceColliders;
    [Tooltip("Optional mask stretched over the surface bounds (XZ). Red channel: 1 = full density, 0 = no grass. Texture must be Read/Write enabled.")]
    [SerializeField] Texture2D densityMask;
    [Tooltip("Noise frequency of bare patches (world units).")]
    [Min(0.001f)] [SerializeField] float patchNoiseScale = 0.12f;
    [Tooltip("How much of the field is bare patches. 0 = uniform cover.")]
    [Range(0f, 1f)] [SerializeField] float patchiness = 0.2f;

    [Header("Blade Shape")]
    [SerializeField] Vector2 bladeHeight = new Vector2(0.3f, 0.6f);
    [SerializeField] Vector2 bladeWidth = new Vector2(0.03f, 0.06f);
    [Tooltip("Noise frequency of tall/short clumps.")]
    [Min(0.001f)] [SerializeField] float clumpNoiseScale = 0.5f;
    [Tooltip("How strongly clumps vary height.")]
    [Range(0f, 1f)] [SerializeField] float clumpStrength = 0.5f;
    [Tooltip("Segments of the close-up blade mesh.")]
    [Range(1, 8)] [SerializeField] int bladeSegments = 4;
    [Tooltip("Segments of the far blade mesh. 1 = single triangle.")]
    [Range(1, 8)] [SerializeField] int bladeSegmentsFar = 1;

    [Header("Culling / LOD")]
    [Tooltip("Blades beyond this distance are never drawn.")]
    [Min(1f)] [SerializeField] float maxDrawDistance = 110f;
    [Tooltip("Blades start to thin out from here until none remain at Max Draw Distance.")]
    [Min(0f)] [SerializeField] float fadeStartDistance = 45f;
    [Tooltip("Surviving far blades get wider to hide the thinning.")]
    [Range(0f, 4f)] [SerializeField] float fadeWidthBoost = 1.5f;
    [Tooltip("Blades closer than this use the detailed mesh.")]
    [Min(0f)] [SerializeField] float lodDistance = 25f;

    /// <summary>Number of blades currently scattered (all cameras share this set).</summary>
    public int BladeCount { get; private set; }

    ComputeBuffer sourceBuffer;
    ComputeBuffer visibleNear;
    ComputeBuffer visibleFar;
    GraphicsBuffer argsNear;
    GraphicsBuffer argsFar;
    Mesh meshNear;
    Mesh meshFar;
    MaterialPropertyBlock propsNear;
    MaterialPropertyBlock propsFar;
    Matrix4x4 scatterWorldToLocal = Matrix4x4.identity;
    Bounds scatterBounds;
    float maxBladeHeight;
    int cullKernel = -1;
    bool dirty = true;
    bool warnedMissingAssets;
    readonly Plane[] frustumPlanes = new Plane[6];
    readonly Vector4[] frustumVectors = new Vector4[6];

    static readonly int InstancesId = Shader.PropertyToID("_Instances");
    static readonly int GrassTransformId = Shader.PropertyToID("_GrassTransform");
    static readonly int SourceId = Shader.PropertyToID("_Source");
    static readonly int VisibleNearId = Shader.PropertyToID("_VisibleNear");
    static readonly int VisibleFarId = Shader.PropertyToID("_VisibleFar");
    static readonly int CountId = Shader.PropertyToID("_Count");
    static readonly int FrustumPlanesId = Shader.PropertyToID("_FrustumPlanes");
    static readonly int CameraPositionId = Shader.PropertyToID("_CameraPosition");
    static readonly int MaxDistanceId = Shader.PropertyToID("_MaxDistance");
    static readonly int FadeStartId = Shader.PropertyToID("_FadeStart");
    static readonly int FadeWidthBoostId = Shader.PropertyToID("_FadeWidthBoost");
    static readonly int LodDistanceId = Shader.PropertyToID("_LodDistance");
    static readonly int BoundsPaddingId = Shader.PropertyToID("_BoundsPadding");

    void OnEnable()
    {
        dirty = true;
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        ReleaseBuffers();
    }

    void OnValidate()
    {
        bladeHeight.x = Mathf.Max(0.01f, bladeHeight.x);
        bladeHeight.y = Mathf.Max(bladeHeight.x, bladeHeight.y);
        bladeWidth.x = Mathf.Max(0.001f, bladeWidth.x);
        bladeWidth.y = Mathf.Max(bladeWidth.x, bladeWidth.y);
        fadeStartDistance = Mathf.Min(fadeStartDistance, maxDrawDistance);
        dirty = true;
    }

    /// <summary>Re-scatters the blades on the surface. Call after changing the geometry at runtime.</summary>
    [ContextMenu("Rebuild Grass")]
    public void Rebuild()
    {
        dirty = true;
    }

    void OnBeginCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        if (cam.cameraType == CameraType.Preview || cam.cameraType == CameraType.Reflection)
            return;

        if (material == null || cullShader == null)
        {
            if (!warnedMissingAssets)
            {
                warnedMissingAssets = true;
                Debug.LogWarning($"{nameof(GrassRenderer)} on '{name}' needs a Material and the GrassCull compute shader.", this);
            }
            return;
        }

        if (dirty)
        {
            dirty = false;
            Scatter();
        }

        if (BladeCount == 0)
            return;

        // Keeps the grass glued to the object if it is moved/rotated after scattering. Rigid changes are exact.
        Matrix4x4 delta = transform.localToWorldMatrix * scatterWorldToLocal;

        Cull(cam, delta);
        Draw(cam, delta);
    }

    void Cull(Camera cam, Matrix4x4 delta)
    {
        GeometryUtility.CalculateFrustumPlanes(cam, frustumPlanes);
        for (int i = 0; i < 6; i++)
        {
            Vector3 n = frustumPlanes[i].normal;
            frustumVectors[i] = new Vector4(n.x, n.y, n.z, frustumPlanes[i].distance);
        }

        visibleNear.SetCounterValue(0);
        visibleFar.SetCounterValue(0);

        cullShader.SetBuffer(cullKernel, SourceId, sourceBuffer);
        cullShader.SetBuffer(cullKernel, VisibleNearId, visibleNear);
        cullShader.SetBuffer(cullKernel, VisibleFarId, visibleFar);
        cullShader.SetInt(CountId, BladeCount);
        cullShader.SetVectorArray(FrustumPlanesId, frustumVectors);
        cullShader.SetVector(CameraPositionId, cam.transform.position);
        cullShader.SetMatrix(GrassTransformId, delta);
        cullShader.SetFloat(MaxDistanceId, maxDrawDistance);
        cullShader.SetFloat(FadeStartId, fadeStartDistance);
        cullShader.SetFloat(FadeWidthBoostId, fadeWidthBoost);
        cullShader.SetFloat(LodDistanceId, lodDistance);
        cullShader.SetFloat(BoundsPaddingId, maxBladeHeight);

        int groups = (BladeCount + ThreadGroupSize - 1) / ThreadGroupSize;
        cullShader.Dispatch(cullKernel, groups, 1, 1);

        // instanceCount lives at byte offset 4 in IndirectDrawIndexedArgs.
        GraphicsBuffer.CopyCount(visibleNear, argsNear, sizeof(uint));
        GraphicsBuffer.CopyCount(visibleFar, argsFar, sizeof(uint));
    }

    void Draw(Camera cam, Matrix4x4 delta)
    {
        Bounds bounds = TransformBounds(scatterBounds, delta);
        bounds.Expand(maxBladeHeight * 2f + 1f);

        propsNear.SetMatrix(GrassTransformId, delta);
        propsFar.SetMatrix(GrassTransformId, delta);

        var rp = new RenderParams(material)
        {
            camera = cam,
            layer = gameObject.layer,
            worldBounds = bounds,
            shadowCastingMode = castShadows,
            receiveShadows = receiveShadows,
            lightProbeUsage = LightProbeUsage.Off,
            reflectionProbeUsage = ReflectionProbeUsage.Off,
            motionVectorMode = MotionVectorGenerationMode.Camera,
        };

        rp.matProps = propsNear;
        Graphics.RenderMeshIndirect(rp, meshNear, argsNear);
        rp.matProps = propsFar;
        Graphics.RenderMeshIndirect(rp, meshFar, argsFar);
    }

    // ---------------------------------------------------------------------------------------------
    // Scattering
    // ---------------------------------------------------------------------------------------------

    void Scatter()
    {
        ReleaseBuffers();
        BladeCount = 0;

        List<Collider> colliders = CollectColliders();
        if (colliders.Count == 0)
        {
            Debug.LogWarning($"{nameof(GrassRenderer)} on '{name}' found no collider to grow on.", this);
            return;
        }

        Physics.SyncTransforms();

        Bounds bounds = colliders[0].bounds;
        for (int i = 1; i < colliders.Count; i++) bounds.Encapsulate(colliders[i].bounds);

        float area = Mathf.Max(bounds.size.x * bounds.size.z, 0.01f);
        float effectiveDensity = Mathf.Min(density, maxInstances / area);
        if (effectiveDensity < density)
            Debug.LogWarning($"{nameof(GrassRenderer)} on '{name}': density reduced to {effectiveDensity:F1}/m² to respect Max Instances ({maxInstances}).", this);

        float spacing = 1f / Mathf.Sqrt(effectiveDensity);
        int cellsX = Mathf.Max(1, Mathf.CeilToInt(bounds.size.x / spacing));
        int cellsZ = Mathf.Max(1, Mathf.CeilToInt(bounds.size.z / spacing));

        var rng = new System.Random(seed);
        float noiseOffsetX = (float)rng.NextDouble() * 1000f;
        float noiseOffsetZ = (float)rng.NextDouble() * 1000f;
        bool useMask = densityMask != null && densityMask.isReadable;
        if (densityMask != null && !densityMask.isReadable)
            Debug.LogWarning($"{nameof(GrassRenderer)}: density mask '{densityMask.name}' is not Read/Write enabled and will be ignored.", this);

        float rayStartY = bounds.max.y + 1f;
        float rayLength = bounds.size.y + 2f;
        float cosMaxSlope = Mathf.Cos(maxSlope * Mathf.Deg2Rad);

        var instances = new List<GrassInstance>(Mathf.Min(cellsX * cellsZ, maxInstances));
        Bounds instanceBounds = new Bounds();
        bool hasBounds = false;
        maxBladeHeight = 0f;

        for (int iz = 0; iz < cellsZ && instances.Count < maxInstances; iz++)
        {
            for (int ix = 0; ix < cellsX && instances.Count < maxInstances; ix++)
            {
                float x = bounds.min.x + (ix + (float)rng.NextDouble()) * spacing;
                float z = bounds.min.z + (iz + (float)rng.NextDouble()) * spacing;

                if (patchiness > 0f)
                {
                    float patch = Mathf.PerlinNoise(x * patchNoiseScale + noiseOffsetX, z * patchNoiseScale + noiseOffsetZ);
                    // Perlin mostly lives in 0.25..0.75; remap so patchiness 1 clears almost everything.
                    if (patch < Mathf.Lerp(0.2f, 0.8f, patchiness)) continue;
                }

                if (useMask)
                {
                    float u = (x - bounds.min.x) / Mathf.Max(bounds.size.x, 1e-4f);
                    float v = (z - bounds.min.z) / Mathf.Max(bounds.size.z, 1e-4f);
                    if (rng.NextDouble() > densityMask.GetPixelBilinear(u, v).r) continue;
                }

                if (!RaycastSurface(colliders, new Ray(new Vector3(x, rayStartY, z), Vector3.down), rayLength, out RaycastHit hit))
                    continue;
                if (Vector3.Dot(hit.normal, Vector3.up) < cosMaxSlope)
                    continue;

                float clump = Mathf.PerlinNoise(x * clumpNoiseScale + noiseOffsetZ, z * clumpNoiseScale + noiseOffsetX);
                float heightMul = Mathf.Lerp(1f, Mathf.Lerp(0.5f, 1.5f, clump), clumpStrength);
                float height = Mathf.Lerp(bladeHeight.x, bladeHeight.y, (float)rng.NextDouble()) * heightMul;
                float width = Mathf.Lerp(bladeWidth.x, bladeWidth.y, (float)rng.NextDouble());

                instances.Add(new GrassInstance
                {
                    position = hit.point,
                    normal = hit.normal,
                    height = height,
                    width = width,
                    rotation = (float)rng.NextDouble() * Mathf.PI * 2f,
                    random = (float)rng.NextDouble(),
                });

                maxBladeHeight = Mathf.Max(maxBladeHeight, height);
                if (!hasBounds) { instanceBounds = new Bounds(hit.point, Vector3.zero); hasBounds = true; }
                else instanceBounds.Encapsulate(hit.point);
            }
        }

        BladeCount = instances.Count;
        if (BladeCount == 0)
            return;

        scatterBounds = instanceBounds;
        scatterWorldToLocal = transform.worldToLocalMatrix;
        CreateBuffers(instances);
    }

    List<Collider> CollectColliders()
    {
        var result = new List<Collider>();
        if (surfaceColliders != null && surfaceColliders.Length > 0)
        {
            foreach (Collider c in surfaceColliders)
                if (c != null && c.enabled) result.Add(c);
        }
        else
        {
            foreach (Collider c in GetComponentsInChildren<Collider>())
                if (c.enabled && !c.isTrigger) result.Add(c);
        }
        return result;
    }

    static bool RaycastSurface(List<Collider> colliders, Ray ray, float maxDistance, out RaycastHit best)
    {
        best = default;
        bool found = false;
        foreach (Collider c in colliders)
        {
            // Collider.Raycast only tests this collider, so rabbits/props standing on the ground never get in the way.
            if (c.Raycast(ray, out RaycastHit hit, maxDistance) && (!found || hit.distance < best.distance))
            {
                best = hit;
                found = true;
            }
        }
        return found;
    }

    // ---------------------------------------------------------------------------------------------
    // GPU resources
    // ---------------------------------------------------------------------------------------------

    void CreateBuffers(List<GrassInstance> instances)
    {
        int count = instances.Count;
        sourceBuffer = new ComputeBuffer(count, InstanceStride, ComputeBufferType.Structured);
        sourceBuffer.SetData(instances);
        visibleNear = new ComputeBuffer(count, InstanceStride, ComputeBufferType.Append);
        visibleFar = new ComputeBuffer(count, InstanceStride, ComputeBufferType.Append);

        meshNear = BuildBladeMesh(bladeSegments);
        meshFar = BuildBladeMesh(bladeSegmentsFar);
        argsNear = CreateArgsBuffer(meshNear);
        argsFar = CreateArgsBuffer(meshFar);

        propsNear = new MaterialPropertyBlock();
        propsNear.SetBuffer(InstancesId, visibleNear);
        propsFar = new MaterialPropertyBlock();
        propsFar.SetBuffer(InstancesId, visibleFar);

        cullKernel = cullShader.FindKernel("CullGrass");
    }

    static GraphicsBuffer CreateArgsBuffer(Mesh mesh)
    {
        var buffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        buffer.SetData(new[]
        {
            new GraphicsBuffer.IndirectDrawIndexedArgs
            {
                indexCountPerInstance = mesh.GetIndexCount(0),
                instanceCount = 0,
                startIndex = 0,
                baseVertexIndex = 0,
                startInstance = 0,
            }
        });
        return buffer;
    }

    void ReleaseBuffers()
    {
        sourceBuffer?.Release(); sourceBuffer = null;
        visibleNear?.Release(); visibleNear = null;
        visibleFar?.Release(); visibleFar = null;
        argsNear?.Release(); argsNear = null;
        argsFar?.Release(); argsFar = null;
        DestroyMesh(ref meshNear);
        DestroyMesh(ref meshFar);
    }

    static void DestroyMesh(ref Mesh mesh)
    {
        if (mesh == null) return;
        if (Application.isPlaying) Destroy(mesh); else DestroyImmediate(mesh);
        mesh = null;
    }

    /// <summary>
    /// Unit blade in the XY plane: x = side (-0.5..0.5, tapering towards the tip), y = 0..1 along the blade.
    /// The shader scales/bends it per instance.
    /// </summary>
    static Mesh BuildBladeMesh(int segments)
    {
        segments = Mathf.Max(1, segments);
        int vertexCount = segments * 2 + 1;
        var vertices = new Vector3[vertexCount];
        var uvs = new Vector2[vertexCount];
        var normals = new Vector3[vertexCount];
        var indices = new int[(segments * 2 - 1) * 3];

        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / segments;
            float halfWidth = 0.5f * (1f - t * t);
            vertices[i * 2] = new Vector3(-halfWidth, t, 0f);
            vertices[i * 2 + 1] = new Vector3(halfWidth, t, 0f);
            uvs[i * 2] = new Vector2(0f, t);
            uvs[i * 2 + 1] = new Vector2(1f, t);
        }
        int tip = vertexCount - 1;
        vertices[tip] = new Vector3(0f, 1f, 0f);
        uvs[tip] = new Vector2(0.5f, 1f);
        for (int i = 0; i < vertexCount; i++) normals[i] = Vector3.forward;

        int idx = 0;
        for (int i = 0; i < segments - 1; i++)
        {
            int a = i * 2, b = a + 1, c = a + 2, d = a + 3;
            indices[idx++] = a; indices[idx++] = c; indices[idx++] = b;
            indices[idx++] = b; indices[idx++] = c; indices[idx++] = d;
        }
        int last = (segments - 1) * 2;
        indices[idx++] = last; indices[idx++] = tip; indices[idx++] = last + 1;

        var mesh = new Mesh { name = $"GrassBlade_{segments}" };
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.normals = normals;
        mesh.SetIndices(indices, MeshTopology.Triangles, 0, false);
        mesh.bounds = new Bounds(new Vector3(0f, 0.5f, 0f), new Vector3(1f, 1f, 1f));
        mesh.UploadMeshData(true);
        return mesh;
    }

    static Bounds TransformBounds(Bounds b, Matrix4x4 m)
    {
        Vector3 center = m.MultiplyPoint3x4(b.center);
        Vector3 ext = b.extents;
        Vector3 newExt = new Vector3(
            Mathf.Abs(m.m00) * ext.x + Mathf.Abs(m.m01) * ext.y + Mathf.Abs(m.m02) * ext.z,
            Mathf.Abs(m.m10) * ext.x + Mathf.Abs(m.m11) * ext.y + Mathf.Abs(m.m12) * ext.z,
            Mathf.Abs(m.m20) * ext.x + Mathf.Abs(m.m21) * ext.y + Mathf.Abs(m.m22) * ext.z);
        return new Bounds(center, newExt * 2f);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (BladeCount == 0) return;
        Gizmos.color = new Color(0.3f, 1f, 0.3f, 0.5f);
        Bounds b = TransformBounds(scatterBounds, transform.localToWorldMatrix * scatterWorldToLocal);
        b.Expand(maxBladeHeight * 2f);
        Gizmos.DrawWireCube(b.center, b.size);
    }
#endif
}
