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

    private void StartCinematic()
    {
        cinematicActive = true;

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

        if (lockPlayer && playerController != null)
            playerController.enabled = false;
    }

    private void EndCinematic()
    {
        cinematicActive = false;

        Camera cam = Camera.main;

        if (thisCinematicChangedCamera && cam != null)
        {
            cam.transform.rotation = originalCameraRotation;
            cam.transform.position = originalCameraPosition;
        }

        if (lockPlayer && playerController != null)
            playerController.enabled = true;

        thisCinematicChangedCamera = false;
    }
}
