using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;


[VolumeComponentMenu("Retro Look Pro/Picture Correction")]

public class PictureCorrection : VolumeComponent, IPostProcessComponent
{
    public BoolParameter enable = new BoolParameter(false);
	public ClampedFloatParameter fade = new ClampedFloatParameter(0, 0, 1, true);
	[Range(-0.25f, 0.25f), Tooltip(" Y permanent adjustment..")]
	public NoInterpClampedFloatParameter signalAdjustY = new NoInterpClampedFloatParameter(0f, -0.25f, 0.25f);
	[Range(-0.25f, 0.25f), Tooltip("I permanent adjustment..")]
	public NoInterpClampedFloatParameter signalAdjustI = new NoInterpClampedFloatParameter(0f, -0.25f, 0.25f);
	[Range(-0.25f, 0.25f), Tooltip("Q permanent adjustment..")]
	public NoInterpClampedFloatParameter signalAdjustQ = new NoInterpClampedFloatParameter(0f, -0.25f, 0.25f);
	[Range(-2f, 2f), Tooltip("tweak/shift Y values..")]
	public NoInterpClampedFloatParameter signalShiftY = new NoInterpClampedFloatParameter(1f, -2f, 2f);
	[Range(-2f, 2f), Tooltip("tweak/shift I values..")]
	public NoInterpClampedFloatParameter signalShiftI = new NoInterpClampedFloatParameter(1f, -2f, 2f);
	[Range(-2f, 2f), Tooltip("tweak/shift Q values..")]
	public NoInterpClampedFloatParameter signalShiftQ = new NoInterpClampedFloatParameter(1f, -2f, 2f);
	[Range(0f, 2f), Tooltip("use this to balance the gamma(brightness) of the signal.")]
	public NoInterpClampedFloatParameter gammaCorection = new NoInterpClampedFloatParameter(1f, -0f, 2f);
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