using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public enum noiseParam {
    blackWhite,color
}
[Serializable]
public sealed class NoiseParameter : VolumeParameter<noiseParam> { };
[VolumeComponentMenu("Retro Look Pro/Noise 2")]
public class Noise2 : VolumeComponent, IPostProcessComponent
{
    public BoolParameter enable = new BoolParameter(false);
	[Range(0f, 1f), Tooltip("Dark areas adjustment.")]
	public ClampedFloatParameter fade = new ClampedFloatParameter(0f, 0f, 1f,true);
    [Header("Tape Noise Settings")]
    public NoInterpClampedFloatParameter waveAmount = new NoInterpClampedFloatParameter(1f, 0f, 10f);
    public NoInterpClampedFloatParameter tapeIntensity = new NoInterpClampedFloatParameter(1f, 0f, 1f);
    public NoInterpClampedFloatParameter tapeLinesAmount = new NoInterpClampedFloatParameter(1f, 0f, 10f);    
    public NoInterpClampedFloatParameter tapeSpeed = new NoInterpClampedFloatParameter(1f, 0f, 1f);
    [Header("Noise Settings")]
    public NoiseParameter Noise = new NoiseParameter();
    [Tooltip("threshold.")]
    public NoInterpClampedFloatParameter threshold = new NoInterpClampedFloatParameter(1f, 0f, 1f);
	public BoolParameter Smoother = new BoolParameter(false);
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