/*******************************************************
 * Author: [Bianca Marinica]
 * Last Modified: [21/11/2025]
 * Description:
 *    Controls flashlight on/off state, likely handles input for toggling and manages light intensity/battery if applicable.
 *******************************************************/
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem; // <- Nuevo Input System

public class FlashlightController : MonoBehaviour
{
    [SerializeField] private GameObject flashlightLight;
    private bool flashlightEnabled = false;

    [Header("Input System")]
    [Tooltip("Arrastra aqu� la InputAction 'Flashlight' desde tu Input Actions (como InputActionReference).")]
    public InputActionReference flashlightAction;

    [SerializeField] public float battery = 100f;
    [SerializeField] public int consume = 10;

    public void RechargeBattery()
    {
        battery = 100f;

        GameManager audioManager = FindAnyObjectByType<GameManager>();
        audioManager.PlayFlashlightRecharge();

    }

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
        if (CinematicManager.IsCinematicActive)
        {
            return;
        }

        if (flashlightAction != null && flashlightAction.action != null &&
            flashlightAction.action.WasPressedThisFrame())
        {
            flashlightEnabled = !flashlightEnabled;
            flashlightLight.gameObject.SetActive(flashlightEnabled);
        }
        if (flashlightEnabled) {
            battery -= consume*Time.deltaTime;

            battery = Mathf.Clamp(battery, 0, 100);
        }
        if (battery <= 0)
        {
            flashlightEnabled = false;
            flashlightLight.gameObject.SetActive(false);
        }
    }
}
