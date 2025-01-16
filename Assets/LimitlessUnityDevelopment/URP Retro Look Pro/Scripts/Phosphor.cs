using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[VolumeComponentMenu("Retro Look Pro/Phosphor")]

public class Phosphor : VolumeComponent, IPostProcessComponent
{
    public BoolParameter enable = new BoolParameter(false);
	public ClampedFloatParameter Fade = new ClampedFloatParameter(0, 0, 1,true);
	public NoInterpClampedFloatParameter width = new NoInterpClampedFloatParameter(0.4f, 0f, 20f);
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