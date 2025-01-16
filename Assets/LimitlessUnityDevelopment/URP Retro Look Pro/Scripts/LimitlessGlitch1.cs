using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
[VolumeComponentMenu("Retro Look Pro/Glitch1")]

public class LimitlessGlitch1 : VolumeComponent, IPostProcessComponent
{
    public BoolParameter enable = new BoolParameter(false);
    [Tooltip("Effect amount")]
    public ClampedFloatParameter Amount = new ClampedFloatParameter(0f, 0f, 2f,true);
    [Tooltip("Effect fade.")]
    public NoInterpClampedFloatParameter fade = new NoInterpClampedFloatParameter(1f, 0f, 1f);
    [Tooltip("Stretch on X axes")]
    public NoInterpClampedFloatParameter stretch = new NoInterpClampedFloatParameter(0.02f, 0f, 4f);
    [Tooltip("Effect speed.")]
    public NoInterpClampedFloatParameter speed = new NoInterpClampedFloatParameter(0.5f, 0f, 1f);
    [Space]
    [Header("Random Seed")]
    [Tooltip("seed x")]
    public NoInterpClampedFloatParameter x = new NoInterpClampedFloatParameter(127.1f, -2f, 200f);
    [Tooltip("seed y")]
    public NoInterpClampedFloatParameter y = new NoInterpClampedFloatParameter(43758.5453123f, -2f, 10002f);
    [Tooltip("seed z")]
    public NoInterpClampedFloatParameter z = new NoInterpClampedFloatParameter(311.7f, -2f, 200f);
    [Space]
    [Tooltip("Red.")]
    public NoInterpClampedFloatParameter rMultiplier = new NoInterpClampedFloatParameter(1f, -1f, 2f);
    [Tooltip("Green.")]
    public NoInterpClampedFloatParameter gMultiplier = new NoInterpClampedFloatParameter(1f, -1f, 2f);
    [Tooltip("Blue.")]
    public NoInterpClampedFloatParameter bMultiplier = new NoInterpClampedFloatParameter(0f, -1f, 2f);
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