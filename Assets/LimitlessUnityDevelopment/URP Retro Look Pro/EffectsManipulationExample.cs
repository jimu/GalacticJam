using UnityEngine;
using UnityEngine.Rendering;

public class EffectsManipulationExample : MonoBehaviour
{
    // Post-processing volume containing the Glitch1 effect
    [Tooltip("Reference to the Post-processing Volume component with LimitlessGlitch1 effect.")]
    public Volume volume;

    // Cached reference to the retroEffect effect in the volume
    private LimitlessGlitch1 retroEffect;

    private void Start()
    {
        // Check if the Volume component is assigned
        if (volume == null)
        {
            Debug.LogWarning("Volume component is not assigned.");
            return;
        }

        // Try to get the Glitch1 effect from the volume profile
        if (!volume.profile.TryGet(out retroEffect))
        {
            Debug.LogError("LimitlessGlitch1 effect not found in the Volume profile. Add LimitlessGlitch1 effect to make the script work.");
            return;
        }

        // Activate the Glitch1 effect if found
        retroEffect.active = true;
    }

    private void FixedUpdate()
    {
        // Ensure both the volume and glitchEffect references are valid
        if (volume == null || retroEffect == null)
            return;

        // Randomly change the Glitch1 Amount property value each frame
        retroEffect.Amount.value = Random.Range(0f, 0.5f);
    }
}
