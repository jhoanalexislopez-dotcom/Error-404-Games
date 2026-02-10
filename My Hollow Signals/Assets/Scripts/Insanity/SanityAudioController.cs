/*******************************************************
 * Author: [Ignacio Lopez]
 * Last Modified: [29/01/2026]
 * Description:
 *    This script manages the audio effects related to the player's sanity level. It plays a specific sound when the player's sanity drops below a certain threshold and adjusts the volume based on how low the sanity is. Additionally, it can modify the RF Noise of a CRT effect to enhance the atmosphere as sanity decreases. The script also includes functionality to reset these effects after a jumpscare event.
 *******************************************************/

using UnityEngine;
using RetroTVFX;

public class SanityAudioController : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("The audio clip to play when sanity is low")]
    [SerializeField] private AudioClip lowSanityClip;
    
    [Tooltip("Sanity percentage threshold to start playing the sound (0-1)")]
    [SerializeField] private float sanityThreshold = 0.5f;
    
    [Tooltip("Minimum volume when at threshold")]
    [SerializeField] private float minVolume = 0.0f;
    
    [Tooltip("Maximum volume when at 0% sanity")]
    [SerializeField] private float maxVolume = 1.0f;
    
    [Tooltip("How smoothly the volume transitions")]
    [SerializeField] private float volumeTransitionSpeed = 2f;

    [Header("CRT Effect Settings")]
    [Tooltip("Reference to the CRT Effect on the camera")]
    [SerializeField] private CRTEffect crtEffect;
    
    [Tooltip("Initial RF Noise value when at threshold")]
    [SerializeField] private float minRFNoise = 0.0f;
    
    [Tooltip("Maximum RF Noise value when at 0% sanity")]
    [SerializeField] private float maxRFNoise = 0.30f;
    
    [Tooltip("How smoothly the RF noise transitions")]
    [SerializeField] private float rfNoiseTransitionSpeed = 2f;

    [Header("References")]
    [Tooltip("Reference to the SanityManager")]
    [SerializeField] private SanityManager sanityManager;

    private AudioSource audioSource;
    private float targetVolume = 0f;
    private float targetRFNoise = 0f;
    private bool isPlaying = false;
    private float initialRFNoise = 0f;
    private bool isResettingAfterJumpscare = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.clip = lowSanityClip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0f;
    }

    private void Start()
    {
        if (sanityManager == null)
        {
            sanityManager = FindObjectOfType<SanityManager>();
            
            if (sanityManager == null)
            {
                Debug.LogError("SanityAudioController: SanityManager not found!");
                enabled = false;
                return;
            }
        }

        if (lowSanityClip == null)
        {
            Debug.LogWarning("SanityAudioController: No audio clip assigned!");
        }

        if (crtEffect == null)
        {
            crtEffect = FindObjectOfType<CRTEffect>();
            
            if (crtEffect == null)
            {
                Debug.LogWarning("SanityAudioController: CRTEffect not found!");
            }
        }

        if (crtEffect != null)
        {
            initialRFNoise = crtEffect.RFNoise;
        }
    }

    private void Update()
    {
        if (sanityManager == null || sanityManager.sanitySlider == null)
        {
            return;
        }

        if (isResettingAfterJumpscare)
        {
            targetVolume = 0f;
            targetRFNoise = initialRFNoise;
            
            audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, Time.deltaTime * volumeTransitionSpeed);

            if (crtEffect != null)
            {
                crtEffect.RFNoise = Mathf.Lerp(crtEffect.RFNoise, targetRFNoise, Time.deltaTime * rfNoiseTransitionSpeed);
            }

            if (audioSource.volume <= 0.01f && isPlaying)
            {
                audioSource.Stop();
                isPlaying = false;
            }

            return;
        }

        float currentSanity = sanityManager.sanitySlider.value;
        float maxSanity = sanityManager.fullSanity;
        float sanityPercent = currentSanity / maxSanity;

        if (sanityPercent <= sanityThreshold)
        {
            if (!isPlaying && lowSanityClip != null)
            {
                audioSource.Play();
                isPlaying = true;
            }

            float normalizedSanity = sanityPercent / sanityThreshold;
            targetVolume = Mathf.Lerp(maxVolume, minVolume, normalizedSanity);
            targetRFNoise = Mathf.Lerp(maxRFNoise, minRFNoise, normalizedSanity);
        }
        else
        {
            targetVolume = 0f;
            targetRFNoise = initialRFNoise;
            
            if (isPlaying && audioSource.volume <= 0.01f)
            {
                audioSource.Stop();
                isPlaying = false;
            }
        }

        audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, Time.deltaTime * volumeTransitionSpeed);

        if (crtEffect != null)
        {
            crtEffect.RFNoise = Mathf.Lerp(crtEffect.RFNoise, targetRFNoise, Time.deltaTime * rfNoiseTransitionSpeed);
        }
    }

    private void OnDisable()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            isPlaying = false;
        }
    }

    public void ResetEffectsAfterJumpscare()
    {
        isResettingAfterJumpscare = true;
    }
}
