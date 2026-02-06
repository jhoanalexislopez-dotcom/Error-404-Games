using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JumpscareManager : MonoBehaviour
{
    [Header("Jumpscare Settings")]
    [SerializeField] private GameObject jumpscareEnemyPrefab;
    [SerializeField] private AudioClip screamSound;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float jumpscareDistance = 2f;
    [SerializeField] private float jumpscareDuration = 3f;
    [SerializeField] private float enemyJumpSpeed = 5f;

    [Header("Camera Shake")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float shakeIntensity = 0.3f;
    [SerializeField] private float shakeFrequency = 25f;

    [Header("UI References")]
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private GameObject layoutCanvas;
    [SerializeField] private GameObject mobileCanvas;

    [Header("Player References")]
    [SerializeField] private FirstPersonController playerController;
    [SerializeField] private HeadBob headBobController;
    [SerializeField] private GameManager gameManager;

    [Header("UI Manager References")]
    [SerializeField] private PauseMenuManager pauseMenuManager;
    [SerializeField] private MobilePhoneToggle mobilePhoneToggle;
    [SerializeField] private Inventory3DController inventory3DController;
    [SerializeField] private NoteUIManager noteUIManager;

    [Header("Scene Transition")]
    [SerializeField] private float onInsaneScreenDuration = 3f;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Insanity Effects")]
    [SerializeField] private InsanityEffectsManager insanityEffectsManager;
    [SerializeField] private SanityAudioController sanityAudioController;

    private AudioSource audioSource;
    private GameObject spawnedEnemy;
    private bool isJumpscareActive = false;
    private bool headBobWasEnabled;
    private bool gameManagerWasEnabled;
    
    private Vector3 originalCameraPosition;
    private Coroutine shakeCoroutine;

    private bool wasPauseMenuOpen;
    private bool wasMobilePhoneOpen;
    private bool wasInventoryOpen;
    private bool wasNoteOpen;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        if (cameraTransform != null)
        {
            originalCameraPosition = cameraTransform.localPosition;
        }
        else
        {
            Debug.LogWarning("Camera Transform reference not set! Camera shake will not work.");
        }
    }

    public void TriggerJumpscare()
    {
        if (isJumpscareActive)
        {
            return;
        }

        StartCoroutine(JumpscareSequence());
    }

    private IEnumerator JumpscareSequence()
    {
        isJumpscareActive = true;

        if (sanityAudioController != null)
        {
            sanityAudioController.ResetEffectsAfterJumpscare();
        }

        CloseAllMenus();
        DisablePlayerInput();
        HideUI();

        yield return new WaitForSeconds(0.2f);

        SpawnEnemyInFrontOfPlayer();

        yield return new WaitForSeconds(0.1f);

        PlayScreamerSound();
        StartCameraShake();

        yield return new WaitForSeconds(jumpscareDuration);

        StopCameraShake();

        CleanupJumpscare();

        if (insanityEffectsManager != null)
        {
            insanityEffectsManager.ActivateInsaneScreen();
        }

        yield return new WaitForSeconds(onInsaneScreenDuration);

        LoadMainMenu();
    }

    private void DisablePlayerInput()
    {
        if (playerController != null)
        {
            playerController.SetPlayerInputEnabled(false);
        }

        if (headBobController != null)
        {
            headBobWasEnabled = headBobController.enabled;
            headBobController.enabled = false;
        }

        if (gameManager != null)
        {
            gameManagerWasEnabled = gameManager.enabled;
            gameManager.enabled = false;
        }

        DisableAllMenuInputs();
    }

    private void EnablePlayerInput()
    {
        if (playerController != null)
        {
            playerController.SetPlayerInputEnabled(true);
        }

        if (headBobController != null)
        {
            headBobController.enabled = headBobWasEnabled;
        }

        if (gameManager != null)
        {
            gameManager.enabled = gameManagerWasEnabled;
        }
    }

    private void CloseAllMenus()
    {
        wasPauseMenuOpen = false;
        wasMobilePhoneOpen = false;
        wasInventoryOpen = false;
        wasNoteOpen = false;

        if (pauseMenuManager != null && pauseMenuManager.IsPaused)
        {
            wasPauseMenuOpen = true;
            pauseMenuManager.ResumeGame();
        }

        if (mobilePhoneToggle != null && mobilePhoneToggle.IsPhoneVisible)
        {
            wasMobilePhoneOpen = true;
            mobilePhoneToggle.ClosePhone();
        }

        if (inventory3DController != null && inventory3DController.IsInventoryOpen)
        {
            wasInventoryOpen = true;
            inventory3DController.CloseInventory();
        }

        if (noteUIManager != null && noteUIManager.IsNoteActive)
        {
            wasNoteOpen = true;
            noteUIManager.CloseNote();
        }
    }

    private void DisableAllMenuInputs()
    {
        if (pauseMenuManager != null)
        {
            pauseMenuManager.enabled = false;
        }

        if (mobilePhoneToggle != null)
        {
            mobilePhoneToggle.enabled = false;
        }

        if (inventory3DController != null)
        {
            inventory3DController.enabled = false;
        }

        if (noteUIManager != null)
        {
            noteUIManager.enabled = false;
        }
    }
    private void DestroyPersistentObjects()
    {
        GameManager gameManagerInstance = FindObjectOfType<GameManager>();
        if (gameManagerInstance != null)
        {
            Destroy(gameManagerInstance.gameObject);
        }

        CinematicManager cinematicManagerInstance = FindObjectOfType<CinematicManager>();
        if (cinematicManagerInstance != null)
        {
            Destroy(cinematicManagerInstance.gameObject);
        }
    }
    private void LoadMainMenu()
    {
        DestroyPersistentObjects();

        // Unlock and show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void HideUI()
    {
        if (mainCanvas != null)
        {
            mainCanvas.SetActive(false);
        }

        //if (layoutCanvas != null)
        //{
        //    layoutCanvas.SetActive(false);
        //}

        if (mobileCanvas != null)
        {
            mobileCanvas.SetActive(false);
        }
    }

    private void ShowUI()
    {
        if (mainCanvas != null)
        {
            mainCanvas.SetActive(true);
        }

        if (layoutCanvas != null)
        {
            layoutCanvas.SetActive(true);
        }
    }

    private void SpawnEnemyInFrontOfPlayer()
    {
        if (jumpscareEnemyPrefab == null || playerCamera == null)
        {
            Debug.LogWarning("Jumpscare enemy prefab or player camera not assigned!");
            return;
        }

        Vector3 spawnPosition = playerCamera.position + playerCamera.forward * jumpscareDistance;
        spawnedEnemy = Instantiate(jumpscareEnemyPrefab, spawnPosition, Quaternion.identity);

        spawnedEnemy.transform.LookAt(playerCamera);
        spawnedEnemy.transform.Rotate(0, 180f, 0);

        StartCoroutine(AnimateEnemyJump());
    }

    private IEnumerator AnimateEnemyJump()
    {
        if (spawnedEnemy == null || playerCamera == null)
        {
            yield break;
        }

        Vector3 startPosition = spawnedEnemy.transform.position;
        Vector3 targetPosition = playerCamera.position + playerCamera.forward * 0.5f;

        float elapsed = 0f;
        float jumpDuration = 0.3f;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / jumpDuration;

            if (spawnedEnemy != null)
            {
                spawnedEnemy.transform.position = Vector3.Lerp(startPosition, targetPosition, t * enemyJumpSpeed);
                spawnedEnemy.transform.LookAt(playerCamera);
                //spawnedEnemy.transform.Rotate(0, 180f, 0);
            }

            yield return null;
        }
    }

    private void PlayScreamerSound()
    {
        if (screamSound != null && audioSource != null)
        {
            audioSource.clip = screamSound;
            audioSource.Play();
        }
    }

    private void StartCameraShake()
    {
        if (cameraTransform != null)
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
            }
            shakeCoroutine = StartCoroutine(CameraShakeCoroutine());
        }
    }

    private void StopCameraShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }

        if (cameraTransform != null)
        {
            cameraTransform.localPosition = originalCameraPosition;
        }
    }

    private IEnumerator CameraShakeCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < jumpscareDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;
            float z = Random.Range(-1f, 1f) * shakeIntensity;

            cameraTransform.localPosition = originalCameraPosition + new Vector3(x, y, z);

            elapsed += Time.deltaTime;
            yield return new WaitForSeconds(1f / shakeFrequency);
        }

        cameraTransform.localPosition = originalCameraPosition;
    }

    private void CleanupJumpscare()
    {
        if (spawnedEnemy != null)
        {
            Destroy(spawnedEnemy);
        }

        ShowUI();
    }

    public bool IsJumpscareActive
    {
        get { return isJumpscareActive; }
    }
}
