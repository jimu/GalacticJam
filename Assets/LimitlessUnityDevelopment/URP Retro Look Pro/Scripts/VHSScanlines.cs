using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[VolumeComponentMenu("Retro Look Pro/VHS Scanlines")]

public class VHSScanlines : VolumeComponent, IPostProcessComponent
{
    public BoolParameter enable = new BoolParameter(false);
    [Tooltip("Effect fade.")]
    public ClampedFloatParameter Fade = new ClampedFloatParameter(0f, 0f, 1f,true);
    [Tooltip("Lines color.")]
    public ColorParameter scanLinesColor = new ColorParameter(new Color());
    [Tooltip("Amount of scanlines.")]
    public NoInterpFloatParameter scanLines = new NoInterpFloatParameter(1.5f);
    [Tooltip("Lines speed.")]
    public NoInterpFloatParameter speed = new NoInterpFloatParameter(0);
    [Tooltip("Enable horizontal lines.")]
    public BoolParameter horizontal = new BoolParameter(true);
    [Tooltip("distortion.")]
    public NoInterpClampedFloatParameter distortion = new NoInterpClampedFloatParameter(0.2f, 0f, 0.5f);
    [Tooltip("distortion1.")]
    public NoInterpFloatParameter distortion1 = new NoInterpFloatParameter(0);
    [Tooltip("distortion2.")]
    public NoInterpFloatParameter distortion2 = new NoInterpFloatParameter(0);
    [Tooltip("Scale lines size.")]
    public NoInterpFloatParameter scale = new NoInterpFloatParameter(1);
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