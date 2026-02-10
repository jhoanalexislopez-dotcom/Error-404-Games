/*******************************************************
 * Author: [Ignacio Lopez]
 * Last Modified: [25/01/2026]
 * Description:
 *    Handles interaction UI during cinematics without raycasting.
 *    Shows keyboard/gamepad prompts and sends input to the DialogueSystem.
 *******************************************************/
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class InteractionUICinematic : MonoBehaviour
{
    [Header("UI")]
    public GameObject interactionUI;
    public GameObject keyUI;
    public GameObject buttonUI;
    public TextMeshProUGUI interactionText;
    public GameObject BlackLayoutTransitionUI;

    [Header("Input System")]
    [Tooltip("Arrastra aquí la InputAction 'Interact' o la que uses en tu DialogueSystem.")]
    public InputActionReference interactAction;

    [Header("Dialogue")]
    public DialogueSystem dialogueSystem;

    private bool usingGamepad = false;

    private void OnEnable()
    {
        if (interactAction != null && interactAction.action != null)
            interactAction.action.Enable();

        InputSystem.onActionChange += OnActionChange;
    }

    private void OnDisable()
    {
        if (interactAction != null && interactAction.action != null)
            interactAction.action.Disable();

        InputSystem.onActionChange -= OnActionChange;
    }

    private void Start()
    {
        if (interactionUI != null)
            interactionUI.SetActive(true);

        if (BlackLayoutTransitionUI != null)
            BlackLayoutTransitionUI.SetActive(true);
    }

    private void Update()
    {
        // Mostrar iconos según dispositivo
        if (keyUI != null) keyUI.SetActive(!usingGamepad);
        if (buttonUI != null) buttonUI.SetActive(usingGamepad);

        // Si no hay diálogo, oculta la UI
        if (dialogueSystem != null && !dialogueSystem.IsDialogueActive())
        {
            if (interactionUI != null)
                interactionUI.SetActive(false);
            return;
        }
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
