using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[VolumeComponentMenu("Retro Look Pro/Negative")]

public class Negative : VolumeComponent, IPostProcessComponent
{
    public BoolParameter enable = new BoolParameter(false);
	public ClampedFloatParameter fade = new ClampedFloatParameter(0f, 0f, 1f,true);
	[Range(0f, 2f), Tooltip("Brightness.")]
	public NoInterpClampedFloatParameter luminosity = new NoInterpClampedFloatParameter(0f, 0f, 1.1f);
	[Range(0f, 1f), Tooltip("Vignette amount.")]
	public NoInterpClampedFloatParameter vignette = new NoInterpClampedFloatParameter(1f, 0f, 1f);
	[Range(0f, 1f), Tooltip("Contrast amount.")]
	public NoInterpClampedFloatParameter contrast = new NoInterpClampedFloatParameter(0.7f, 0f, 1f);
	[Range(0f, 1f), Tooltip("Negative amount.")]
	public NoInterpClampedFloatParameter negative = new NoInterpClampedFloatParameter(1f, 0f, 1f);
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