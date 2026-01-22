using System.Collections;
using UnityEngine;

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

    private AudioSource audioSource;
    private GameObject spawnedEnemy;
    private bool isJumpscareActive = false;
    private bool headBobWasEnabled;
    private bool gameManagerWasEnabled;
    
    private Vector3 originalCameraPosition;
    private Coroutine shakeCoroutine;

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

        isJumpscareActive = false;
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

    private void HideUI()
    {
        if (mainCanvas != null)
        {
            mainCanvas.SetActive(false);
        }

        if (layoutCanvas != null)
        {
            layoutCanvas.SetActive(false);
        }

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
                spawnedEnemy.transform.Rotate(0, 180f, 0);
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
        EnablePlayerInput();
    }

    public bool IsJumpscareActive
    {
        get { return isJumpscareActive; }
    }
}
