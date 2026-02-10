using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlickeringLights : MonoBehaviour
{
    [Header("Flicker Settings")]
    [Tooltip("Minimum intensity multiplier during flicker")]
    [Range(0f, 1f)]
    public float minIntensity = 0.2f;
    
    [Tooltip("Maximum intensity multiplier during flicker")]
    [Range(0f, 1f)]
    public float maxIntensity = 1f;
    
    [Header("Timing Settings")]
    [Tooltip("Minimum time between flicker events")]
    public float minFlickerInterval = 0.5f;
    
    [Tooltip("Maximum time between flicker events")]
    public float maxFlickerInterval = 3f;
    
    [Tooltip("Minimum duration of a single flicker")]
    public float minFlickerDuration = 0.05f;
    
    [Tooltip("Maximum duration of a single flicker")]
    public float maxFlickerDuration = 0.3f;
    
    [Header("Tension Settings")]
    [Tooltip("Number of consecutive flickers in a burst")]
    [Range(1, 10)]
    public int burstCount = 3;
    
    [Tooltip("Time between flickers in a burst")]
    public float burstDelay = 0.1f;
    
    [Tooltip("Probability of a burst occurring instead of single flicker (0-1)")]
    [Range(0f, 1f)]
    public float burstProbability = 0.3f;
    
    [Header("Advanced")]
    [Tooltip("Apply flicker to all child lights, or only this GameObject")]
    public bool affectChildLights = true;
    
    private List<LightData> lights = new List<LightData>();
    private float nextFlickerTime;
    
    private class LightData
    {
        public Light light;
        public float originalIntensity;
        public bool isFlickering;
        
        public LightData(Light light)
        {
            this.light = light;
            this.originalIntensity = light.intensity;
            this.isFlickering = false;
        }
    }
    
    private void Start()
    {
        CollectLights();
        ScheduleNextFlicker();
    }
    
    private void CollectLights()
    {
        lights.Clear();
        
        if (affectChildLights)
        {
            Light[] childLights = GetComponentsInChildren<Light>();
            foreach (Light light in childLights)
            {
                lights.Add(new LightData(light));
            }
        }
        else
        {
            Light light = GetComponent<Light>();
            if (light != null)
            {
                lights.Add(new LightData(light));
            }
        }
        
        if (lights.Count == 0)
        {
            Debug.LogWarning($"FlickeringLights on {gameObject.name}: No lights found!");
        }
    }
    
    private void Update()
    {
        if (Time.time >= nextFlickerTime)
        {
            if (Random.value < burstProbability)
            {
                StartCoroutine(FlickerBurst());
            }
            else
            {
                StartCoroutine(FlickerOnce());
            }
            
            ScheduleNextFlicker();
        }
    }
    
    private void ScheduleNextFlicker()
    {
        nextFlickerTime = Time.time + Random.Range(minFlickerInterval, maxFlickerInterval);
    }
    
    private IEnumerator FlickerOnce()
    {
        float duration = Random.Range(minFlickerDuration, maxFlickerDuration);
        float targetIntensity = Random.Range(minIntensity, maxIntensity);
        
        SetLightsIntensity(targetIntensity);
        
        yield return new WaitForSeconds(duration);
        
        RestoreLightsIntensity();
    }
    
    private IEnumerator FlickerBurst()
    {
        for (int i = 0; i < burstCount; i++)
        {
            float duration = Random.Range(minFlickerDuration, maxFlickerDuration);
            float targetIntensity = Random.Range(minIntensity, maxIntensity);
            
            SetLightsIntensity(targetIntensity);
            
            yield return new WaitForSeconds(duration);
            
            RestoreLightsIntensity();
            
            if (i < burstCount - 1)
            {
                yield return new WaitForSeconds(burstDelay);
            }
        }
    }
    
    private void SetLightsIntensity(float multiplier)
    {
        foreach (LightData data in lights)
        {
            if (data.light != null)
            {
                data.light.intensity = data.originalIntensity * multiplier;
                data.isFlickering = true;
            }
        }
    }
    
    private void RestoreLightsIntensity()
    {
        foreach (LightData data in lights)
        {
            if (data.light != null)
            {
                data.light.intensity = data.originalIntensity;
                data.isFlickering = false;
            }
        }
    }
    
    private void OnDestroy()
    {
        RestoreLightsIntensity();
    }
}
