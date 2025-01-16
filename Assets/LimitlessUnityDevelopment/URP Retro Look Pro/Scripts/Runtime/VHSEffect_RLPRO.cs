using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using RetroLookPro.Enums;
using UnityEditor;
using UnityEngine.Rendering.RenderGraphModule;

public class VHSEffect_RLPRO : ScriptableRendererFeature
{
    [SerializeField]
    private Shader m_Shader;
    private Material material;
    private VHSEffect_RLPROPass VHSEffectRenderPass;
    public RenderPassEvent Event = RenderPassEvent.AfterRenderingPostProcessing;

    public override void Create()
    {
        m_Shader = Shader.Find("Hidden/Shader/VHSEffect_RLPRO");
        if (m_Shader == null)
        {
            return;
        }
        material = new Material(m_Shader);
        VHSEffectRenderPass = new VHSEffect_RLPROPass(material);

        VHSEffectRenderPass.renderPassEvent = Event;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        VHSEffect myVolume = VolumeManager.instance.stack?.GetComponent<VHSEffect>();
        if (myVolume == null || !myVolume.IsActive())
            return;
        if (!renderingData.cameraData.postProcessEnabled && myVolume.GlobalPostProcessingSettings.value) return;

        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            renderer.EnqueuePass(VHSEffectRenderPass);
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
    public class VHSEffect_RLPROPass : ScriptableRenderPass
	{
		static readonly int TimeV = Shader.PropertyToID("Time");

		static readonly int _OffsetPosY = Shader.PropertyToID("_OffsetPosY");
		static readonly int smoothSize = Shader.PropertyToID("smoothSize");
		static readonly int _StandardDeviation = Shader.PropertyToID("_StandardDeviation");
		static readonly int iterations = Shader.PropertyToID("iterations");
		static readonly int tileX = Shader.PropertyToID("tileX");
		static readonly int smooth = Shader.PropertyToID("smooth1");
		static readonly int tileY = Shader.PropertyToID("tileY");
		static readonly int _OffsetDistortion = Shader.PropertyToID("_OffsetDistortion");
		static readonly int _Stripes = Shader.PropertyToID("_Stripes");

		static readonly int _OffsetColorAngle = Shader.PropertyToID("_OffsetColorAngle");
		static readonly int _OffsetColor = Shader.PropertyToID("_OffsetColor");
		static readonly int _OffsetNoiseX = Shader.PropertyToID("_OffsetNoiseX");
		static readonly int _SecondaryTex = Shader.PropertyToID("_SecondaryTex");
		static readonly int _OffsetNoiseY = Shader.PropertyToID("_OffsetNoiseY");
		static readonly int _TexIntensity = Shader.PropertyToID("_TexIntensity");
		static readonly int _TexCut = Shader.PropertyToID("_TexCut");
		static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");
		static readonly int _Mask = Shader.PropertyToID("_Mask");

        private Material material;

        public VHSEffect_RLPROPass(Material material)
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
        float T;
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (material == null) return;

            // Use the Volume settings or the default settings if no Volume is set.
            var volumeComponent = VolumeManager.instance.stack.GetComponent<VHSEffect>();
            
            if (!volumeComponent.unscaledTime.value)
				T += Time.deltaTime;
			else
				T += Time.unscaledDeltaTime;
			material.SetFloat(TimeV, T);
			if (UnityEngine.Random.Range(0, 100 - volumeComponent.verticalOffsetFrequency.value) <= 5)
			{
				if (volumeComponent.verticalOffset == 0.0f)
				{
					material.SetFloat(_OffsetPosY, volumeComponent.verticalOffset.value);
				}
				if (volumeComponent.verticalOffset.value > 0.0f)
				{
					material.SetFloat(_OffsetPosY, volumeComponent.verticalOffset.value - UnityEngine.Random.Range(0f, volumeComponent.verticalOffset.value));
				}
				else if (volumeComponent.verticalOffset.value < 0.0f)
				{
					material.SetFloat(_OffsetPosY, volumeComponent.verticalOffset.value + UnityEngine.Random.Range(0f, -volumeComponent.verticalOffset.value));
				}
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
			material.SetFloat(iterations, volumeComponent.iterations.value);
			material.SetFloat(smoothSize, volumeComponent.smoothSize.value);
			material.SetFloat(_StandardDeviation, volumeComponent.deviation.value);

			material.SetFloat(tileX, volumeComponent.tile.value.x);
			material.SetFloat(smooth, volumeComponent.smoothCut.value ? 1 : 0);
			material.SetFloat(tileY, volumeComponent.tile.value.y);
			material.SetFloat(_OffsetDistortion, volumeComponent.offsetDistortion.value);
			material.SetFloat(_Stripes, 0.51f - volumeComponent.stripes.value);

			material.SetVector(_OffsetColorAngle, new Vector2(Mathf.Sin(volumeComponent.colorOffsetAngle.value),
					Mathf.Cos(volumeComponent.colorOffsetAngle.value)));
			material.SetFloat(_OffsetColor, volumeComponent.colorOffset.value * 0.001f);

			material.SetFloat(_OffsetNoiseX, UnityEngine.Random.Range(-0.4f, 0.4f));
			if (volumeComponent.noiseTexture.value != null)
				material.SetTexture(_SecondaryTex, volumeComponent.noiseTexture.value);

			if (material.HasProperty(_OffsetNoiseY))
			{
				float offsetNoise = material.GetFloat(_OffsetNoiseY);
				material.SetFloat(_OffsetNoiseY, offsetNoise + UnityEngine.Random.Range(-0.03f, 0.03f));
			}
			material.SetFloat(_TexIntensity, volumeComponent._textureIntensity.value);
			material.SetFloat(_TexCut, volumeComponent._textureCutOff.value);
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