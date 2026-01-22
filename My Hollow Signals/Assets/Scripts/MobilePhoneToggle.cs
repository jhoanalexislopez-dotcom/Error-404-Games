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
    
    [Header("Input System")]
    [Tooltip("Drag the InputAction 'Phone' from your Input Actions here as InputActionReference.")]
    public InputActionReference phoneAction;
    
    private bool isPhoneVisible = false;
    private ScrollRect scrollRect;
    private PauseMenuManager pauseMenuManager;
    private Inventory3DController inventory3DController;
    private NoteUIManager noteUIManager;
    private FlashlightController flashlightController;
    
    public bool IsPhoneVisible => isPhoneVisible;

    void Awake()
    {
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
        inventory3DController = FindObjectOfType<Inventory3DController>();
        noteUIManager = FindObjectOfType<NoteUIManager>(true);
        flashlightController = FindObjectOfType<FlashlightController>();
    }

    void OnEnable()
    {
        if (phoneAction != null && phoneAction.action != null)
            phoneAction.action.Enable();
    }

    void OnDisable()
    {
        if (phoneAction != null && phoneAction.action != null)
            phoneAction.action.Disable();
    }
    
    void Update()
    {
        if (CinematicManager.IsCinematicActive)
        {
            return;
        }
        
        if (pauseMenuManager != null && pauseMenuManager.IsPaused)
        {
            return;
        }
        
        if (inventory3DController != null && inventory3DController.IsInventoryOpen)
        {
            return;
        }
        
        if (noteUIManager != null && noteUIManager.IsNoteActive)
        {
            return;
        }
        
        if (phoneAction != null && phoneAction.action != null &&
            phoneAction.action.WasPressedThisFrame())
        {
            TogglePhone();
        }
    }
    
    private void TogglePhone()
    {
        if (mobileCanvas == null)
        {
            Debug.LogWarning("Mobile Canvas reference is not set in MobilePhoneToggle!");
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
                playerController.SetPlayerInputEnabled(false);
            }
            
            if (flashlightController != null)
            {
                flashlightController.SetFlashlightInputEnabled(false);
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
                playerController.SetPlayerInputEnabled(true);
            }
            
            if (flashlightController != null)
            {
                flashlightController.SetFlashlightInputEnabled(true);
            }
        }
    }

    public void ClosePhone()
    {
        if (isPhoneVisible)
        {
            isPhoneVisible = false;
            if (mobileCanvas != null)
            {
                mobileCanvas.SetActive(false);
            }
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            if (playerController != null)
            {
                playerController.enabled = true;
                playerController.SetPlayerInputEnabled(true);
            }
            
            if (flashlightController != null)
            {
                flashlightController.SetFlashlightInputEnabled(true);
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
