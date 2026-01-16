using System.Collections;
using UnityEngine;
using TMPro;

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
    [Tooltip("Hide player UI elements during the event (battery, item counter, etc.)")]
    public bool hideUI = false;

    [Header("Sanity Settings")]
    [Tooltip("Amount of sanity to lower when this event triggers")]
    public float sanityLossAmount = 0f;

    [Header("Phone Message Settings")]
    public bool addPhoneMessages = false;
    [Tooltip("Add a timestamp before the messages")]
    public bool addTimestamp = false;
    [Tooltip("Custom day label (leave empty for current day)")]
    public string timestampDay = "";
    [Tooltip("Custom time text (leave empty for current time)")]
    public string timestampTime = "";
    [System.Serializable]
    public struct PhoneMessage
    {
        public bool isPlayerMessage;
        [TextArea]
        public string messageText;
    }
    public PhoneMessage[] phoneMessages;

    [Header("Trigger Settings")]
    public bool triggerOnlyOnce = true;
    private bool hasTriggered = false;
    private bool hasFinishedMainEvent = false;

    private bool inCube = false;
    private bool cinematicActive = false;
    private Coroutine typingCoroutine;

    private Transform player;
    private MonoBehaviour playerController;

    private Quaternion originalCameraRotation;
    private Vector3 originalCameraPosition;

    private bool thisCinematicChangedCamera = false;

    private bool hasShownPostEventDialogue = false;
    
    private GameObject uiToHide;
    private bool wasUIActive;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        playerController = player?.GetComponent<MonoBehaviour>();

        // Hide the cube mesh
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }
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

        // If the main event already happened and post-event lines exist → play them
        if (hasFinishedMainEvent && postEventDialogueLines.Length > 0)
        {
            if (!hasShownPostEventDialogue)  // Add this check
            {
                PlayPostEventDialogue();
                hasShownPostEventDialogue = true;  // Mark as shown
            }
            return;
        }

        // If main event can only happen once and already did → ignore
        if (triggerOnlyOnce && hasTriggered)
            return;

        if (inCube)
            return;

        inCube = true;
        hasTriggered = true;

        StartCinematic();

        if (playTriggerSound && audioSource != null && triggerSound != null)
            audioSource.PlayOneShot(triggerSound);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(PlayDialogueSequence());


    }

    // ------------------------------
    // MAIN EVENT DIALOGUE
    // ------------------------------

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

        AddPhoneMessagesIfEnabled();

        // Automatically show post-event dialogue if it exists
        if (postEventDialogueLines.Length > 0 && !hasShownPostEventDialogue)
        {
            yield return new WaitForSeconds(0.5f); // Optional small delay
            hasShownPostEventDialogue = true;
            yield return StartCoroutine(PlayPostEventDialogueSequence());
        }
    }


    // ------------------------------
    // POST EVENT DIALOGUE
    // ------------------------------

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

    // ------------------------------
    // TYPEWRITER
    // ------------------------------

    private IEnumerator TypeText(string text)
    {
        subtext.text = "";

        foreach (char c in text)
        {
            subtext.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    // ------------------------------
    // CINEMATIC CONTROL
    // ------------------------------

    private void AddPhoneMessagesIfEnabled()
    {
        if (!addPhoneMessages || phoneMessages.Length == 0)
            return;

        if (MessageManager.instance == null)
        {
            Debug.LogWarning("MessageManager instance not found. Cannot add phone messages.");
            return;
        }

        if (addTimestamp)
        {
            if (string.IsNullOrEmpty(timestampDay) && string.IsNullOrEmpty(timestampTime))
            {
                MessageManager.instance.AddTimestamp();
            }
            else
            {
                string day = string.IsNullOrEmpty(timestampDay) ? System.DateTime.Now.ToString("dddd") : timestampDay;
                string time = string.IsNullOrEmpty(timestampTime) ? System.DateTime.Now.ToString("h:mm tt") : timestampTime;
                MessageManager.instance.AddTimestamp(day, time);
            }
        }

        foreach (PhoneMessage message in phoneMessages)
        {
            MessageManager.instance.AddMessage(message.isPlayerMessage, message.messageText);
        }
    }

    // ------------------------------
    // CINEMATIC CONTROL
    // ------------------------------

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
            {
                sanityManager.LowerSanity(sanityLossAmount);
            }
        }

        if (useCinematicCamera)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                originalCameraRotation = cam.transform.rotation;
                originalCameraPosition = cam.transform.position;
                thisCinematicChangedCamera = true;
            }
        }
        else
        {
            thisCinematicChangedCamera = false;
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

        Camera cam = Camera.main;

        if (thisCinematicChangedCamera && cam != null)
        {
            cam.transform.rotation = originalCameraRotation;
            cam.transform.position = originalCameraPosition;
        }

        thisCinematicChangedCamera = false;
    }
}
