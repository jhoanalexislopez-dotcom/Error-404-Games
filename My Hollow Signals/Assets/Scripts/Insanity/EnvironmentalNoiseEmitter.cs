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
