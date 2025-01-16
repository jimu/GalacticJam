using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;
using RetroLookPro.Enums;


[Serializable]
public sealed class VignetteModeParameter : VolumeParameter<VignetteShape> { };
[VolumeComponentMenu("Retro Look Pro/Ultimate Vignette")]

public class UltimateVignette : VolumeComponent, IPostProcessComponent
{
    public BoolParameter enable = new BoolParameter(false);
	[Range(0f, 100), Tooltip(".")]
	public ClampedFloatParameter vignetteAmount = new ClampedFloatParameter(0, 0f, 100,true);
	public VignetteModeParameter vignetteShape = new VignetteModeParameter { };
	[Tooltip(".")]
	public NoInterpVector2Parameter center = new NoInterpVector2Parameter(new Vector2(0.5f, 0.5f));
	[Range(-1f, -100f), Tooltip(".")]
	public NoInterpClampedFloatParameter vignetteFineTune = new NoInterpClampedFloatParameter(-10f, -100f, -10f);
	[Range(0f, 100f), Tooltip("Scanlines width.")]
	public NoInterpClampedFloatParameter edgeSoftness = new NoInterpClampedFloatParameter(1.5f, 0f, 100f);
	[Range(200f, 0f), Tooltip("Horizontal/Vertical scanlines.")]
	public NoInterpClampedFloatParameter edgeBlend = new NoInterpClampedFloatParameter(0f, 0f, 200f);
	[Range(0f, 200f), Tooltip(".")]
	public NoInterpClampedFloatParameter innerColorAlpha = new NoInterpClampedFloatParameter(0f, 0f, 200f);
	public ColorParameter innerColor = new ColorParameter(new Color());
    [Space]
    [Tooltip("Use Global Post Processing Settings to enable or disable Post Processing in scene view or via camera setup. THIS SETTING SHOULD BE TURNED OFF FOR EFFECTS, IN CASE OF USING THEM FOR SEPARATE LAYERS")]
    public BoolParameter GlobalPostProcessingSettings = new BoolParameter(false);

    public bool IsActive() => (bool)enable;

    public bool IsTileCompatible() => false;
}