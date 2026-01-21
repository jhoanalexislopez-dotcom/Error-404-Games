/*******************************************************
 * Author: [Ignacio Lopez]
 * Last Modified: [21/11/2025]
 * Description:
 *    Detects and manages input mode switching between keyboard/mouse and gamepad, automatically updating UI prompts and cursor visibility.
 *******************************************************/
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputModeManager : MonoBehaviour
{
    [Header("Input Detection Settings")]
    [SerializeField] private float mouseMovementThreshold = 0.1f;
    [SerializeField] private bool debugInputMode = false;

    [Header("UI Controls")]
    [SerializeField] private GameObject controlsImageGamepad;
    [SerializeField] private GameObject controlsImageKeyboard;

    private Vector2 lastMousePosition;
    private bool isGamepadMode = false;
    private InputSystem_Actions inputActions;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        lastMousePosition = Mouse.current?.position.ReadValue() ?? Vector2.zero;

        UpdateUI(); // Mostrar la imagen correcta al iniciar
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

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        UpdateUI();

        if (debugInputMode)
            Debug.Log("Switched to Mouse Mode");
    }

    private void SwitchToGamepadMode()
    {
        isGamepadMode = true;

        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
        {
            SetDefaultSelectedObject();
        }

        UpdateUI();

        if (debugInputMode)
            Debug.Log("Switched to Gamepad Mode");
    }

    private void SetDefaultSelectedObject()
    {
        UnityEngine.UI.Selectable firstSelectable = FindFirstActiveSelectable();
        if (firstSelectable != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectable.gameObject);
        }
    }

    private UnityEngine.UI.Selectable FindFirstActiveSelectable()
    {
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

    private void UpdateUI()
    {
        if (controlsImageGamepad != null)
            controlsImageGamepad.SetActive(isGamepadMode);

        if (controlsImageKeyboard != null)
            controlsImageKeyboard.SetActive(!isGamepadMode);
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
