using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using RetroLookPro.Enums;

public class Phosphor_RLPRO : ScriptableRendererFeature
{
    [SerializeField]
    private Shader m_Shader;
    private Material material;
    private Phosphor_RLPROPass PhosphorRenderPass;
    public RenderPassEvent Event = RenderPassEvent.AfterRenderingPostProcessing;

    public override void Create()
    {
        m_Shader = Shader.Find("Hidden/Shader/Phosphor_RLPRO");
        if (m_Shader == null)
        {
            return;
        }
        material = new Material(m_Shader);
        PhosphorRenderPass = new Phosphor_RLPROPass(material);

        PhosphorRenderPass.renderPassEvent = Event;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        Phosphor myVolume = VolumeManager.instance.stack?.GetComponent<Phosphor>();
        if (myVolume == null || !myVolume.IsActive())
            return;
        if (!renderingData.cameraData.postProcessEnabled && myVolume.GlobalPostProcessingSettings.value) return;

        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            renderer.EnqueuePass(PhosphorRenderPass);
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
    public class Phosphor_RLPROPass : ScriptableRenderPass
    {
        static readonly int TV = Shader.PropertyToID("T");
        static readonly int speedV = Shader.PropertyToID("speed");
        static readonly int fadeV = Shader.PropertyToID("fade");
        static readonly int _TexV = Shader.PropertyToID("_Tex");
        static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");
        static readonly int _Mask = Shader.PropertyToID("_Mask");
        private Material material;

        public Phosphor_RLPROPass(Material material)
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

            // Use the Volume settings or the default settings if no Volume is set.
            var volumeComponent = VolumeManager.instance.stack.GetComponent<Phosphor>();

            UniversalResourceData resourcesData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            UniversalRenderer renderer = (UniversalRenderer)cameraData.renderer;
            var colorCopyDescriptor = GetCopyPassTextureDescriptor(cameraData.cameraTargetDescriptor);
            TextureHandle copiedColor = TextureHandle.nullHandle;
            copiedColor = UniversalRenderer.CreateRenderGraphTexture(renderGraph, colorCopyDescriptor, "_CustomPostPassColorCopy", false);

            T = Time.time;

            material.SetFloat(TV, T);
            material.SetFloat(speedV, volumeComponent.width.value);
            material.SetFloat(fadeV, volumeComponent.Fade.value);
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


            using (var builder = renderGraph.AddRasterRenderPass<PassData>("NoisePass", out var passData, profilingSampler))
            {

                RenderTextureDescriptor textureProperties = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Default, 0);
                TextureHandle texture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, textureProperties, "_Tex", false);
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

                var customData = frameData.Get<Phosphor_RLPROPass.CustomData>();
                var customTexture = customData.newTextureForFrameData;
                builder.UseTexture(customTexture, AccessFlags.Read);

                builder.SetRenderAttachment(resourcesData.activeColorTexture, 0, AccessFlags.Write);
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecuteMainPass(data, context,0));
            }



        }
    }
}