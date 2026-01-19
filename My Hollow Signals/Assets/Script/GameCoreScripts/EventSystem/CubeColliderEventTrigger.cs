using System.Collections;
using UnityEngine;
using TMPro;
using System;

public class CubeColliderEventTrigger : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI subtext;

    [Header("Main Dialogue (First Trigger Only)")]
    [TextArea]
    public string[] dialogueLines;

    [Header("Post-Event Dialogue (After Main Event Completes)")]
    [TextArea]
    public string[] postEventDialogueLines;

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

        [TextArea]
        public string messageText;

        public string timestampDay;
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

    private bool hasTriggered = false;
    private bool hasFinishedMainEvent = false;
    private bool hasShownPostEventDialogue = false;

    private bool inCube = false;
    private bool cinematicActive = false;
    private Coroutine typingCoroutine;

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
            return;

        Camera cam = Camera.main;
        if (cam == null || cameraFocusTarget == null)
            return;

        cam.transform.rotation = Quaternion.Lerp(
            cam.transform.rotation,
            Quaternion.LookRotation(cameraFocusTarget.position - cam.transform.position),
            Time.deltaTime * cameraTurnSpeed
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (hasFinishedMainEvent && postEventDialogueLines.Length > 0)
        {
            if (!hasShownPostEventDialogue)
            {
                PlayPostEventDialogue();
                hasShownPostEventDialogue = true;
            }
            return;
        }

        if (triggerOnlyOnce && hasTriggered)
            return;

        if (inCube)
            return;

        inCube = true;
        hasTriggered = true;

        // 🔥 ADD PHONE MESSAGES IMMEDIATELY
        AddPhoneMessagesIfEnabled();
        
        // 🔔 TRIGGER NOTIFICATION UI IF ENABLED
        TriggerNotificationIfEnabled();

        StartCinematic();

        if (playTriggerSound && audioSource != null && triggerSound != null)
            audioSource.PlayOneShot(triggerSound);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(PlayDialogueSequence());
    }

    // ==========================
    // MAIN DIALOGUE
    // ==========================

    private IEnumerator PlayDialogueSequence()
    {
        foreach (string line in dialogueLines)
        {
            yield return StartCoroutine(TypeText(line));
            yield return new WaitForSeconds(lineDelay);
        }

        yield return new WaitForSeconds(autoHideDelay);

        subtext.text = "";
        EndCinematic();

        hasFinishedMainEvent = true;

        if (postEventDialogueLines.Length > 0 && !hasShownPostEventDialogue)
        {
            yield return new WaitForSeconds(0.5f);
            hasShownPostEventDialogue = true;
            yield return StartCoroutine(PlayPostEventDialogueSequence());
        }
    }

    // ==========================
    // POST EVENT DIALOGUE
    // ==========================

    private void PlayPostEventDialogue()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(PlayPostEventDialogueSequence());
    }

    private IEnumerator PlayPostEventDialogueSequence()
    {
        foreach (string line in postEventDialogueLines)
        {
            yield return StartCoroutine(TypeText(line));
            yield return new WaitForSeconds(lineDelay);
        }

        yield return new WaitForSeconds(autoHideDelay);
        subtext.text = "";
    }

    // ==========================
    // TYPEWRITER
    // ==========================

    private IEnumerator TypeText(string text)
    {
        subtext.text = "";

        foreach (char c in text)
        {
            subtext.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
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

        foreach (PhoneMessage entry in phoneMessages)
        {
            if (entry.entryType == PhoneEntryType.Timestamp)
            {
                if (string.IsNullOrEmpty(entry.timestampDay) &&
                    string.IsNullOrEmpty(entry.timestampTime))
                {
                    MessageManager.instance.AddTimestamp();
                }
                else
                {
                    string day = string.IsNullOrEmpty(entry.timestampDay)
                        ? DateTime.Now.ToString("dddd")
                        : entry.timestampDay;

                    string time = string.IsNullOrEmpty(entry.timestampTime)
                        ? DateTime.Now.ToString("h:mm tt")
                        : entry.timestampTime;

                    MessageManager.instance.AddTimestamp(day, time);
                }
            }
            else
            {
                MessageManager.instance.AddMessage(
                    entry.isPlayerMessage,
                    entry.messageText
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
}
