using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movimiento")]
    public float walkSpeed = 5f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;
    public float crouchHeight = 1f; // Altura al agacharse
    public float crouchCameraOffset = 0.5f; // Cuánto baja la cámara al agacharse
    public float crouchSmoothTime = 0.2f; // Velocidad de interpolación
    private float originalHeight;
    private Vector3 originalCameraPos;

    [Header("Cinemachine")]
    public Transform cameraRoot;
    public float mouseSensitivity = 1f;
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
    private bool jumpPressed;
    private bool walkPressed;
    private bool crouchPressed;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        originalHeight = controller.height;
        originalCameraPos = cameraRoot.localPosition;

        inputActions = new InputSystem_Actions();

        // --- Movimiento (Vector2) ---
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        // --- Mirada (Vector2) ---
        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        // --- Salto (Button) ---
        inputActions.Player.Jump.performed += ctx => jumpPressed = true;
        inputActions.Player.Jump.canceled += ctx => jumpPressed = false;

        // --- Caminar lento ---
        inputActions.Player.Walk.performed += ctx => walkPressed = true;
        inputActions.Player.Walk.canceled += ctx => walkPressed = false;

        // --- Agacharse ---
        inputActions.Player.Crouch.performed += ctx => crouchPressed = true;
        inputActions.Player.Crouch.canceled += ctx => crouchPressed = false;
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void OnDisable()
    {
        inputActions.Player.Disable();
    }

    void Update()
    {
        // --- Ground Check ---
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // --- Movimiento ---
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * walkSpeed * Time.deltaTime);

        // --- Salto ---
        if (jumpPressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // --- Andar lento ---
        if (walkPressed && isGrounded)
        {
            walkSpeed = 2.5f;
        }
        else
        {
            walkSpeed = 5.0f;
        }

        // --- Agacharse ---
        float targetHeight;
        if (crouchPressed)
        {
            targetHeight = crouchHeight;
        }
        else
        {
            targetHeight = originalHeight;
        }
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * 10f);

        // --- Cámara suavizada al agacharse ---
        Vector3 targetCameraPos;
        if (crouchPressed)
        {
            targetCameraPos = originalCameraPos + Vector3.down * crouchCameraOffset;
            walkSpeed = 3.5f;
        }
        else
        {
            targetCameraPos = originalCameraPos;
        }
        cameraRoot.localPosition = Vector3.Lerp(cameraRoot.localPosition, targetCameraPos, Time.deltaTime / crouchSmoothTime);

        // --- Gravedad ---
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // --- Rotación de cámara ---
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraRoot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }
}
