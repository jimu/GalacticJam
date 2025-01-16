using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EdgeNoise_RLPRO : ScriptableRendererFeature
{
    [SerializeField]
    private Shader m_Shader;
    private Material material;
    private EdgeNoiseRenderPass edgeNoiseRenderPass;
    public RenderPassEvent Event = RenderPassEvent.AfterRenderingPostProcessing;

    public override void Create()
    {
        m_Shader = Shader.Find("Hidden/Shader/EdgeNoiseEffect_RLPRO");
        if (m_Shader == null)
        {
            return;
        }
        material = new Material(m_Shader);
        edgeNoiseRenderPass = new EdgeNoiseRenderPass(material);

        edgeNoiseRenderPass.renderPassEvent = Event;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        EdgeNoise myVolume = VolumeManager.instance.stack?.GetComponent<EdgeNoise>();
        if (myVolume == null || !myVolume.IsActive())
            return;
        if (!renderingData.cameraData.postProcessEnabled && myVolume.GlobalPostProcessingSettings.value) return;

        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            renderer.EnqueuePass(edgeNoiseRenderPass);
        }
    }

    protected override void Dispose(bool disposing)
    {
#if UNITY_EDITOR
        if (EditorApplication.isPlaying)
        {
            Destroy(material);
        }
        else
        {
            DestroyImmediate(material);
        }
#else
                Destroy(material);
#endif
    }
    public class EdgeNoiseRenderPass : ScriptableRenderPass
    {
        static readonly int _OffsetNoiseYV = Shader.PropertyToID("_OffsetNoiseY");
        static readonly int _OffsetNoiseXV = Shader.PropertyToID("_OffsetNoiseX");
        static readonly int _NoiseBottomHeightV = Shader.PropertyToID("_NoiseBottomHeight");
        static readonly int _NoiseBottomIntensityV = Shader.PropertyToID("_NoiseBottomIntensity");
        static readonly int _NoiseTextureV = Shader.PropertyToID("_NoiseTexture");
        static readonly int tileXV = Shader.PropertyToID("tileX");
        static readonly int tileYV = Shader.PropertyToID("tileY");

        private Material material;

        public EdgeNoiseRenderPass(Material material)
        {
            this.material = material;
        }
        private void ParamSwitch(Material mat, bool paramValue, string paramName)
        {
            if (paramValue) mat.EnableKeyword(paramName);
            else mat.DisableKeyword(paramName);
        }

        private static RenderTextureDescriptor GetCopyPassTextureDescriptor(RenderTextureDescriptor desc)
        {
            // Unless 'desc.bindMS = true' for an MSAA texture a resolve pass will be inserted before it is bound for sampling.
            // Since our main pass shader does not expect to sample an MSAA target we will leave 'bindMS = false'.
            // If the camera target has MSAA enabled an MSAA resolve will still happen before our copy-color pass but
            // with this change we will avoid an unnecessary MSAA resolve before our main pass.
            desc.msaaSamples = 1;

            // This avoids copying the depth buffer tied to the current descriptor as the main pass in this example does not use it
            desc.depthBufferBits = (int)DepthBits.None;

            return desc;
        }
        private static void ExecuteCopyColorPass(RasterCommandBuffer cmd, RTHandle sourceTexture)
        {
            Blitter.BlitTexture(cmd, sourceTexture, new Vector4(1, 1, 0, 0), 0.0f, false);
        }
        private static void ExecuteCopyColorPass(CopyPassData data, RasterGraphContext context)
        {
            ExecuteCopyColorPass(context.cmd, data.inputTexture);
        }

        private static void ExecuteMainPass(RasterCommandBuffer cmd, RTHandle sourceTexture, Material material)
        {
            Blitter.BlitTexture(cmd, sourceTexture, new Vector4(1f, 1f, 0f, 0f), material, 0);
        }
        private static void ExecuteMainPass(PassData data, RasterGraphContext context)
        {
            ExecuteMainPass(context.cmd, data.src.IsValid() ? data.src : null, data.material);
        }

        private class PassData
        {
            internal TextureHandle src;
            internal Material material;
        }
        private class CopyPassData
        {
            public TextureHandle inputTexture;
        }
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (material == null) return;

            // Use the Volume settings or the default settings if no Volume is set.
            var volumeComponent = VolumeManager.instance.stack.GetComponent<EdgeNoise>();
            if (material.HasProperty(_OffsetNoiseYV))
            {
                float offsetNoise1 = material.GetFloat(_OffsetNoiseYV);
                material.SetFloat(_OffsetNoiseYV, offsetNoise1 + UnityEngine.Random.Range(-0.05f, 0.05f));
            }
            material.SetFloat(_OffsetNoiseXV, UnityEngine.Random.Range(0f, 1.0f));

            material.SetFloat(_NoiseBottomHeightV, volumeComponent.height.value);
            material.SetFloat(_NoiseBottomIntensityV, volumeComponent.intensity.value);
            if (volumeComponent.noiseTexture.value != null)
            {
                material.SetTexture(_NoiseTextureV, volumeComponent.noiseTexture.value);
            }
            material.SetFloat(tileXV, volumeComponent.tile.value.x);
            material.SetFloat(tileYV, volumeComponent.tile.value.y);
            ParamSwitch(material, volumeComponent.top.value, "top_ON");
            ParamSwitch(material, volumeComponent.bottom.value, "bottom_ON");
            ParamSwitch(material, volumeComponent.left.value, "left_ON");
            ParamSwitch(material, volumeComponent.right.value, "right_ON");

            UniversalResourceData resourcesData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            UniversalRenderer renderer = (UniversalRenderer)cameraData.renderer;
            var colorCopyDescriptor = GetCopyPassTextureDescriptor(cameraData.cameraTargetDescriptor);
            TextureHandle copiedColor = TextureHandle.nullHandle;

            copiedColor = UniversalRenderer.CreateRenderGraphTexture(renderGraph, colorCopyDescriptor, "_CustomPostPassColorCopy", false);

            using (var builder = renderGraph.AddRasterRenderPass<CopyPassData>("CustomPostPass_CopyColor", out var passData, profilingSampler))
            {
                passData.inputTexture = resourcesData.activeColorTexture;
                builder.UseTexture(resourcesData.activeColorTexture, AccessFlags.Read);
                builder.SetRenderAttachment(copiedColor, 0, AccessFlags.Write);
                builder.SetRenderFunc((CopyPassData data, RasterGraphContext context) => ExecuteCopyColorPass(data, context));
            }

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("CustomPostPass", out var passData, profilingSampler))
            {
                passData.material = material;

                passData.src = copiedColor;
                builder.UseTexture(copiedColor, AccessFlags.Read);

                builder.SetRenderAttachment(resourcesData.activeColorTexture, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecuteMainPass(data, context));
            }
        }
    }
}


