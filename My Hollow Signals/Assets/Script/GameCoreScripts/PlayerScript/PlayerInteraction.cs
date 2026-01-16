/*******************************************************
 * Author: [Jhoan Alexis Lopez]
 * Last Modified: [21/11/2025]
 * Description:
 *    Handles player interaction with objects in the world using raycasting.
 *******************************************************/
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction")]
    public Camera mainCam;
    public float interactionDistance = 2f;

    [Header("UI")]
    public GameObject interactionUI;
    public GameObject interactionReticle;
    public TextMeshProUGUI interactionText;

    [Header("UI Elements")]
    public GameObject keyUI;    // ← arrastra aquí el objeto "Key"
    public GameObject buttonUI; // ← arrastra aquí el objeto "Button"

    [Header("Input System")]
    [Tooltip("Arrastra aquí la InputAction 'Interact' desde tu Input Actions (como InputActionReference).")]
    public InputActionReference interactAction;

    private bool usingGamepad = false;
    private PauseMenuManager pauseMenuManager;
    private NoteInventoryUI noteInventoryUI;
    private NoteUIManager noteUIManager;
    private MobilePhoneToggle mobilePhoneToggle;

    void Awake()
    {
        pauseMenuManager = FindObjectOfType<PauseMenuManager>();
        noteInventoryUI = FindObjectOfType<NoteInventoryUI>();
        noteUIManager = FindObjectOfType<NoteUIManager>(true);
        mobilePhoneToggle = FindObjectOfType<MobilePhoneToggle>();
    }

    void OnEnable()
    {
        if (interactAction != null && interactAction.action != null)
            interactAction.action.Enable();

        // Detectar cambio de dispositivo
        InputSystem.onActionChange += OnActionChange;
    }

    void OnDisable()
    {
        if (interactAction != null && interactAction.action != null)
            interactAction.action.Disable();

        InputSystem.onActionChange -= OnActionChange;
    }

    void Update()
    {
        InteractionRay();

        // Activar Key o Button según dispositivo
        if (keyUI != null) keyUI.SetActive(!usingGamepad);
        if (buttonUI != null) buttonUI.SetActive(usingGamepad);
    }

    void InteractionRay()
    {
        if (mainCam == null) mainCam = Camera.main;

        Ray ray = mainCam.ViewportPointToRay(Vector3.one / 2f);
        bool hitSomething = false;

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                hitSomething = true;
                if (interactionText != null)
                    interactionText.text = interactable.GetDescription();

                if (interactAction != null && interactAction.action != null &&
                    interactAction.action.WasPressedThisFrame())
                {
                    if (CanInteract())
                    {
                        interactable.Interact();
                    }
                }
            }
        }

        if (interactionUI != null)
            interactionUI.SetActive(hitSomething);
        if (interactionReticle != null)
            interactionReticle.SetActive(hitSomething);
    }

    private bool CanInteract()
    {
        if (pauseMenuManager != null && pauseMenuManager.IsPaused)
        {
            return false;
        }

        if (noteInventoryUI != null && noteInventoryUI.IsInventoryOpen)
        {
            return false;
        }

        if (noteUIManager != null && noteUIManager.IsNoteActive)
        {
            return false;
        }

        if (mobilePhoneToggle != null && mobilePhoneToggle.IsPhoneVisible)
        {
            return false;
        }

        if (CinematicManager.IsCinematicActive)
        {
            return false;
        }

        return true;
    }

    private void OnActionChange(object obj, InputActionChange change)
    {
        if (change == InputActionChange.ActionPerformed)
        {
            if (obj is InputAction action)
            {
                var device = action.activeControl?.device;

                if (device is Gamepad)
                    usingGamepad = true;
                else if (device is Keyboard || device is Mouse)
                    usingGamepad = false;
            }
        }
    }
}
