using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[VolumeComponentMenu("Retro Look Pro/OldFilm2")]

public class OldFilm2 : VolumeComponent, IPostProcessComponent
{
    public BoolParameter enable = new BoolParameter(false);
    [Tooltip("Effect fade.")]
    public ClampedFloatParameter Fade = new ClampedFloatParameter(0f, 0f, 1f, true);
    public NoInterpClampedFloatParameter SepiaAmount = new NoInterpClampedFloatParameter(.8f, 0f, 1f);
    [Tooltip("Frames per second.")]
    public NoInterpClampedFloatParameter NoiseAmount = new NoInterpClampedFloatParameter(1f, 0f, 1);
    [Space]
    [Tooltip(".")]
    public NoInterpClampedFloatParameter ScratchAmount = new NoInterpClampedFloatParameter(1f, 0f, 1f);
    public NoInterpClampedFloatParameter speed = new NoInterpClampedFloatParameter(1f, 0f, 1f);
    [Tooltip("Image burn.")]
    public NoInterpClampedFloatParameter ScratchSize = new NoInterpClampedFloatParameter(0.88f, 0.0001f, 1f);
    public NoInterpClampedFloatParameter ScratchResolution = new NoInterpClampedFloatParameter(0.88f, 0.0001f, 1f);
    [Space]
    [Range(0f, 16f), Tooltip("Scene cut off.")]
    public NoInterpClampedFloatParameter Grain = new NoInterpClampedFloatParameter(1f, 0f, 1f);
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