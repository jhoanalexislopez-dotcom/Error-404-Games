using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class MobilePhoneToggle : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject mobileCanvas;
    
    [Header("Player References")]
    [SerializeField] private FirstPersonController playerController;
    
    private InputSystem_Actions inputActions;
    private bool isPhoneVisible = false;
    private ScrollRect scrollRect;
    private PauseMenuManager pauseMenuManager;
    private NoteInventoryUI noteInventoryUI;
    private NoteUIManager noteUIManager;
    
    public bool IsPhoneVisible => isPhoneVisible;

    void Awake()
    {
        inputActions = new InputSystem_Actions();
        
        inputActions.Player.Phone.performed += ctx => TogglePhone();
        
        if (mobileCanvas != null)
        {
            mobileCanvas.SetActive(isPhoneVisible);
            scrollRect = mobileCanvas.GetComponentInChildren<ScrollRect>();
        }
        
        if (playerController == null)
        {
            playerController = GetComponent<FirstPersonController>();
        }
        
        pauseMenuManager = FindObjectOfType<PauseMenuManager>();
        noteInventoryUI = FindObjectOfType<NoteInventoryUI>();
        noteUIManager = FindObjectOfType<NoteUIManager>(true);
        
        if (noteUIManager == null)
        {
            Debug.LogWarning("NoteUIManager not found in MobilePhoneToggle!");
        }
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
    }

    void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void TogglePhone()
    {
        if (mobileCanvas == null)
        {
            Debug.LogWarning("Mobile Canvas reference is not set in MobilePhoneToggle!");
            return;
        }
        
        if (pauseMenuManager != null && pauseMenuManager.IsPaused)
        {
            Debug.Log("Phone blocked: Pause menu is open");
            return;
        }
        
        if (noteInventoryUI != null && noteInventoryUI.IsInventoryOpen)
        {
            Debug.Log("Phone blocked: Inventory is open");
            return;
        }
        
        if (noteUIManager != null && noteUIManager.IsNoteActive)
        {
            Debug.Log("Phone blocked: Note UI is active");
            return;
        }
        
        if (CinematicManager.IsCinematicActive)
        {
            Debug.Log("Phone blocked: Cinematic is active");
            return;
        }
        
        isPhoneVisible = !isPhoneVisible;
        mobileCanvas.SetActive(isPhoneVisible);
        
        if (isPhoneVisible)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            if (playerController != null)
            {
                playerController.enabled = false;
            }
            
            StartCoroutine(ScrollToBottomOnOpen());
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            if (playerController != null)
            {
                playerController.enabled = true;
            }
        }
    }
    
    private IEnumerator ScrollToBottomOnOpen()
    {
        yield return new WaitForEndOfFrame();
        
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
