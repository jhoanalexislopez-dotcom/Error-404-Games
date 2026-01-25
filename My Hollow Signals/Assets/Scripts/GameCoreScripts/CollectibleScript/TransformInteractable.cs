/*******************************************************
 * Author: [Bianca Marinica]
 * Last Modified: [21/11/2025]
 * Description:
 *    Interactable object that transforms (position, rotation, scale) when interacted with.
 *    Used for doors, drawers, and other moveable objects.
 *******************************************************/

using UnityEngine;
using UnityEngine.Localization;

public class TransformInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private LocalizedString localizedDescription;

    [Header("Transform Settings")]
    [Tooltip("Target position offset (local space)")]
    [SerializeField] private Vector3 targetPosition;
    
    [Tooltip("Target rotation (local space)")]
    [SerializeField] private Vector3 targetRotation;
    
    [Tooltip("Target scale")]
    [SerializeField] private Vector3 targetScale = Vector3.one;

    [Header("Animation Settings")]
    [Tooltip("Time to complete the transformation")]
    [SerializeField] private float transformDuration = 1f;
    
    [Tooltip("Animation curve for smooth movement")]
    [SerializeField] private AnimationCurve transformCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Audio Settings")]
    [Tooltip("Optional AudioSource to use. If not set, one will be created automatically")]
    [SerializeField] private AudioSource customAudioSource;
    
    [Tooltip("Sound effect to play when interacting")]
    [SerializeField] private AudioClip interactionSound;
    
    [Tooltip("Volume for the interaction sound")]
    [SerializeField][Range(0f, 1f)] private float soundVolume = 1f;

    [Header("Interaction Settings")]
    [Tooltip("Can this object be interacted with multiple times?")]
    [SerializeField] private bool canReInteract = false;
    
    [Tooltip("If true, object returns to original state. If false, toggles between states")]
    [SerializeField] private bool returnToOriginal = false;
    
    [Header("Requirements")]
    [Tooltip("Optional requirements that must be met before interaction")]
    [SerializeField] private InteractionRequirement interactionRequirement = null;
    
    [Header("Locked Feedback")]
    [Tooltip("Optional AudioClip to play when requirements are not met")]
    [SerializeField] private AudioClip lockedSound;
    
    [Tooltip("Reference to DialogueUI prefab for displaying locked messages")]
    [SerializeField] private GameObject dialogueUIPrefab;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialScale;

    private Vector3 currentTargetPosition;
    private Quaternion currentTargetRotation;
    private Vector3 currentTargetScale;

    private bool isTransforming = false;
    private bool hasInteracted = false;
    private bool isInTargetState = false;

    private float transformProgress = 0f;
    private AudioSource audioSource;
    private DialogueUI currentDialogueUI;

    private const float EPSILON = 0.001f;

    void Start()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
        initialScale = transform.localScale;

        currentTargetPosition = initialPosition + targetPosition;
        currentTargetRotation = Quaternion.Euler(targetRotation) * initialRotation;
        currentTargetScale = targetScale;

        if (customAudioSource != null)
        {
            audioSource = customAudioSource;
        }
        else
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null && interactionSound != null)
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
        if (isTransforming)
            return;

        if (hasInteracted && !canReInteract)
            return;
        
        if (interactionRequirement != null && !interactionRequirement.AreRequirementsMet())
        {
            OnRequirementsNotMet();
            return;
        }

        hasInteracted = true;

        if (canReInteract && !returnToOriginal)
        {
            isInTargetState = !isInTargetState;
        }

        PlaySound();
        StartTransform();
    }
    
    private void OnRequirementsNotMet()
    {
        if (interactionRequirement != null)
        {
            string reason = interactionRequirement.GetLockReason();
            
            if (lockedSound != null && audioSource != null)
            {
                audioSource.loop = false;
                audioSource.clip = lockedSound;
                audioSource.volume = soundVolume;
                audioSource.Play();
            }
            
            ShowLockedDialogue(reason);
        }
    }
    
    private void ShowLockedDialogue(string message)
    {
        if (dialogueUIPrefab != null)
        {
            if (currentDialogueUI != null)
            {
                Destroy(currentDialogueUI.gameObject);
                currentDialogueUI = null;
            }
            
            GameObject uiInstance = Instantiate(dialogueUIPrefab);
            currentDialogueUI = uiInstance.GetComponent<DialogueUI>();
            
            if (currentDialogueUI != null)
            {
                string[] lines = { message };
                currentDialogueUI.PlayDialogue(lines, OnDialogueComplete);
            }
        }
        else
        {
            Debug.Log($"Cannot interact: {message}");
        }
    }
    
    private void OnDialogueComplete()
    {
        currentDialogueUI = null;
    }

    private void StartTransform()
    {
        isTransforming = true;
        transformProgress = 0f;
    }

    private void PlaySound()
    {
        if (interactionSound != null && audioSource != null)
        {
            audioSource.loop = false;
            audioSource.clip = interactionSound;
            audioSource.volume = soundVolume;
            audioSource.Play();
        }
    }

    void Update()
    {
        if (isTransforming)
        {
            transformProgress += Time.deltaTime / transformDuration;
            float curveValue = transformCurve.Evaluate(transformProgress);

            Vector3 startPos, endPos;
            Quaternion startRot, endRot;
            Vector3 startScale, endScale;

            if (returnToOriginal || (!canReInteract && !isInTargetState) || (canReInteract && isInTargetState))
            {
                startPos = initialPosition;
                startRot = initialRotation;
                startScale = initialScale;
                endPos = currentTargetPosition;
                endRot = currentTargetRotation;
                endScale = currentTargetScale;
            }
            else
            {
                startPos = currentTargetPosition;
                startRot = currentTargetRotation;
                startScale = currentTargetScale;
                endPos = initialPosition;
                endRot = initialRotation;
                endScale = initialScale;
            }

            transform.localPosition = Vector3.Lerp(startPos, endPos, curveValue);
            transform.localRotation = Quaternion.Slerp(startRot, endRot, curveValue);
            transform.localScale = Vector3.Lerp(startScale, endScale, curveValue);

            if (transformProgress >= 1f)
            {
                isTransforming = false;
                transformProgress = 1f;

                if (returnToOriginal)
                {
                    hasInteracted = false;
                }
            }
        }
    }
}
