using System.Collections;
using UnityEngine;
using TMPro;

public class CubeColliderEventTrigger : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI subtext;

    [Header("Dialogue Lines")]
    [TextArea]
    public string[] dialogueLines;

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
    public bool triggerOnlyOnce = true;   // <--- NEW
    private bool hasTriggered = false;    // <--- NEW

    private bool inCube = false;
    private bool cinematicActive = false;
    private Coroutine typingCoroutine;

    private Transform player;
    private MonoBehaviour playerController;

    // Camera reset storage
    private Quaternion originalCameraRotation;
    private Vector3 originalCameraPosition;

    private bool thisCinematicChangedCamera = false;


    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        playerController = player?.GetComponent<MonoBehaviour>();
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

        // Block retriggering if already fired once
        if (triggerOnlyOnce && hasTriggered)
            return;

        // If still inside, don't retrigger
        if (inCube)
            return;

        inCube = true;
        hasTriggered = true;  // <--- Marks event as completed

        StartCinematic();

        if (playTriggerSound && audioSource != null && triggerSound != null)
            audioSource.PlayOneShot(triggerSound);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(PlayDialogueSequence());
    }

    //private void OnTriggerExit(Collider other)
    //{
    //    if (!other.CompareTag("Player"))
    //        return;

    //    if (!inCube) return;

    //    inCube = false;

    //    // If triggerOnlyOnce is enabled, do NOT reset or interrupt the cinematic
    //    if (!triggerOnlyOnce)
    //        EndCinematic();

    //    subtext.text = "";
    //}

    // ------------------------------
    // Dialogue System
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
    }

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
    // Cinematic Control
    // ------------------------------

    private void StartCinematic()
    {
        cinematicActive = true;

        // Only save camera state if THIS trigger is configured to use the cinematic camera
        if (useCinematicCamera)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                originalCameraRotation = cam.transform.rotation;
                originalCameraPosition = cam.transform.position;
                thisCinematicChangedCamera = true;  // <--- important!
            }
        }
        else
        {
            thisCinematicChangedCamera = false;  // <--- ensures we don't restore on exit
        }

        // Lock player if required
        if (lockPlayer && playerController != null)
            playerController.enabled = false;
    }


    private void EndCinematic()
    {
        cinematicActive = false;

        Camera cam = Camera.main;

        // Only restore if THIS cinematic modified the camera!
        if (thisCinematicChangedCamera && cam != null)
        {
            cam.transform.rotation = originalCameraRotation;
            cam.transform.position = originalCameraPosition;
        }

        // Unlock player
        if (lockPlayer && playerController != null)
            playerController.enabled = true;

        // Reset per-cinematic flag
        thisCinematicChangedCamera = false;
    }

}
