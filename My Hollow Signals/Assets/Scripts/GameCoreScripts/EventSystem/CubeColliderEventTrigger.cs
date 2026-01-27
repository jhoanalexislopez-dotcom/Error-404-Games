using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using TMPro;
using System;

public class CubeColliderEventTrigger : MonoBehaviour
{
    // ==========================
    // GLOBAL DIALOGUE TRACKING
    // ==========================

    private static DialogueUI globalActiveDialogueUI;

    [Header("Dialogue UI")]
    public GameObject dialogueUIPrefab;

    [Header("Main Dialogue (First Trigger Only)")]
    public LocalizedString[] dialogueLines;

    [Header("Post-Event Dialogue (After Main Event Completes)")]
    public LocalizedString[] postEventDialogueLines;

    [Header("Typing Settings")]
    [Range(0.01f, 0.2f)]
    public float typingSpeed = 0.05f;
    public float lineDelay = 1.2f;
    public float autoHideDelay = 2f;

    [Header("Trigger Sound")]
    public bool playTriggerSound = true;
    public AudioSource audioSource;
    public AudioClip triggerSound;

    [Header("Cinematic Settings")]
    public bool lockPlayer = true;
    public bool useCinematicCamera = true;
    public Transform cameraFocusTarget;
    public float cameraTurnSpeed = 5f;

    [Header("UI Settings")]
    public bool hideUI = false;

    [Header("Sanity Settings")]
    public float sanityLossAmount = 0f;

    [Header("Scene Transition Settings")]
    [Tooltip("Enable to trigger scene transition when event ends")]
    public bool triggerSceneTransition = false;
    [Tooltip("Name of the scene to load after transition")]
    public string targetSceneName = "";
    [Tooltip("Delay in seconds before loading the new scene")]
    public float sceneTransitionDelay = 2f;

    // ==========================
    // PHONE MESSAGE SYSTEM
    // ==========================

    [Header("Phone Message Settings")]
    public bool addPhoneMessages = false;

    public enum PhoneEntryType
    {
        Message,
        Timestamp
    }

    [System.Serializable]
    public struct PhoneMessage
    {
        public PhoneEntryType entryType;

        public bool isPlayerMessage;

        public LocalizedString messageText;

        public LocalizedString timestampDay;
        public string timestampTime;
    }

    public PhoneMessage[] phoneMessages;

    // ==========================
    // NOTIFICATION UI SYSTEM
    // ==========================

    [Header("Notification Settings")]
    [Tooltip("Trigger the NotificationUI animator when this event fires")]
    public bool triggerNotification = false;

    // ==========================
    // INTERNAL STATE
    // ==========================

    [Header("Trigger Settings")]
    public bool triggerOnlyOnce = true;

    [Header("Lock Requirements")]
    [Tooltip("Enable to require specific flags before this event can trigger")]
    public bool useRequirements = false;
    
    [Tooltip("Requirements that must be met before this event can trigger")]
    public InteractionRequirement requirements;

    private bool hasTriggered = false;
    private bool hasFinishedMainEvent = false;
    private bool hasShownPostEventDialogue = false;

    private bool inCube = false;
    private bool playerInsideTrigger = false;
    private bool cinematicActive = false;
    private DialogueUI currentDialogueUI;

    private Transform player;
    private MonoBehaviour playerController;

    private GameObject uiToHide;
    private bool wasUIActive;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        playerController = player?.GetComponent<MonoBehaviour>();

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
            meshRenderer.enabled = false;
    }

    private void Update()
    {
        if (!cinematicActive || !useCinematicCamera)
        {
            if (useRequirements && playerInsideTrigger && !hasTriggered && !cinematicActive)
            {
                if (CheckRequirements())
                {
                    TriggerEventSequence();
                }
            }
        }
        else
        {
            Camera cam = Camera.main;
            if (cam == null || cameraFocusTarget == null)
                return;

            cam.transform.rotation = Quaternion.Lerp(
                cam.transform.rotation,
                Quaternion.LookRotation(cameraFocusTarget.position - cam.transform.position),
                Time.deltaTime * cameraTurnSpeed
            );
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInsideTrigger = true;

        if (triggerOnlyOnce && hasTriggered)
            return;

        if (inCube || cinematicActive)
            return;

        if (useRequirements && !CheckRequirements())
            return;

        TriggerEventSequence();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInsideTrigger = false;
            inCube = false;
        }
    }

    private void TriggerEventSequence()
    {
        inCube = true;
        hasTriggered = true;

        // 🔥 ADD PHONE MESSAGES IMMEDIATELY
        AddPhoneMessagesIfEnabled();
        
        // 🔔 TRIGGER NOTIFICATION UI IF ENABLED
        TriggerNotificationIfEnabled();

        StartCinematic();

        if (playTriggerSound && audioSource != null && triggerSound != null)
            audioSource.PlayOneShot(triggerSound);

        DestroyCurrentDialogue();

        if (dialogueUIPrefab != null && dialogueLines.Length > 0)
        {
            GameObject uiInstance = Instantiate(dialogueUIPrefab);
            currentDialogueUI = uiInstance.GetComponent<DialogueUI>();
            globalActiveDialogueUI = currentDialogueUI;

            if (currentDialogueUI != null)
            {
                currentDialogueUI.typingSpeed = typingSpeed;
                currentDialogueUI.lineDelay = lineDelay;
                currentDialogueUI.autoHideDelay = autoHideDelay;
                currentDialogueUI.PlayDialogue(dialogueLines, OnMainDialogueComplete);
            }
        }
        else
        {
            OnMainDialogueComplete();
        }
    }

    private void OnMainDialogueComplete()
    {
        EndCinematic();
        hasFinishedMainEvent = true;
        
        if (globalActiveDialogueUI == currentDialogueUI)
            globalActiveDialogueUI = null;
        
        currentDialogueUI = null;

        if (postEventDialogueLines.Length > 0 && !hasShownPostEventDialogue)
        {
            hasShownPostEventDialogue = true;
            PlayPostEventDialogue();
        }
        else if (triggerSceneTransition && !string.IsNullOrEmpty(targetSceneName))
        {
            SceneTransitionManager.CutToBlackAndLoadScene(targetSceneName, sceneTransitionDelay);
        }
    }

    // ==========================
    // POST EVENT DIALOGUE
    // ==========================

    private void PlayPostEventDialogue()
    {
        DestroyCurrentDialogue();

        if (dialogueUIPrefab != null && postEventDialogueLines.Length > 0)
        {
            GameObject uiInstance = Instantiate(dialogueUIPrefab);
            currentDialogueUI = uiInstance.GetComponent<DialogueUI>();
            globalActiveDialogueUI = currentDialogueUI;

            if (currentDialogueUI != null)
            {
                currentDialogueUI.typingSpeed = typingSpeed;
                currentDialogueUI.lineDelay = lineDelay;
                currentDialogueUI.autoHideDelay = autoHideDelay;
                currentDialogueUI.PlayDialogue(postEventDialogueLines, () => { 
                    currentDialogueUI = null;
                    if (globalActiveDialogueUI == currentDialogueUI)
                        globalActiveDialogueUI = null;
                    
                    if (triggerSceneTransition && !string.IsNullOrEmpty(targetSceneName))
                    {
                        SceneTransitionManager.CutToBlackAndLoadScene(targetSceneName, sceneTransitionDelay);
                    }
                });
            }
        }
    }

    private void DestroyCurrentDialogue()
    {
        if (globalActiveDialogueUI != null)
        {
            Destroy(globalActiveDialogueUI.gameObject);
            globalActiveDialogueUI = null;
        }

        currentDialogueUI = null;
    }

    // ==========================
    // PHONE MESSAGE HANDLING
    // ==========================

    private void AddPhoneMessagesIfEnabled()
    {
        if (!addPhoneMessages || phoneMessages.Length == 0)
            return;

        if (MessageManager.instance == null)
        {
            Debug.LogWarning("MessageManager instance not found.");
            return;
        }

        StartCoroutine(AddPhoneMessagesCoroutine());
    }

    private IEnumerator AddPhoneMessagesCoroutine()
    {
        foreach (PhoneMessage entry in phoneMessages)
        {
            if (entry.entryType == PhoneEntryType.Timestamp)
            {
                string day = "";
                string time = entry.timestampTime;

                if (entry.timestampDay != null && !entry.timestampDay.IsEmpty)
                {
                    var dayLoadOperation = entry.timestampDay.GetLocalizedStringAsync();
                    yield return dayLoadOperation;
                    day = dayLoadOperation.Result;
                }

                if (!string.IsNullOrEmpty(day) || !string.IsNullOrEmpty(time))
                {
                    MessageManager.instance.AddTimestamp(day, time);
                }
                else
                {
                    MessageManager.instance.AddTimestamp();
                }
            }
            else
            {
                var loadOperation = entry.messageText.GetLocalizedStringAsync();
                yield return loadOperation;

                MessageManager.instance.AddMessage(
                    entry.isPlayerMessage,
                    loadOperation.Result
                );
            }
        }
    }

    // ==========================
    // NOTIFICATION UI HANDLING
    // ==========================

    private void TriggerNotificationIfEnabled()
    {
        if (!triggerNotification)
            return;

        GameObject notificationUI = GameObject.Find("NotificationUI");
        if (notificationUI == null)
        {
            Debug.LogWarning("NotificationUI GameObject not found in scene.");
            return;
        }

        Animator animator = notificationUI.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("NotificationUI does not have an Animator component.");
            return;
        }

        animator.SetTrigger("SetNotification");
        Debug.Log("NotificationUI animator trigger 'SetNotification' activated.");
    }

    // ==========================
    // CINEMATIC CONTROL
    // ==========================

    private void StartCinematic()
    {
        cinematicActive = true;

        if (lockPlayer)
        {
            CinematicManager.StartCinematic();
            if (playerController != null)
                playerController.enabled = false;
        }

        if (hideUI)
        {
            uiToHide = GameObject.Find("GameUI");
            if (uiToHide != null)
            {
                wasUIActive = uiToHide.activeSelf;
                uiToHide.SetActive(false);
            }
        }

        if (sanityLossAmount > 0f)
        {
            SanityManager sanityManager = FindObjectOfType<SanityManager>(true);
            if (sanityManager != null)
                sanityManager.LowerSanity(sanityLossAmount);
        }
    }

    private void EndCinematic()
    {
        cinematicActive = false;

        if (lockPlayer)
        {
            CinematicManager.EndCinematic();
            if (playerController != null)
                playerController.enabled = true;
        }

        if (hideUI && uiToHide != null)
        {
            uiToHide.SetActive(wasUIActive);
            uiToHide = null;
        }
    }

    private bool CheckRequirements()
    {
        if (requirements == null)
        {
            Debug.LogWarning($"Requirements enabled but not configured on {gameObject.name}");
            return false;
        }

        bool requirementsMet = requirements.AreRequirementsMet();

        if (!requirementsMet)
        {
            LocalizedString lockReason = requirements.GetLockReason();
            if (lockReason != null && !lockReason.IsEmpty)
            {
                Debug.Log($"Event trigger requirements not met: {lockReason.GetLocalizedString()}");
            }
        }

        return requirementsMet;
    }
}
