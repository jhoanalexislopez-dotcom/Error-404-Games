using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Events;
using UnityEditorInternal.VersionControl;

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
    public float micSanityLossMultiplier; //cu�nto afecta hablar fuerte
    public float micQuietRecoveryMultiplier; //cu�nto affecta no hablar

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
    }

    void OnDisable()
    {
        if (sanityCoroutine != null)
        {
            StopCoroutine(sanityCoroutine);
            sanityCoroutine = null;
        }
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
}
