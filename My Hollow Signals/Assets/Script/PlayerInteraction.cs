using UnityEngine;
using TMPro;
using UnityEngine.InputSystem; // <- Nuevo input system

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction")]
    public Camera mainCam;
    public float interactionDistance = 2f;

    [Header("UI")]
    public GameObject interactionUI;
    public TextMeshProUGUI interactionText;

    [Header("Input System")]
    [Tooltip("Arrastra aquí la InputAction 'Interact' desde tu Input Actions (como InputActionReference).")]
    public InputActionReference interactAction; // Debe apuntar a la acción llamada "Interact"

    void OnEnable()
    {
        // Asegura que la acción esté habilitada cuando el objeto se active
        if (interactAction != null && interactAction.action != null)
            interactAction.action.Enable();
    }

    void OnDisable()
    {
        // Deshabilita para evitar escuchas fantasma
        if (interactAction != null && interactAction.action != null)
            interactAction.action.Disable();
    }

    void Update()
    {
        InteractionRay();
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

                // --- Nuevo Input System ---
                // Usamos polling para detectar el "press" de la acción "Interact"
                if (interactAction != null && interactAction.action != null &&
                    interactAction.action.WasPressedThisFrame())
                {
                    interactable.Interact();
                }
            }
        }

        if (interactionUI != null)
            interactionUI.SetActive(hitSomething);
    }
}
