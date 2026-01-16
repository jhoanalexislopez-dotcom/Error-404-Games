/*******************************************************
 * Author: [Ignacio Lopez]
 * Last Modified: [21/11/2025]
 * Description:
 *    Handles pause menu functionality including pausing/resuming the game, cursor management, and menu navigation.
 *******************************************************/
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuUI;
    public Button resumeButton;
    public Button optionsButton;
    public Button quitButton;

    [Header("Settings")]
    public bool pauseOnStart = false;

    private bool isPaused = false;
    private InputSystem_Actions inputActions;
    private FirstPersonController playerController;
    private GameManager gameManager;
    private MobilePhoneToggle mobilePhoneToggle;

    // Store original cursor state
    private CursorLockMode originalCursorLockMode;
    private bool originalCursorVisible;

    void Awake()
    {
        // Initialize input actions
        inputActions = new InputSystem_Actions();

        // Subscribe to pause action
        inputActions.Player.Pause.performed += OnPausePressed;

        // Find player components
        playerController = FindObjectOfType<FirstPersonController>();
        gameManager = FindObjectOfType<GameManager>();
        mobilePhoneToggle = FindObjectOfType<MobilePhoneToggle>();

        // Set up button events
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (optionsButton != null)
            optionsButton.onClick.AddListener(OpenOptions);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitToMainMenu);
    }

    void Start()
    {
        // Store original cursor settings
        originalCursorLockMode = Cursor.lockState;
        originalCursorVisible = Cursor.visible;

        // Ensure pause menu is hidden at start
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        // Pause on start if needed (for testing)
        if (pauseOnStart)
            PauseGame();
    }

    void OnEnable()
    {
        inputActions?.Player.Enable();
    }

    void OnDisable()
    {
        inputActions?.Player.Disable();
    }

    void Update()
    {

    }

    void OnPausePressed(InputAction.CallbackContext context)
    {
        TogglePause();
    }

    public void TogglePause()
    {
        if (mobilePhoneToggle != null && mobilePhoneToggle.IsPhoneVisible)
        {
            return;
        }
        
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;

        // Pause time
        Time.timeScale = 0f;

        // Show pause menu
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);

            // Select resume button for controller navigation
            if (resumeButton != null)
                resumeButton.Select();
        }

        // Unlock and show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Disable player input
        if (playerController != null && playerController.enabled)
        {
            var playerInput = playerController.GetComponent<PlayerInput>();
            if (playerInput != null)
                playerInput.enabled = false;
            else
                playerController.enabled = false;
        }

        Debug.Log("Game Paused");
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;

        // Resume time
        Time.timeScale = 1f;

        // Hide pause menu
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        // Restore cursor settings
        Cursor.lockState = originalCursorLockMode;
        Cursor.visible = originalCursorVisible;

        // Re-enable player input
        if (playerController != null)
        {
            var playerInput = playerController.GetComponent<PlayerInput>();
            if (playerInput != null)
                playerInput.enabled = true;
            else
                playerController.enabled = true;
        }

        Debug.Log("Game Resumed");
    }

    public void OpenOptions()
    {
        SensitivitySettingsManager settingsManager = FindObjectOfType<SensitivitySettingsManager>();
        if (settingsManager != null)
        {
            settingsManager.OpenSettings();
            Debug.Log("Opening sensitivity settings from pause menu");
        }
        else
        {
            Debug.LogWarning("SensitivitySettingsManager not found! Please add it to your scene.");
        }
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        
        DestroyPersistentObjects();
        
        SceneManager.LoadScene("MainMenu");
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

    void OnDestroy()
    {
        // Clean up input actions
        if (inputActions != null)
        {
            inputActions.Player.Pause.performed -= OnPausePressed;
            inputActions.Dispose();
        }
    }

    // Public properties for other scripts to check pause state
    public bool IsPaused => isPaused;

    // Method to force pause/resume from other script
    public void SetPauseState(bool pause)
    {
        if (pause)
            PauseGame();
        else
            ResumeGame();
    }
}
