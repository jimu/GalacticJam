using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using RetroLookPro.Enums;

public class Noise_RLPRO : ScriptableRendererFeature
{
    private Shader m_Shader;
    private Material material;
    private Noise_RLPROPass NoiseRenderPass;
    public RenderPassEvent Event = RenderPassEvent.AfterRenderingPostProcessing;

    public override void Create()
    {
        m_Shader = Shader.Find("Hidden/Shader/NoiseEffects_RLPRO");
        if (m_Shader == null)
        {
            return;
        }
        material = new Material(m_Shader);
        NoiseRenderPass = new Noise_RLPROPass(material);

        NoiseRenderPass.renderPassEvent = Event;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        Noise myVolume = VolumeManager.instance.stack?.GetComponent<Noise>();
        if (myVolume == null || !myVolume.IsActive())
            return;
        if (!renderingData.cameraData.postProcessEnabled && myVolume.GlobalPostProcessingSettings.value) return;

        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            renderer.EnqueuePass(NoiseRenderPass);
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
    public class Noise_RLPROPass : ScriptableRenderPass
    {
        static readonly int tapeLinesAmountV = Shader.PropertyToID("tapeLinesAmount");
        static readonly int time_V = Shader.PropertyToID("time_");
        static readonly int screenLinesNumV = Shader.PropertyToID("screenLinesNum");
        static readonly int noiseLinesNumV = Shader.PropertyToID("noiseLinesNum");
        static readonly int noiseQuantizeXV = Shader.PropertyToID("noiseQuantizeX");
        static readonly int signalNoisePowerV = Shader.PropertyToID("signalNoisePower");
        static readonly int signalNoiseAmountV = Shader.PropertyToID("signalNoiseAmount");
        static readonly int filmGrainAmountV = Shader.PropertyToID("filmGrainAmount");
        static readonly int tapeNoiseTHV = Shader.PropertyToID("tapeNoiseTH");
        static readonly int tapeNoiseAmountV = Shader.PropertyToID("tapeNoiseAmount");
        static readonly int tapeNoiseSpeedV = Shader.PropertyToID("tapeNoiseSpeed");
        static readonly int lineNoiseAmountV = Shader.PropertyToID("lineNoiseAmount");
        static readonly int lineNoiseSpeedV = Shader.PropertyToID("lineNoiseSpeed");
        static readonly int _TexV = Shader.PropertyToID("_TapeTex");
        static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");
        static readonly int _Mask = Shader.PropertyToID("_Mask");

        private float _time;

        private Material material;

        public Noise_RLPROPass(Material material)
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
        private static void ExecuteMainPass(RasterCommandBuffer cmd, RTHandle sourceTexture, Material material, int pass)
        {
            Blitter.BlitTexture(cmd, sourceTexture, new Vector4(1f, 1f, 0f, 0f), material, pass);
        }
        private static void ExecuteMainPass(PassData data, RasterGraphContext context, int pass)
        {
            ExecuteMainPass(context.cmd, data.src.IsValid() ? data.src : null, data.material, pass);
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
            internal Material material;
            public TextureHandle inputTexture;
        }
        float T;

        public class CustomData : ContextItem
        {
            public TextureHandle newTextureForFrameData;

            public override void Reset()
            {
                newTextureForFrameData = TextureHandle.nullHandle;
            }
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (material == null) return;

            var volumeComponent = VolumeManager.instance.stack.GetComponent<Noise>();

            UniversalResourceData resourcesData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            UniversalRenderer renderer = (UniversalRenderer)cameraData.renderer;
            var colorCopyDescriptor = GetCopyPassTextureDescriptor(cameraData.cameraTargetDescriptor);
            TextureHandle copiedColor = TextureHandle.nullHandle;
            copiedColor = UniversalRenderer.CreateRenderGraphTexture(renderGraph, colorCopyDescriptor, "_CustomPostPassColorCopy", false);

            //
            if (volumeComponent.unscaledTime.value) { _time = Time.unscaledTime; }
            else _time = Time.time;

            float screenLinesNum_ = volumeComponent.stretchResolution.value;
            if (screenLinesNum_ <= 0) screenLinesNum_ = Screen.height;


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
            material.SetFloat(tapeLinesAmountV, 1 - volumeComponent.tapeLinesAmount.value);
            material.SetFloat(time_V, _time);
            material.SetFloat("fade", volumeComponent.fade.value);
            material.SetFloat(screenLinesNumV, screenLinesNum_);
            material.SetFloat(noiseLinesNumV, volumeComponent.VerticalResolution.value);
            material.SetFloat(noiseQuantizeXV, volumeComponent.TapeNoiseSignalProcessing.value);
            ParamSwitch(material, volumeComponent.Granularity.value, "VHS_FILMGRAIN_ON");
            ParamSwitch(material, volumeComponent.TapeNoise.value, "VHS_TAPENOISE_ON");
            ParamSwitch(material, volumeComponent.LineNoise.value, "VHS_LINENOISE_ON");
            ParamSwitch(material, volumeComponent.SignalNoise.value, "VHS_YIQNOISE_ON");

            material.SetFloat(signalNoisePowerV, volumeComponent.SignalNoisePower.value);
            material.SetFloat(signalNoiseAmountV, volumeComponent.SignalNoiseAmount.value);
            material.SetFloat(filmGrainAmountV, volumeComponent.GranularityAmount.value);
            material.SetFloat(tapeNoiseTHV, volumeComponent.TapeNoiseAmount.value);
            material.SetFloat(tapeNoiseAmountV, volumeComponent.TapeNoiseFade.value);
            material.SetFloat(tapeNoiseSpeedV, volumeComponent.TapeNoiseSpeed.value);

            material.SetFloat(lineNoiseAmountV, volumeComponent.LineNoiseAmount.value);
            material.SetFloat(lineNoiseSpeedV, volumeComponent.LineNoiseSpeed.value);
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("NoisePass", out var passData, profilingSampler))
            {
                int texHeight = (int)Mathf.Min(volumeComponent.VerticalResolution.value, screenLinesNum_);
                int texWidth = (int)((float)texHeight * (float)Screen.width / (float)Screen.height);

                RenderTextureDescriptor textureProperties = new RenderTextureDescriptor(texWidth, texHeight, RenderTextureFormat.Default, 0);
                TextureHandle texture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, textureProperties, "_TapeTex", false);
                CustomData customData = frameData.Create<CustomData>();
                customData.newTextureForFrameData = texture;
                passData.src = texture;
                passData.material = material;
                builder.SetRenderAttachment(texture, 0, AccessFlags.Write);
                builder.SetGlobalTextureAfterPass(texture, _TexV);
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecuteMainPass(data, context, 1));
            }

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

                var customData = frameData.Get<Noise_RLPROPass.CustomData>();
                var customTexture = customData.newTextureForFrameData;
                builder.UseTexture(customTexture, AccessFlags.Read);

                builder.SetRenderAttachment(resourcesData.activeColorTexture, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecuteMainPass(data, context, 0));
            }
        }
    }
}