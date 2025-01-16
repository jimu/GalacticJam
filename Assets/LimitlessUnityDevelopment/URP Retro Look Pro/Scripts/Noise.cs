using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[VolumeComponentMenu("Retro Look Pro/Noise")]

public class Noise : VolumeComponent, IPostProcessComponent
{
    public BoolParameter enable = new BoolParameter(false);
	public ClampedFloatParameter fade = new ClampedFloatParameter(0f, 0f, 1f, true);
	[Tooltip("stretch Resolution")]
	public NoInterpFloatParameter stretchResolution = new NoInterpFloatParameter(480f);
	[Tooltip("Vertical Resolution")]
	public NoInterpFloatParameter VerticalResolution = new NoInterpFloatParameter(480f);
	[Space]
	[Space]
	[Tooltip("Granularity")]
	public BoolParameter Granularity = new BoolParameter(false);
	[Tooltip("Granularity Amount")]
	public NoInterpClampedFloatParameter GranularityAmount = new NoInterpClampedFloatParameter(0.5f, 0f, 0.5f);
	[Space]
	[Tooltip("Tape Noise")]
	public BoolParameter TapeNoise = new BoolParameter(false);
	[Tooltip("Tape Noise Signal Processing")]
	public NoInterpClampedFloatParameter TapeNoiseSignalProcessing = new NoInterpClampedFloatParameter(1f, 0f, 15f);
	[Tooltip("Tape Noise Fade")]
	public NoInterpClampedFloatParameter TapeNoiseFade = new NoInterpClampedFloatParameter(1f, 0f, 1.5f);
	[Tooltip("Tape Noise Amount(lower value = more noise)")]
	public NoInterpClampedFloatParameter TapeNoiseAmount = new NoInterpClampedFloatParameter(1f, 0f, 1.5f);
	[Tooltip("tape Lines Amount")]
	public NoInterpClampedFloatParameter tapeLinesAmount = new NoInterpClampedFloatParameter(0.8f, 0f, 1f);
	[Tooltip("Tape Noise Speed")]
	public NoInterpClampedFloatParameter TapeNoiseSpeed = new NoInterpClampedFloatParameter(0.5f, -1.5f, 1.5f);
	[Space]
	[Tooltip("Line Noise")]
	public BoolParameter LineNoise = new BoolParameter(false);
	[Tooltip("Line Noise Amount")]
	public NoInterpClampedFloatParameter LineNoiseAmount = new NoInterpClampedFloatParameter(1f, 0f, 15f);
	[Tooltip("Line Noise Speed")]
	public NoInterpClampedFloatParameter LineNoiseSpeed = new NoInterpClampedFloatParameter(1f, 0f, 10f);
	[Space]
	[Tooltip("Signal Noise")]
	public BoolParameter SignalNoise = new BoolParameter(false);
	[Tooltip("Signal Noise Power")]
	public NoInterpClampedFloatParameter SignalNoisePower = new NoInterpClampedFloatParameter(0.9f, 0.5f, 0.97f);
	[Tooltip("Signal Noise Amount")]
	public NoInterpClampedFloatParameter SignalNoiseAmount = new NoInterpClampedFloatParameter(1f, 0f, 2f);
	[Space]
	[Tooltip("Mask texture")]
	public TextureParameter mask = new TextureParameter(null);
	public maskChannelModeParameter maskChannel = new maskChannelModeParameter();

	[Tooltip("Time.unscaledTime.")]
	public BoolParameter unscaledTime = new BoolParameter(false);
    [Space]
    [Tooltip("Use Global Post Processing Settings to enable or disable Post Processing in scene view or via camera setup. THIS SETTING SHOULD BE TURNED OFF FOR EFFECTS, IN CASE OF USING THEM FOR SEPARATE LAYERS")]
    public BoolParameter GlobalPostProcessingSettings = new BoolParameter(false);


    public bool IsActive() => (bool)enable;

    public bool IsTileCompatible() => false;
}