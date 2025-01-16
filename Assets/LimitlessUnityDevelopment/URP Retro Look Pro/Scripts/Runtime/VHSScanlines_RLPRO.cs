using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using RetroLookPro.Enums;
using UnityEditor;
using UnityEngine.Rendering.RenderGraphModule;

public class VHSScanlines_RLPRO : ScriptableRendererFeature
{
    [SerializeField]
    private Shader m_Shader;
    private Material material;
    private VHSScanlines_RLPROPass VHSScanlinesRenderPass;
    public RenderPassEvent Event = RenderPassEvent.AfterRenderingPostProcessing;

    public override void Create()
    {
        m_Shader = Shader.Find("Hidden/Shader/VHSScanlinesEffect_RLPRO");
        if (m_Shader == null)
        {
            return;
        }
        material = new Material(m_Shader);
        VHSScanlinesRenderPass = new VHSScanlines_RLPROPass(material);

        VHSScanlinesRenderPass.renderPassEvent = Event;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        VHSScanlines myVolume = VolumeManager.instance.stack?.GetComponent<VHSScanlines>();
        if (myVolume == null || !myVolume.IsActive())
            return;
        if (!renderingData.cameraData.postProcessEnabled && myVolume.GlobalPostProcessingSettings.value) return;

        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            renderer.EnqueuePass(VHSScanlinesRenderPass);
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
    public class VHSScanlines_RLPROPass : ScriptableRenderPass
	{
		static readonly int TimeV = Shader.PropertyToID("Time");

		static readonly int _ScanLinesV = Shader.PropertyToID("_ScanLines");
		static readonly int speedV = Shader.PropertyToID("speed");
		static readonly int fadeV = Shader.PropertyToID("fade");
		static readonly int _OffsetDistortionV = Shader.PropertyToID("_OffsetDistortion");
		static readonly int sfericalV = Shader.PropertyToID("sferical");
		static readonly int barrelV = Shader.PropertyToID("barrel");
		static readonly int scaleV = Shader.PropertyToID("scale");
		static readonly int _ScanLinesColorV = Shader.PropertyToID("_ScanLinesColor");
		static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");
		static readonly int _Mask = Shader.PropertyToID("_Mask");

        private Material material;

        public VHSScanlines_RLPROPass(Material material)
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

        private static void ExecuteMainPass(RasterCommandBuffer cmd, RTHandle sourceTexture, Material material,int pass)
        {
            Blitter.BlitTexture(cmd, sourceTexture, new Vector4(1f, 1f, 0f, 0f), material, pass);
        }
        private static void ExecuteMainPass(PassData data, RasterGraphContext context,int pass)
        {
            ExecuteMainPass(context.cmd, data.src.IsValid() ? data.src : null, data.material,pass);
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
        float T;

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            int pass = 0;
            if (material == null) return;

            // Use the Volume settings or the default settings if no Volume is set.
            var volumeComponent = VolumeManager.instance.stack.GetComponent<VHSScanlines>(); 
            T += Time.deltaTime;



			material.SetFloat(TimeV, T);
			material.SetFloat(_ScanLinesV, volumeComponent.scanLines.value);
			material.SetFloat(speedV, volumeComponent.speed.value);
			material.SetFloat(_OffsetDistortionV, volumeComponent.distortion.value);
			material.SetFloat(fadeV, volumeComponent.Fade.value);
			material.SetFloat(sfericalV, volumeComponent.distortion1.value);
			material.SetFloat(barrelV, volumeComponent.distortion2.value);
			material.SetFloat(scaleV, volumeComponent.scale.value);
			material.SetColor(_ScanLinesColorV, volumeComponent.scanLinesColor.value);
			if (volumeComponent.horizontal.value)
			{
				if (volumeComponent.distortion.value != 0)
					pass = 1;
				else
					pass = 0;
			}
			else
			{
				if (volumeComponent.distortion.value != 0)
					pass = 3;
				else
					pass = 2;
			}
			if (volumeComponent.mask.value != null)
			{
				material.SetTexture(_Mask, volumeComponent.mask.value);
				material.SetFloat(_FadeMultiplier, 1);
				ParamSwitch(material, volumeComponent.maskChannel.value == maskChannelMode.alphaChannel ? true : false, "ALPHA_CHANNEL");
			}
			else
			{
				material.SetFloat(_FadeMultiplier, 0);
			}
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

                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecuteMainPass(data, context,pass));
            }
        }
	}
}