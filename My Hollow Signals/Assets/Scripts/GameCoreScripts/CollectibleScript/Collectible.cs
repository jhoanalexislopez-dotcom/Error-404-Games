/*******************************************************
 * Author: [Bianca Marinica]
 * Last Modified: [21/11/2025]
 * Description:
 *    Represents collectible items (like notes or objects) that players can pick up. Implements the IInteractable interface..
 *******************************************************/

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

public class Collectible : MonoBehaviour, IInteractable
{
    [SerializeField] private LocalizedString localizedDescription;
    [SerializeField] private int value = 1;
    [SerializeField] private GameObject noteUI;

    [Header("Note Settings")]
    [SerializeField] private string noteTitle = "Note";
    [SerializeField][TextArea(3, 6)] private string noteText = "This is a note...";

    [Header("Sanity Settings")]
    [Tooltip("Amount of sanity to lower when this item is collected")]
    [SerializeField] private float sanityLossAmount = 0f;

    [Header("Audio Settings")]
    [Tooltip("Optional AudioSource to use. If not set, one will be created automatically")]
    [SerializeField] private AudioSource customAudioSource;
    
    [Tooltip("Sound effect to play when collecting")]
    [SerializeField] private AudioClip collectSound;
    
    [Tooltip("Volume for the collect sound")]
    [SerializeField][Range(0f, 1f)] private float soundVolume = 1f;

    private AudioSource audioSource;

    void Start()
    {
        if (customAudioSource != null)
        {
            audioSource = customAudioSource;
        }
        else
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && collectSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.loop = false;
                audioSource.spatialBlend = 1f;
            }
        }
        
        if (audioSource != null)
        {
            audioSource.loop = false;
        }
    }

    public LocalizedString GetLocalizedDescription()
    {
        return localizedDescription;
    }

    public void Interact()
    {
        PlayCollectSound();

        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.AddItem(value);
            PlayerInventory.Instance.AddNote(noteTitle, noteText);
        }

        if (sanityLossAmount > 0f)
        {
            SanityManager sanityManager = FindObjectOfType<SanityManager>(true);
            if (sanityManager != null)
            {
                sanityManager.LowerSanity(sanityLossAmount);
            }
        }

        if (noteUI != null)
        {
            noteUI.SetActive(true);

            var noteUIManager = noteUI.GetComponent<NoteUIManager>();
            if (noteUIManager != null)
            {
                noteUIManager.SetNoteActive(noteText);
            }

            Time.timeScale = 0f;

            PauseMenuManager pauseManager = FindObjectOfType<PauseMenuManager>();
            if (pauseManager != null)
            {
                pauseManager.enabled = false;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            var playerController = FindObjectOfType<FirstPersonController>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }
        }

        Destroy(gameObject);
    }

    private void PlayCollectSound()
    {
        if (collectSound != null && audioSource != null)
        {
            audioSource.loop = false;
            audioSource.clip = collectSound;
            audioSource.volume = soundVolume;
            audioSource.Play();
        }
    }
}
