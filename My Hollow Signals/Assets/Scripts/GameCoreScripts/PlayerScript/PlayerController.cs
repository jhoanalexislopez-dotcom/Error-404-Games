/*******************************************************
 * Author: [Jhoan Alexis Lopez]
 * Last Modified: [21/11/2025]
 * Description:
 *    Main first-person controller handling movement, looking, crouching, and player physics.
 *******************************************************/
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("UI")]
    public GameObject crouchIcon;

    [Header("Movimiento")]
    public float walkSpeed = 2.5f;
    public float runSpeed = 4f;
    public float crouchSpeed = 1.5f;
    public float gravity = -9.81f;
    public float crouchHeight = 1f;
    public float crouchCameraOffset = 0.5f;
    public float crouchSmoothTime = 0.2f;
    private float originalHeight;
    private Vector3 originalCameraPos;

    [Header("Cinemachine")]
    public Transform cameraRoot;
    [Tooltip("Sensibilidad al usar rat�n")]
    public float mouseSensitivity = 1f;
    [Tooltip("Sensibilidad al usar gamepad")]
    public float gamepadSensitivity = 3f;
    private float currentSensitivity;
    private float xRotation = 0f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    // Input System
    private InputSystem_Actions inputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool runPressed;
    private bool crouchPressed;

    // �ltimo dispositivo usado
    private InputDevice lastUsedDevice;

    // Public accessors for actual movement state (after mutual exclusion logic)
    public bool IsCrouching => crouchPressed;
    public bool IsRunning => runPressed && !crouchPressed && isGrounded;
    public bool IsMoving => moveInput.magnitude > 0.01f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        originalHeight = controller.height;
        originalCameraPos = cameraRoot.localPosition;

        inputActions = new InputSystem_Actions();

        // --- Movimiento ---
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        // --- Mirada ---
        inputActions.Player.Look.performed += ctx =>
        {
            if (CinematicManager.IsCinematicActive)
            {
                lookInput = Vector2.zero;
                return;
            }
            
            lookInput = ctx.ReadValue<Vector2>();
            lastUsedDevice = ctx.control.device; // Guardamos el dispositivo

            if (lastUsedDevice is Mouse)
                currentSensitivity = mouseSensitivity;
            else if (lastUsedDevice is Gamepad)
                currentSensitivity = gamepadSensitivity;
        };
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        // --- Correr (Shift) ---
        inputActions.Player.Walk.performed += ctx => 
        { 
            // Cannot run while crouching
            if (!crouchPressed)
                runPressed = true;
            lastUsedDevice = ctx.control.device;
        };
        inputActions.Player.Walk.canceled += ctx => runPressed = false;

        // --- Agacharse ---
        inputActions.Player.Crouch.performed += ctx => 
        { 
            crouchPressed = true;
            // Cancel run when crouching
            runPressed = false;
            lastUsedDevice = ctx.control.device;
        };
        inputActions.Player.Crouch.canceled += ctx => crouchPressed = false;
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
        Cursor.lockState = CursorLockMode.Locked;

        LoadSensitivitySettings();
    }

    void LoadSensitivitySettings()
    {
        mouseSensitivity = SensitivitySettingsManager.GetMouseSensitivity();
        gamepadSensitivity = SensitivitySettingsManager.GetGamepadSensitivity();

        currentSensitivity = mouseSensitivity;
    }

    void OnDisable()
    {
        inputActions.Player.Disable();
    }
    
    public void ResetLookInput()
    {
        lookInput = Vector2.zero;
    }
    
    public void SetPlayerInputEnabled(bool enabled)
    {
        if (enabled)
        {
            inputActions.Player.Enable();
        }
        else
        {
            inputActions.Player.Disable();
        }
    }

    void Start()
    {
        // Forzar c�mara mirando al frente
        xRotation = 0f;

        if (cameraRoot != null)
            cameraRoot.localRotation = Quaternion.Euler(90f, 0f, 0f);
    }

    void Update()
    {
        if (CinematicManager.IsCinematicActive)
            return;
            
        // --- Ground Check ---
        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // --- Determinar velocidad actual ---
        // Priority: Crouch > Run > Walk (cannot run while crouching)
        float currentSpeed;
        if (crouchPressed)
        {
            currentSpeed = crouchSpeed;
        }
        else if (runPressed && isGrounded)
        {
            currentSpeed = runSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }

        // --- Movimiento ---
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // --- Agacharse ---
        float targetHeight = crouchPressed ? crouchHeight : originalHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * 10f);

        // --- C�mara suavizada al agacharse ---
        Vector3 targetCameraPos = crouchPressed
            ? originalCameraPos + Vector3.down * crouchCameraOffset
            : originalCameraPos;
        cameraRoot.localPosition = Vector3.Lerp(cameraRoot.localPosition, targetCameraPos, Time.deltaTime / crouchSmoothTime);

        // --- Gravedad ---
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // --- Rotaci�n de c�mara ---
        // Make sensitivity frame rate independent
        float frameRateMultiplier = Time.unscaledDeltaTime * 60f; // Normalize to 60 FPS
        float mouseX = lookInput.x * currentSensitivity * frameRateMultiplier;
        float mouseY = lookInput.y * currentSensitivity * frameRateMultiplier;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraRoot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);

        // --- UI: activar crouchIcon solo si se usa gamepad ---
        if (lastUsedDevice is Gamepad)
            crouchIcon.SetActive(crouchPressed);
        else
            crouchIcon.SetActive(false);
    }
}
