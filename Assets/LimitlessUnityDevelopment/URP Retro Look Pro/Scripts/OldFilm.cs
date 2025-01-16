using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[VolumeComponentMenu("Retro Look Pro/OldFilm")]

public class OldFilm : VolumeComponent, IPostProcessComponent
{
    public BoolParameter enable = new BoolParameter(false);
	[Tooltip("Effect fade.")]
	public ClampedFloatParameter Fade = new ClampedFloatParameter(0f, 0f, 1f,true);

	[Range(0f, 60f), Tooltip("Frames per second.")]
	public NoInterpClampedFloatParameter fps = new NoInterpClampedFloatParameter(1f, 0f, 60f);
	[Range(0f, 5f), Tooltip(".")]
	public NoInterpClampedFloatParameter contrast = new NoInterpClampedFloatParameter(1f, 0f, 5f);

	[Range(-2f, 4f), Tooltip("Image burn.")]
	public NoInterpClampedFloatParameter burn = new NoInterpClampedFloatParameter(0.88f, -2f, 4f);
	[Range(0f, 16f), Tooltip("Scene cut off.")]
	public NoInterpClampedFloatParameter sceneCut = new NoInterpClampedFloatParameter(0.88f, 0f, 16f);
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