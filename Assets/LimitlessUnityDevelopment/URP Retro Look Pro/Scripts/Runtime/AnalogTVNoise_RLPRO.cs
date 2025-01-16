using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using RetroLookPro.Enums;

public class AnalogTVNoise_RLPRO : ScriptableRendererFeature
{
    [SerializeField]
    private Shader m_Shader;
    private Material material;
    private AnalogTVNoise_RLPROPass AnalogTVNoiseRenderPass;
    public RenderPassEvent Event = RenderPassEvent.AfterRenderingPostProcessing;

    public override void Create()
    {
        m_Shader = Shader.Find("Hidden/Shader/AnalogTVNoiseEffect_RLPRO");
        if (m_Shader == null)
        {
            return;
        }
        material = new Material(m_Shader);
        AnalogTVNoiseRenderPass = new AnalogTVNoise_RLPROPass(material);

        AnalogTVNoiseRenderPass.renderPassEvent = Event;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        AnalogTVNoise myVolume = VolumeManager.instance.stack?.GetComponent<AnalogTVNoise>();
        if (myVolume == null || !myVolume.IsActive())
            return;
        if (!renderingData.cameraData.postProcessEnabled && myVolume.GlobalPostProcessingSettings.value) return;

        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            renderer.EnqueuePass(AnalogTVNoiseRenderPass);
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
    public class AnalogTVNoise_RLPROPass : ScriptableRenderPass
    {
        static readonly int TimeXV = Shader.PropertyToID("TimeX");
        static readonly int _PatternV = Shader.PropertyToID("_Pattern");
        static readonly int barHeightV = Shader.PropertyToID("barHeight");
        static readonly int barSpeedV = Shader.PropertyToID("barSpeed");
        static readonly int cutV = Shader.PropertyToID("cut");
        static readonly int edgeCutOffV = Shader.PropertyToID("edgeCutOff");
        static readonly int angleV = Shader.PropertyToID("angle");
        static readonly int tileXV = Shader.PropertyToID("tileX");
        static readonly int tileYV = Shader.PropertyToID("tileY");
        static readonly int horizontalV = Shader.PropertyToID("horizontal");
        static readonly int _OffsetNoiseXV = Shader.PropertyToID("_OffsetNoiseX");
        static readonly int _OffsetNoiseYV = Shader.PropertyToID("_OffsetNoiseY");
        static readonly int _FadeV = Shader.PropertyToID("_Fade");
        static readonly int _Mask = Shader.PropertyToID("_Mask");
        static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");

        private Material material;

        public AnalogTVNoise_RLPROPass(Material material)
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
        float TimeX;
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (material == null) return;

            // Use the Volume settings or the default settings if no Volume is set.
            var volumeComponent = VolumeManager.instance.stack.GetComponent<AnalogTVNoise>();

            TimeX += Time.deltaTime;
            if (TimeX > 100) TimeX = 0;

            material.SetFloat(TimeXV, TimeX);
            material.SetFloat(_FadeV, volumeComponent.fade.value);
            if (volumeComponent.texture.value != null)
                material.SetTexture(_PatternV, volumeComponent.texture.value);
            material.SetFloat(barHeightV, volumeComponent.barWidth.value);
            material.SetFloat(barSpeedV, volumeComponent.barSpeed.value);
            material.SetFloat(cutV, volumeComponent.CutOff.value);
            material.SetFloat(edgeCutOffV, volumeComponent.edgeCutOff.value);
            material.SetFloat(angleV, volumeComponent.textureAngle.value);
            material.SetFloat(tileXV, volumeComponent.tile.value.x);
            material.SetFloat(tileYV, volumeComponent.tile.value.y);
            material.SetFloat(horizontalV, volumeComponent.Horizontal.value ? 1 : 0);
            if (!volumeComponent.staticNoise.value)
            {
                material.SetFloat(_OffsetNoiseXV, UnityEngine.Random.Range(0f, 0.6f));
                material.SetFloat(_OffsetNoiseYV, UnityEngine.Random.Range(0f, 0.6f));
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

                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecuteMainPass(data, context));
            }
        }
    }
}


