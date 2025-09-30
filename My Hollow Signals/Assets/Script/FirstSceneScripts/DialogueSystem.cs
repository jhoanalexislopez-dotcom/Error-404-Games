using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI dialogueText;
    public Animator transitionAnimator;

    [Header("Dialogue Lines")]
    [Tooltip("Define your dialogue lines directly here")]
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();

    [Header("Typing Settings")]
    [Range(0.01f, 0.2f)]
    public float typingSpeed = 0.05f;

    [Header("Input Settings")]
    [Tooltip("Use your existing Player Input Actions asset")]
    public InputActionAsset playerInputActions;

    [Tooltip("Name of the action to use for advancing dialogue (e.g., 'Interact', 'Attack', 'Jump')")]
    public string advanceActionName = "Interact";

    [Header("Audio Settings")]
    public AudioSource typingAudioSource;
    public AudioClip typingSound;

    [Tooltip("How many characters to type before playing the next sound")]
    [Range(1, 10)]
    public int charactersPerSound = 3;

    [Tooltip("Maximum length to play from the audio clip (in seconds)")]
    [Range(0.01f, 1f)]
    public float maxAudioDuration = 0.1f;

    [Tooltip("Volume for typing sounds")]
    [Range(0f, 1f)]
    public float typingVolume = 0.5f;

    [Header("Transition Settings")]
    [Tooltip("Time to wait for fade out transition before starting dialogue")]
    public float fadeOutWaitTime = 1.67f;

    [Tooltip("Time to wait after triggering transition before loading scene")]
    public float transitionDelayBeforeSceneLoad = 2f;

    [Tooltip("Name of the scene to load after dialogue ends")]
    public string nextSceneName = "TestScene";

    [Tooltip("Name of the animator trigger for the transition")]
    public string transitionTriggerName = "StartTransition";

    private int currentDialogueIndex = 0;
    private bool isTyping = false;
    private bool dialogueComplete = false;
    private bool dialogueStarted = false;
    private Coroutine typingCoroutine;
    private InputAction advanceAction;
    private int characterCount = 0;

    private void OnEnable()
    {
        SetupInputAction();
    }

    private void OnDisable()
    {
        CleanupInputAction();
    }

    private void SetupInputAction()
    {
        if (playerInputActions != null)
        {
            var playerActionMap = playerInputActions.FindActionMap("Player");
            if (playerActionMap != null)
            {
                advanceAction = playerActionMap.FindAction(advanceActionName);
                if (advanceAction != null)
                {
                    advanceAction.performed += OnAdvanceDialogue;
                    advanceAction.Enable();
                }
                else
                {
                    Debug.LogWarning($"Action '{advanceActionName}' not found in Player action map!");
                }
            }
            else
            {
                Debug.LogWarning("Player action map not found in Input Actions asset!");
            }
        }
        else
        {
            Debug.LogWarning("Player Input Actions asset not assigned!");
        }
    }

    private void CleanupInputAction()
    {
        if (advanceAction != null)
        {
            advanceAction.performed -= OnAdvanceDialogue;
            advanceAction.Disable();
        }
    }

    private void Start()
    {
        if (transitionAnimator == null)
        {
            transitionAnimator = GameObject.Find("Canvas/Image")?.GetComponent<Animator>();
            if (transitionAnimator == null)
            {
                Debug.LogError("Transition Animator not found! Please assign the transitionAnimator field in the DialogueSystem component.");
            }
        }

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        if (dialogueLines != null && dialogueLines.Count > 0)
        {
            StartCoroutine(WaitForFadeOutThenStartDialogue());
        }
        else
        {
            Debug.LogWarning("No dialogue lines defined in the inspector!");
        }
    }

    private IEnumerator WaitForFadeOutThenStartDialogue()
    {
        yield return new WaitForSeconds(fadeOutWaitTime);
        StartDialogue();
    }

    public void StartDialogue()
    {
        currentDialogueIndex = 0;
        dialogueComplete = false;
        dialogueStarted = true;
        DisplayCurrentLine();
    }

    private void DisplayCurrentLine()
    {
        if (currentDialogueIndex < dialogueLines.Count)
        {
            DialogueLine currentLine = dialogueLines[currentDialogueIndex];

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            typingCoroutine = StartCoroutine(TypeText(currentLine.text));
        }
        else
        {
            EndDialogue();
        }
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";
        characterCount = 0;

        foreach (char character in text)
        {
            dialogueText.text += character;

            if (character != ' ' && character != '\n')
            {
                characterCount++;

                if (characterCount % charactersPerSound == 0)
                {
                    PlayTypingSound();
                }
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void PlayTypingSound()
    {
        if (typingAudioSource != null && typingSound != null)
        {
            typingAudioSource.Stop();
            typingAudioSource.pitch = Random.Range(0.9f, 1.1f);
            typingAudioSource.volume = typingVolume;
            typingAudioSource.PlayOneShot(typingSound, typingVolume);

            StartCoroutine(StopAudioAfterDuration(maxAudioDuration));
        }
    }

    private IEnumerator StopAudioAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (typingAudioSource != null && typingAudioSource.isPlaying)
        {
            typingAudioSource.Stop();
        }
    }

    private void OnAdvanceDialogue(InputAction.CallbackContext context)
    {
        if (!dialogueStarted)
            return;

        if (isTyping)
        {
            CompleteCurrentLine();
        }
        else if (!dialogueComplete)
        {
            AdvanceToNextLine();
        }
    }

    private void CompleteCurrentLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        if (typingAudioSource != null)
        {
            typingAudioSource.Stop();
        }

        isTyping = false;

        if (currentDialogueIndex < dialogueLines.Count)
        {
            dialogueText.text = dialogueLines[currentDialogueIndex].text;
        }
    }

    private void AdvanceToNextLine()
    {
        currentDialogueIndex++;
        DisplayCurrentLine();
    }

    private void EndDialogue()
    {
        dialogueComplete = true;
        if (typingAudioSource != null)
        {
            typingAudioSource.Stop();
        }

        Debug.Log("Dialogue ended, triggering transition animation...");
        StartCoroutine(TriggerTransitionAndChangeScene());
    }

    private IEnumerator TriggerTransitionAndChangeScene()
    {
        if (transitionAnimator != null)
        {
            Debug.Log($"Triggering animator with '{transitionTriggerName}' trigger...");
            transitionAnimator.SetTrigger(transitionTriggerName);
        }
        else
        {
            Debug.LogWarning("transitionAnimator is null, cannot trigger transition animation!");
        }

        Debug.Log($"Waiting {transitionDelayBeforeSceneLoad} seconds before loading scene...");
        yield return new WaitForSeconds(transitionDelayBeforeSceneLoad);

        Debug.Log($"Loading scene: {nextSceneName}");
        SceneManager.LoadScene(nextSceneName);
    }

    public void RestartDialogue()
    {
        StartDialogue();
    }

    public bool IsDialogueActive()
    {
        return !dialogueComplete;
    }

    public bool IsCurrentlyTyping()
    {
        return isTyping;
    }

    public void ChangeAdvanceAction(string newActionName)
    {
        CleanupInputAction();
        advanceActionName = newActionName;
        SetupInputAction();
    }

    public void AddDialogueLine(string text)
    {
        DialogueLine newLine = new DialogueLine();
        newLine.text = text;
        dialogueLines.Add(newLine);
    }

    public void ClearDialogue()
    {
        dialogueLines.Clear();
        currentDialogueIndex = 0;
        dialogueComplete = true;
    }
}
