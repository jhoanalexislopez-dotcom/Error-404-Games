using UnityEngine;
using UnityEngine.InputSystem; // <- Nuevo Input System

public class FlashlightController : MonoBehaviour
{
    [SerializeField] private GameObject flashlightLight;
    private bool flashlightEnabled = false;

    [Header("Input System")]
    [Tooltip("Arrastra aquí la InputAction 'Flashlight' desde tu Input Actions (como InputActionReference).")]
    public InputActionReference flashlightAction;

    void OnEnable()
    {
        if (flashlightAction != null && flashlightAction.action != null)
            flashlightAction.action.Enable();
    }

    void OnDisable()
    {
        if (flashlightAction != null && flashlightAction.action != null)
            flashlightAction.action.Disable();
    }

    void Start()
    {
        flashlightLight.gameObject.SetActive(false);
    }

    void Update()
    {
        if (flashlightAction != null && flashlightAction.action != null &&
            flashlightAction.action.WasPressedThisFrame())
        {
            flashlightEnabled = !flashlightEnabled;
            flashlightLight.gameObject.SetActive(flashlightEnabled);
        }
    }
}
