using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[VolumeComponentMenu("Retro Look Pro/CRT Aperture")]

public class CRTAperture: VolumeComponent, IPostProcessComponent
{
    public BoolParameter enable = new BoolParameter(false);
	public ClampedFloatParameter fade = new ClampedFloatParameter(0, 0, 1, true);
	[Tooltip("Glow Halation.")]
	public NoInterpClampedFloatParameter GlowHalation = new NoInterpClampedFloatParameter(4.27f, 0f, 5f);
	[Tooltip("Glow Difusion.")]
	public NoInterpClampedFloatParameter GlowDifusion = new NoInterpClampedFloatParameter(0.83f, 0f, 2f);
	[Tooltip("Mask Colors.")]
	public NoInterpClampedFloatParameter MaskColors = new NoInterpClampedFloatParameter(0.57f, 0f, 5f);
	[Tooltip("Mask Strength.")]
	public NoInterpClampedFloatParameter MaskStrength = new NoInterpClampedFloatParameter(0.318f, 0f, 1f);
	[Tooltip("Gamma Input.")]
	public NoInterpClampedFloatParameter GammaInput = new NoInterpClampedFloatParameter(1.12f, 0f, 5f);
	[Tooltip("Gamma Output.")]
	public NoInterpClampedFloatParameter GammaOutput = new NoInterpClampedFloatParameter(0.89f, 0f, 5f);
	[Tooltip("Brightness.")]
	public NoInterpClampedFloatParameter Brightness = new NoInterpClampedFloatParameter(0.85f, 0f, 2.5f);
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