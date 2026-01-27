/*******************************************************
 * Author: [Ignacio Lopez]
 * Last Modified: [21/11/2025]
 * Description:
 *    Manages the display of collectible notes/documents in the UI.
 *******************************************************/
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;
using UnityEngine.Localization;

public class NoteUIManager : MonoBehaviour
{
    public static event Action OnNoteClosed;
    [Header("UI Settings")]
    [SerializeField] private GameObject notePanel;
    [SerializeField] private TextMeshProUGUI noteText; // Reference to the text component

    [SerializeField] private GameObject flashlight;

    private InputSystem_Actions inputActions;
    private FirstPersonController playerController;
    private PauseMenuManager pauseMenuManager;
    private MobilePhoneToggle mobilePhoneToggle;
    private FlashlightController flashlightController;
    private NoteInventoryUI noteInventoryUI;
    private CursorLockMode originalCursorLockMode;
    private bool originalCursorVisible;
    private bool isNoteActive = false;

    void Awake()
    {
        inputActions = new InputSystem_Actions();
        playerController = FindObjectOfType<FirstPersonController>();
        pauseMenuManager = FindObjectOfType<PauseMenuManager>();
        mobilePhoneToggle = FindObjectOfType<MobilePhoneToggle>();
        flashlightController = FindObjectOfType<FlashlightController>();
        noteInventoryUI = FindObjectOfType<NoteInventoryUI>();

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
    public void SetNoteActive(LocalizedString localizedNoteText)
    {
        string noteTextString = localizedNoteText != null && !localizedNoteText.IsEmpty 
            ? localizedNoteText.GetLocalizedString() 
            : "";
            
        Debug.Log($"[NoteUIManager] SetNoteActive called with localized text length: {noteTextString?.Length ?? 0}");
        Debug.Log($"[NoteUIManager] MobilePhoneToggle found: {mobilePhoneToggle != null}");
        if (mobilePhoneToggle != null)
        {
            Debug.Log($"[NoteUIManager] MobilePhoneToggle.IsPhoneVisible: {mobilePhoneToggle.IsPhoneVisible}");
        }
        
        if (mobilePhoneToggle != null && mobilePhoneToggle.IsPhoneVisible)
        {
            Debug.Log("[NoteUIManager] Note blocked: Phone is visible");
            return;
        }
        
        isNoteActive = true;
        Debug.Log($"[NoteUIManager] isNoteActive set to TRUE");

        if (!string.IsNullOrEmpty(noteTextString) && this.noteText != null)
        {
            this.noteText.text = noteTextString;
        }

        Debug.Log("Note manually set as active with text: " + noteTextString);

        // Store cursor state
        originalCursorLockMode = Cursor.lockState;
        originalCursorVisible = Cursor.visible;

        // Unlock cursor for note reading
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Disable player controller
        if (playerController != null)
        {
            playerController.enabled = false;
            playerController.SetPlayerInputEnabled(false);
        }

        // Disable pause menu while note is active
        if (pauseMenuManager != null)
        {
            pauseMenuManager.enabled = false;
        }
        
        // Disable flashlight controller while note is active
        if (flashlightController != null)
        {
            flashlightController.SetFlashlightInputEnabled(false);
        }
        
        // Desactivar mesh renderer de flashlight
        if (flashlight != null)
        {
            MeshRenderer mesh = flashlight.GetComponent<MeshRenderer>();
            if (mesh == null)
                mesh = flashlight.GetComponentInChildren<MeshRenderer>();

            if (mesh != null)
                mesh.enabled = false;
        }

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

        // Resume the game manually
        Time.timeScale = 1f;
        
        // Restore cursor state
        Cursor.lockState = originalCursorLockMode;
        Cursor.visible = originalCursorVisible;

        // Re-enable player controller
        if (playerController != null)
        {
            playerController.enabled = true;
            playerController.SetPlayerInputEnabled(true);
            
            Debug.Log("Player controller re-enabled");
        }

        // Re-enable the pause menu
        if (pauseMenuManager != null)
        {
            pauseMenuManager.enabled = true;
        }
        
        // Re-enable flashlight controller
        if (flashlightController != null)
        {
            flashlightController.SetFlashlightInputEnabled(true);
        }

        Debug.Log("Note closed - NoteUI disabled and game resumed");
        
        // Activar mesh renderer de flashlight
        if (flashlight != null)
        {
            MeshRenderer mesh = flashlight.GetComponent<MeshRenderer>();
            if (mesh == null)
                mesh = flashlight.GetComponentInChildren<MeshRenderer>();

            if (mesh != null)
                mesh.enabled = true;
        }
        
        OnNoteClosed?.Invoke();
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
