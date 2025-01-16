using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[VolumeComponentMenu("Retro Look Pro/Jitter")]

public class Jitter : VolumeComponent, IPostProcessComponent
{
    public BoolParameter enable = new BoolParameter(false);
	public ClampedFloatParameter fade = new ClampedFloatParameter(0f, 0f, 1f,true);
	[Tooltip("Enable Twitch on X axes.")]
	public BoolParameter twitchHorizontal = new BoolParameter(false);
	[Range(0f, 5f), Tooltip("Twitch frequency on X axes.")]
	public NoInterpClampedFloatParameter horizontalFreq = new NoInterpClampedFloatParameter(1f, 0f, 5f);
	[Space]
	[Tooltip("Enable Twitch on Y axes.")]
	public BoolParameter twitchVertical = new BoolParameter(false);
	[Range(0f, 5f), Tooltip("Twitch frequency on Y axes.")]
	public NoInterpClampedFloatParameter verticalFreq = new NoInterpClampedFloatParameter(1f, 0f, 5f);
	[Space]
	[Tooltip("Enable Stretch.")]
	public BoolParameter stretch = new BoolParameter(false);
	[Tooltip("Stretch Resolution.")]
	public NoInterpFloatParameter stretchResolution = new NoInterpFloatParameter(1f);
	[Space]
	[Tooltip("Enable Horizontal Interlacing.")]
	public BoolParameter jitterHorizontal = new BoolParameter(false);
	[Range(0f, 5f), Tooltip("Amount of horizontal interlacing.")]
	public ClampedFloatParameter jitterHorizontalAmount = new ClampedFloatParameter(0f, 0f, 5f,true);
	[Space]
	[Tooltip("Shake Vertical.")]
	public BoolParameter jitterVertical = new BoolParameter(false);
	[Range(0f, 15f), Tooltip("Amount of shake.")]
	public ClampedFloatParameter jitterVerticalAmount = new ClampedFloatParameter(0f, 0f, 15f,true);
	[Range(0f, 15f), Tooltip("Speed of vertical shake. ")]
	public NoInterpClampedFloatParameter jitterVerticalSpeed = new NoInterpClampedFloatParameter(1f, 0f, 15f);
	[Space]
	[Tooltip("Time.unscaledTime .")]
	public BoolParameter unscaledTime = new BoolParameter(false);
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