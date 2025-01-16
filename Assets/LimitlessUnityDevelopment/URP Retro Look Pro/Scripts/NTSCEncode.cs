using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[VolumeComponentMenu("Retro Look Pro/NTSC Encode")]

public class NTSCEncode : VolumeComponent, IPostProcessComponent
{
    public BoolParameter enable = new BoolParameter(false);
    public ClampedFloatParameter fade = new ClampedFloatParameter(0, 0, 1, true);
    [Tooltip("Blur size.")]
    public NoInterpClampedFloatParameter blur = new NoInterpClampedFloatParameter(0.83f, 0.01f, 2f);
    [Tooltip("Brigtness.")]
    public NoInterpClampedFloatParameter brigtness = new NoInterpClampedFloatParameter(3f, 1f, 40f);
    [Tooltip("Floating lines speed")]
    public NoInterpClampedFloatParameter lineSpeed = new NoInterpClampedFloatParameter(0.01f, 0f, 10f);
    [Space]
    [Tooltip("Mask texture")]
    public TextureParameter mask = new TextureParameter(null);
    public maskChannelModeParameter maskChannel = new maskChannelModeParameter();
    [Space]
    [Tooltip("Use Global Post Processing Settings to enable or disable Post Processing in scene view or via camera setup. THIS SETTING SHOULD BE TURNED OFF FOR EFFECTS, IN CASE OF USING THEM FOR SEPARATE LAYERS")]
    public BoolParameter GlobalPostProcessingSettings = new BoolParameter(false);


    public bool IsActive() => (bool)enable;

    public bool IsTileCompatible() => false;
}