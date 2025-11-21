/*******************************************************
 * Author: [Alejandro Vila]
 * Last Modified: [21/11/2025]
 * Description:
 *    Plays sound effects for button interactions (hover, click, select, deselect).
 *******************************************************/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Audio;

[RequireComponent(typeof(Button))]
public class ButtonSFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("Audio Sources")]
    [Tooltip("AudioSource to play button sounds (will auto-find sfxMixer if not assigned)")]
    public AudioSource audioSource;

    [Header("Sound Effects")]
    [Tooltip("Sound to play when hovering over the button")]
    public AudioClip hoverSound;

    [Tooltip("Sound to play when clicking the button")]
    public AudioClip clickSound;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    [Tooltip("Volume for hover sound")]
    public float hoverVolume = 0.7f;

    [Range(0f, 1f)]
    [Tooltip("Volume for click sound")]
    public float clickVolume = 1f;

    [Range(0.8f, 1.2f)]
    [Tooltip("Pitch variation for sounds")]
    public float pitchVariation = 0.1f;

    [Header("Behavior")]
    [Tooltip("Should play hover sound when selected via gamepad/keyboard")]
    public bool playHoverOnSelect = true;

    private Button button;
    private bool isHovered = false;

    private void Awake()
    {
        button = GetComponent<Button>();

        // Auto-find audio source if not assigned
        if (audioSource == null)
        {
            GameObject sfxMixer = GameObject.Find("AudioGroup/sfxMixer");
            if (sfxMixer != null)
            {
                audioSource = sfxMixer.GetComponent<AudioSource>();
            }
        }

        if (audioSource == null)
        {
            Debug.LogWarning($"No AudioSource found for ButtonSFX on {gameObject.name}. Button sounds will not play.");
        }
    }

    private void Start()
    {
        // Hook up click sound to button
        if (button != null)
        {
            button.onClick.AddListener(PlayClickSound);
        }
    }

    private void OnDestroy()
    {
        // Clean up listener
        if (button != null)
        {
            button.onClick.RemoveListener(PlayClickSound);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isHovered)
        {
            isHovered = true;
            PlayHoverSound();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    // Called when button is selected via gamepad/keyboard navigation
    public void OnSelect(BaseEventData eventData)
    {
        if (playHoverOnSelect && !isHovered)
        {
            PlayHoverSound();
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isHovered = false;
    }

    private void PlayHoverSound()
    {
        if (hoverSound != null && audioSource != null && button.interactable)
        {
            PlaySound(hoverSound, hoverVolume);
        }
    }

    private void PlayClickSound()
    {
        if (clickSound != null && audioSource != null && button.interactable)
        {
            PlaySound(clickSound, clickVolume);
        }
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (audioSource != null && clip != null)
        {
            // Add slight pitch variation for more interesting sounds
            float pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
            audioSource.pitch = pitch;

            audioSource.PlayOneShot(clip, volume);

            // Reset pitch after playing
            audioSource.pitch = 1f;
        }
    }

    // Public methods for manual triggering
    public void TriggerHoverSound()
    {
        PlayHoverSound();
    }

    public void TriggerClickSound()
    {
        PlayClickSound();
    }
}
