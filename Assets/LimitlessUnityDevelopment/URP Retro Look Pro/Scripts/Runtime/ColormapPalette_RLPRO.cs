using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using RetroLookPro.Enums;

public class ColormapPalette_RLPRO : ScriptableRendererFeature
{
    [SerializeField]
    private Shader m_Shader;
    private Material material;
    private ColormapPalette_RLPROPass ColormapPaletteRenderPass;
    public RenderPassEvent Event = RenderPassEvent.AfterRenderingPostProcessing;

    public override void Create()
    {
        m_Shader = Shader.Find("Hidden/Shader/ColormapPaletteEffect_RLPRO");
        if (m_Shader == null)
        {
            return;
        }
        material = new Material(m_Shader);
        ColormapPaletteRenderPass = new ColormapPalette_RLPROPass(material);

        ColormapPaletteRenderPass.renderPassEvent = Event;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        ColormapPalette myVolume = VolumeManager.instance.stack?.GetComponent<ColormapPalette>();
        if (myVolume == null || !myVolume.IsActive())
            return;
        if (!renderingData.cameraData.postProcessEnabled && myVolume.GlobalPostProcessingSettings.value) return;

        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            renderer.EnqueuePass(ColormapPaletteRenderPass);
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
    public class ColormapPalette_RLPROPass : ScriptableRenderPass
    {
        static readonly int heightV = Shader.PropertyToID("height");
        static readonly int widthV = Shader.PropertyToID("width");
        static readonly int _DitherV = Shader.PropertyToID("_Dither");
        static readonly int _OpacityV = Shader.PropertyToID("_Opacity");
        static readonly int _BlueNoiseV = Shader.PropertyToID("_BlueNoise");
        static readonly int _PaletteV = Shader.PropertyToID("_Palette");
        static readonly int _ColormapV = Shader.PropertyToID("_Colormap");
        static readonly int _FadeMultiplier = Shader.PropertyToID("_FadeMultiplier");
        static readonly int _Mask = Shader.PropertyToID("_Mask");
        public int tempPresetIndex = 0;
        private bool m_Init;
        Texture2D colormapPalette;
        Texture3D colormapTexture;
        private Vector2 m_Res;
        private int m_TempPixelSize;

        private Material material;

        public ColormapPalette_RLPROPass(Material material)
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
            var volumeComponent = VolumeManager.instance.stack.GetComponent<ColormapPalette>();

            ApplyMaterialVariables(material, out m_Res, volumeComponent);

            if (m_Init || intHasChanged(tempPresetIndex, volumeComponent.presetIndex.value) || m_TempPixelSize != volumeComponent.pixelSize.value)
            {
                tempPresetIndex = volumeComponent.presetIndex.value;
                ApplyColormapToMaterial(material, volumeComponent);
                m_Init = false;
                m_TempPixelSize = volumeComponent.pixelSize.value;
            }

            UniversalResourceData resourcesData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            float ratio = ((float)cameraData.camera.scaledPixelWidth) / (float)cameraData.camera.scaledPixelHeight;

            var w = cameraData.camera.scaledPixelWidth;
            var h = cameraData.camera.scaledPixelHeight;

            material.SetInt(heightV, (int)volumeComponent.pixelSize.value);
            material.SetInt(widthV, Mathf.RoundToInt((int)volumeComponent.pixelSize.value * ratio));

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

        public void ApplyMaterialVariables(Material bl, out Vector2 res, ColormapPalette volumeComponent)
        {

            res.x = Screen.width / volumeComponent.pixelSize.value;
            res.y = Screen.height / volumeComponent.pixelSize.value;

            volumeComponent.Opacity.value = Mathf.Clamp01(volumeComponent.Opacity.value);
            volumeComponent.dither.value = Mathf.Clamp01(volumeComponent.dither.value);

            bl.SetFloat(_DitherV, volumeComponent.dither.value);
            bl.SetFloat(_OpacityV, volumeComponent.Opacity.value);
        }
        public void ApplyColormapToMaterial(Material bl, ColormapPalette volumeComponent)
        {

            if (volumeComponent.presetsList.value != null)
            {
                if (volumeComponent.bluenoise.value != null)
                {
                    bl.SetTexture(_BlueNoiseV, volumeComponent.bluenoise.value);

                }
                ApplyPalette(bl, volumeComponent);
                ApplyMap(bl,volumeComponent);
            }
        }
        void ApplyPalette(Material bl, ColormapPalette volumeComponent)
        {
            colormapPalette = new Texture2D(256, 1, TextureFormat.RGB24, false);
            colormapPalette.filterMode = FilterMode.Point;
            colormapPalette.wrapMode = TextureWrapMode.Clamp;

            for (int i = 0; i < volumeComponent.presetsList.value.presetsList[volumeComponent.presetIndex.value].preset.numberOfColors; ++i)
            {
                colormapPalette.SetPixel(i, 0, volumeComponent.presetsList.value.presetsList[volumeComponent.presetIndex.value].preset.palette[i]);
            }

            colormapPalette.Apply();

            bl.SetTexture(_PaletteV, colormapPalette);
        }
        public void ApplyMap(Material bl, ColormapPalette volumeComponent)
        {
            int colorsteps = 64;
            colormapTexture = new Texture3D(colorsteps, colorsteps, colorsteps, TextureFormat.RGB24, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            colormapTexture.SetPixels32(volumeComponent.presetsList.value.presetsList[volumeComponent.presetIndex.value].preset.pixels);
            colormapTexture.Apply();
            bl.SetTexture(_ColormapV, colormapTexture);

        }
        public bool intHasChanged(int A, int B)
        {
            bool result = false;
            if (B != A)
            {
                A = B;
                result = true;
            }
            return result;
        }
    }

}


