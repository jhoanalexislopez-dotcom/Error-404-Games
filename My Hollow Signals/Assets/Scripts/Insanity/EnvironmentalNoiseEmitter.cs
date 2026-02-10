/*******************************************************
 * Author: [Bianca Marinica]
 * Last Modified: [23/01/2026]
 * Description:
 *    This script allows you to emit environmental noise that affects the player's sanity. You can specify the intensity of the noise, and other scripts can subscribe to the OnEnvironmentalNoise event to react accordingly (e.g., by increasing the player's insanity level). The EmitNoise method can be called with a custom intensity or use the default value set in the inspector.
 *******************************************************/


using UnityEngine;
using UnityEngine.Events;

public class EnvironmentalNoiseEmitter : MonoBehaviour
{
    public static UnityAction<float> OnEnvironmentalNoise;

    [Header("Noise Settings")]
    [Tooltip("How much this noise affects sanity (lower = less impact)")]
    [Range(0f, 1f)]
    public float noiseIntensity = 0.3f;
    
    public void EmitNoise()
    {
        OnEnvironmentalNoise?.Invoke(noiseIntensity);
    }
    
    public void EmitNoise(float customIntensity)
    {
        OnEnvironmentalNoise?.Invoke(customIntensity);
    }
}
