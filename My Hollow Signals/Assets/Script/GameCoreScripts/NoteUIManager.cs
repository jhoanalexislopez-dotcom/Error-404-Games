/*******************************************************
 * Author: [Ignacio Lopez]
 * Last Modified: [21/11/2025]
 * Description:
 *    Manages the display of collectible notes/documents in the UI.
 *******************************************************/
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class NoteUIManager : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private GameObject notePanel;
    [SerializeField] private TextMeshProUGUI noteText; // Reference to the text component

    private InputSystem_Actions inputActions;
    private FirstPersonController playerController;
    private PauseMenuManager pauseMenuManager;
    private CursorLockMode originalCursorLockMode;
    private bool originalCursorVisible;
    private bool isNoteActive = false;

    void Awake()
    {
        inputActions = new InputSystem_Actions();
        playerController = FindObjectOfType<FirstPersonController>();
        pauseMenuManager = FindObjectOfType<PauseMenuManager>();

        // Find the note text component if not assigned
        if (noteText == null && notePanel != null)
        {
            noteText = notePanel.GetComponentInChildren<TextMeshProUGUI>();
        }

        // Store original cursor settings
        originalCursorLockMode = Cursor.lockState;
        originalCursorVisible = Cursor.visible;
    }

    void OnEnable()
    {
        inputActions?.Player.Enable();

        // Subscribe only to button presses, NOT movement or look actions
        inputActions.Player.Attack.performed += OnAnyInput;
        inputActions.Player.Interact.performed += OnAnyInput;
        inputActions.Player.Crouch.performed += OnAnyInput;
        inputActions.Player.Jump.performed += OnAnyInput;
        inputActions.Player.Previous.performed += OnAnyInput;
        inputActions.Player.Next.performed += OnAnyInput;
        inputActions.Player.Sprint.performed += OnAnyInput;
        inputActions.Player.Walk.performed += OnAnyInput;
        inputActions.Player.Flashlight.performed += OnAnyInput;
        inputActions.Player.Pause.performed += OnAnyInput;
    }

    void OnDisable()
    {
        inputActions?.Player.Disable();

        if (inputActions != null)
        {
            inputActions.Player.Attack.performed -= OnAnyInput;
            inputActions.Player.Interact.performed -= OnAnyInput;
            inputActions.Player.Crouch.performed -= OnAnyInput;
            inputActions.Player.Jump.performed -= OnAnyInput;
            inputActions.Player.Previous.performed -= OnAnyInput;
            inputActions.Player.Next.performed -= OnAnyInput;
            inputActions.Player.Sprint.performed -= OnAnyInput;
            inputActions.Player.Walk.performed -= OnAnyInput;
            inputActions.Player.Flashlight.performed -= OnAnyInput;
            inputActions.Player.Pause.performed -= OnAnyInput;
        }
    }

    void Update()
    {
        // Check if notePanel became active and update our state accordingly
        if (notePanel != null && notePanel.activeInHierarchy && !isNoteActive)
        {
            isNoteActive = true;
            Debug.Log("Note detected as active - NoteUIManager state updated");
        }
        else if (notePanel != null && !notePanel.activeInHierarchy && isNoteActive)
        {
            isNoteActive = false;
            Debug.Log("Note detected as inactive - NoteUIManager state updated");
        }
    }

    void OnAnyInput(InputAction.CallbackContext context)
    {
        // Only respond to input if the note is currently active
        if (isNoteActive && notePanel.activeInHierarchy)
        {
            CloseNote();
        }
    }

    void Start()
    {
        // Check if note is already active when scene starts
        if (notePanel != null && notePanel.activeInHierarchy)
        {
            isNoteActive = true;
        }
    }

    // Public method for Collectible to set note text and make it active
    public void SetNoteActive(string noteText = "")
    {
        isNoteActive = true;

        // Set the note text if provided
        if (!string.IsNullOrEmpty(noteText) && this.noteText != null)
        {
            this.noteText.text = noteText;
        }

        Debug.Log("Note manually set as active with text: " + noteText);
    }

    // Public method to set just the text without changing active state
    public void SetNoteText(string text)
    {
        if (noteText != null)
        {
            noteText.text = text;
            Debug.Log("Note text set to: " + text);
        }
        else
        {
            Debug.LogWarning("Note text component is not assigned!");
        }
    }

    public void CloseNote()
    {
        if (!isNoteActive && !notePanel.activeInHierarchy) return;

        isNoteActive = false;

        // Explicitly disable the note UI
        if (notePanel != null)
        {
            notePanel.SetActive(false);
            Debug.Log("NoteUI disabled");
        }

        // Use PauseMenuManager to resume the game
        if (pauseMenuManager != null)
        {
            pauseMenuManager.ResumeGame();

            // Re-enable the pause menu
            pauseMenuManager.enabled = true;
        }

        Debug.Log("Note closed - NoteUI disabled and game resumed via PauseMenuManager");
    }

    void OnDestroy()
    {
        // Ensure game is resumed if this object is destroyed while note is active
        if (isNoteActive && pauseMenuManager != null)
        {
            pauseMenuManager.ResumeGame();
            pauseMenuManager.enabled = true;
        }

        inputActions?.Dispose();
    }

    // Public property to check if note is currently active
    public bool IsNoteActive => isNoteActive;
}
