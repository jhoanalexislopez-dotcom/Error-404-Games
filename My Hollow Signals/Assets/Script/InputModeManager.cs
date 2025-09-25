using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputModeManager : MonoBehaviour
{
    [Header("Input Detection Settings")]
    [SerializeField] private float mouseMovementThreshold = 0.1f;
    [SerializeField] private bool debugInputMode = false;

    private Vector2 lastMousePosition;
    private bool isGamepadMode = false;
    private InputSystem_Actions inputActions;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        lastMousePosition = Mouse.current?.position.ReadValue() ?? Vector2.zero;
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void Update()
    {
        CheckForMouseMovement();
        CheckForGamepadInput();
    }

    private void CheckForMouseMovement()
    {
        if (Mouse.current == null) return;

        Vector2 currentMousePosition = Mouse.current.position.ReadValue();
        float mouseDelta = Vector2.Distance(currentMousePosition, lastMousePosition);

        if (mouseDelta > mouseMovementThreshold)
        {
            if (isGamepadMode)
            {
                SwitchToMouseMode();
            }
            lastMousePosition = currentMousePosition;
        }
    }

    private void CheckForGamepadInput()
    {
        if (Gamepad.current == null) return;

        // Check for any gamepad navigation input
        Vector2 navigation = inputActions.UI.Navigate.ReadValue<Vector2>();
        bool submitPressed = inputActions.UI.Submit.WasPressedThisFrame();
        bool cancelPressed = inputActions.UI.Cancel.WasPressedThisFrame();

        if (navigation.magnitude > 0.1f || submitPressed || cancelPressed)
        {
            if (!isGamepadMode)
            {
                SwitchToGamepadMode();
            }
        }
    }

    private void SwitchToMouseMode()
    {
        isGamepadMode = false;

        // Clear the current selection to allow pure mouse interaction
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        if (debugInputMode)
            Debug.Log("Switched to Mouse Mode");
    }

    private void SwitchToGamepadMode()
    {
        isGamepadMode = true;

        // Find a selectable object to highlight if none is currently selected
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
        {
            SetDefaultSelectedObject();
        }

        if (debugInputMode)
            Debug.Log("Switched to Gamepad Mode");
    }

    private void SetDefaultSelectedObject()
    {
        // Try to find the first active selectable in the scene
        UnityEngine.UI.Selectable firstSelectable = FindFirstActiveSelectable();
        if (firstSelectable != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectable.gameObject);
        }
    }

    private UnityEngine.UI.Selectable FindFirstActiveSelectable()
    {
        // Look for active selectables in order of preference
        UnityEngine.UI.Selectable[] selectables = FindObjectsOfType<UnityEngine.UI.Selectable>();

        foreach (var selectable in selectables)
        {
            if (selectable.gameObject.activeInHierarchy && selectable.interactable)
            {
                return selectable;
            }
        }

        return null;
    }

    public bool IsGamepadMode => isGamepadMode;

    public void ForceGamepadMode()
    {
        SwitchToGamepadMode();
    }

    public void ForceMouseMode()
    {
        SwitchToMouseMode();
    }

    private void OnDestroy()
    {
        inputActions?.Dispose();
    }
}
