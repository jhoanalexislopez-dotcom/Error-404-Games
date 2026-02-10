/*******************************************************
 * Author: [Bianca Marinica]
 * Last Modified: [23/01/2026]
 * Description:
 *    This script manages the player's sanity level, which is affected by microphone input and environmental noise. It updates a UI slider to reflect the current sanity, applies visual effects like vignette intensity based on sanity, and triggers events when sanity reaches critical levels (e.g., showing a warning or triggering a jumpscare). The script also allows for sanity recovery when the player is quiet and includes functionality to update a sanity image based on the current sanity percentage.
 *******************************************************/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Events;
using UnityEngine.Localization;

public class SanityManager : MonoBehaviour
{   
    [Header("UI & PostProcess")]
    public Slider sanitySlider;
    public PostProcessProfile profile;
    Vignette vignette;

    [Header("Sanity Values")]
    public int fullSanity;
        
    [Header("Mic Settings")]
    public AudioLoudnessDetector micDetector;
    private float micThreshold = 0.01f;
    private const string MIC_SENSITIVITY_KEY = "MicrophoneSensitivity"; 
    public float micSanityLossMultiplier; //cuánto afecta hablar fuerte
    public float micQuietRecoveryMultiplier; //cuánto afecta no hablar
    [Header("Environmental Noise Settings")]
    [Tooltip("Multiplier for environmental noise (footsteps, doors, etc.). Should be lower than mic multiplier")]
    public float environmentalNoiseMultiplier = 5f;

    [Header("Low Sanity Warning")]
    [Tooltip("DialogueUI_Thinking prefab to show warning")]
    public GameObject dialogueUIPrefab;
    [Tooltip("Warning message when sanity reaches threshold")]
    public LocalizedString lowSanityWarning;
    [Range(0f, 1f)]
    [Tooltip("Sanity percentage threshold to show warning (0.5 = 50%)")]
    public float warningThreshold = 0.5f;
    private bool warningShown = false;

    [Header("Eventos")]
    public UnityEvent onInsane; //Evento cuando muere
    private bool isInsane = false;

    [Header("Jumpscare")]
    public JumpscareManager jumpscareManager;

    [Header("Sanity visual")]
    public Image sanityImage;
    public Sprite[] sanitySprites;

    private Coroutine sanityCoroutine;

    void Start()
    {
        micThreshold = PlayerPrefs.GetFloat(MIC_SENSITIVITY_KEY, 0.01f);
        
        profile.TryGetSettings(out vignette);
        sanitySlider = GetComponent<Slider>();
        sanitySlider.maxValue = fullSanity;
        sanitySlider.value = fullSanity;

        vignette.intensity.value = 0;

        sanityCoroutine = StartCoroutine(MicSanityRoutine());
    }

    void OnEnable()
    {
        if (vignette != null && sanityCoroutine == null && !isInsane)
        {
            sanityCoroutine = StartCoroutine(MicSanityRoutine());
        }

        EnvironmentalNoiseEmitter.OnEnvironmentalNoise += HandleEnvironmentalNoise;
    }

    void OnDisable()
    {
        if (sanityCoroutine != null)
        {
            StopCoroutine(sanityCoroutine);
            sanityCoroutine = null;
        }

        EnvironmentalNoiseEmitter.OnEnvironmentalNoise -= HandleEnvironmentalNoise;
    }

    IEnumerator MicSanityRoutine()
    {
        while (!isInsane)
        {
            float loudness = 0f;

            if (micDetector != null)
            {
                loudness = micDetector.GetLoudnessFromMicrophone();
            }

            if (loudness > micThreshold)
            {
                float loss = micSanityLossMultiplier * Time.deltaTime;
                sanitySlider.value -= loss;
            }

            else
            {
                float gain = micQuietRecoveryMultiplier * Time.deltaTime;
                sanitySlider.value += gain;
            }

            sanitySlider.value = Mathf.Clamp(sanitySlider.value, 0f, fullSanity);

            float newValue = (fullSanity - sanitySlider.value) / fullSanity;
            vignette.intensity.value = Mathf.Clamp01(newValue) * 0.5f;

            UpdateSanityImage();
            CheckAndShowLowSanityWarning();

            if (sanitySlider.value <= 0)
            {
                isInsane = true;
                Debug.Log("You're nuts!");
                
                if (jumpscareManager != null)
                {
                    jumpscareManager.TriggerJumpscare();
                }
                
                onInsane.Invoke();
                yield break;
            }

            yield return null;
        }            
    }    

    public void UpdateSanityImage()
    {
        if (sanitySprites.Length == 0 || sanityImage == null)
        {
            return;
        }

        float percent = sanitySlider.value / fullSanity;

        int index = 0;

        if (percent > 0f && percent < 0.2f)
        {
            index = 0;
        }

        else if (percent >= 0.2f && percent < 0.4f)
        {
            index = 1;
        }

        else if (percent >= 0.4f && percent < 0.6f)
        {
            index = 2;
        }

        else if (percent >= 0.6f && percent < 0.8f)
        {
            index = 3;
        }

        else if (percent >= 0.8f)
        {
            index = 4;
        }

        sanityImage.sprite = sanitySprites[index];
    }

    private void HandleEnvironmentalNoise(float noiseIntensity)
    {
        if (isInsane)
        {
            return;
        }

        float sanityLoss = noiseIntensity * environmentalNoiseMultiplier;
        LowerSanity(sanityLoss);
    }

    public void LowerSanity(float amount)
    {
        if (isInsane)
        {
            return;
        }

        if (sanitySlider == null)
        {
            sanitySlider = GetComponent<Slider>();
        }

        sanitySlider.value -= amount;
        sanitySlider.value = Mathf.Clamp(sanitySlider.value, 0f, fullSanity);

        if (vignette != null)
        {
            float newValue = (fullSanity - sanitySlider.value) / fullSanity;
            vignette.intensity.value = Mathf.Clamp01(newValue) * 0.5f;
        }

        UpdateSanityImage();
        CheckAndShowLowSanityWarning();

        if (sanitySlider.value <= 0)
        {
            isInsane = true;
            Debug.Log("You're nuts!");
            
            if (jumpscareManager != null)
            {
                jumpscareManager.TriggerJumpscare();
            }
            
            onInsane.Invoke();
        }
    }

    private void CheckAndShowLowSanityWarning()
    {
        if (warningShown || isInsane)
        {
            return;
        }

        float sanityPercent = sanitySlider.value / fullSanity;

        if (sanityPercent <= warningThreshold)
        {
            warningShown = true;
            ShowLowSanityWarning();
        }
    }

    private void ShowLowSanityWarning()
    {
        if (dialogueUIPrefab == null || lowSanityWarning == null || lowSanityWarning.IsEmpty)
        {
            return;
        }

        GameObject gameUIObject = GameObject.Find("GameUI");
        Transform parent = gameUIObject != null ? gameUIObject.transform : null;

        GameObject uiInstance = Instantiate(dialogueUIPrefab, parent);
        DialogueUI dialogueUI = uiInstance.GetComponent<DialogueUI>();

        if (dialogueUI != null)
        {
            LocalizedString[] warningLines = new LocalizedString[] { lowSanityWarning };
            dialogueUI.PlayDialogue(warningLines);
        }
    }
}
