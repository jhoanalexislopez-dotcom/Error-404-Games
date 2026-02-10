/*******************************************************
 * Author: [Jhoan Alexis Lopez]
 * Updated by: [Ignacio López]
 * Last Modified: [30/01/2026]
 * Description:
 *    Creates realistic camera bobbing effect when the player walks or runs.
 *******************************************************/
using UnityEngine;
using UnityEngine.InputSystem; // <- Input System

[DisallowMultipleComponent]
public class HeadBob : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Transform que se bobeará. Suele ser la cámara o un empty padre de la cámara.")]
    public Transform cameraTarget;

    [Tooltip("CharacterController opcional para saber si estás en el suelo.")]
    public CharacterController characterController;

    [Tooltip("PlayerController para leer el estado de movimiento real.")]
    public FirstPersonController playerController;

    [Header("Input (Input System)")]
    [Tooltip("InputActionReference del vector de movimiento (Vector2).")]
    public InputActionReference moveAction;   // ej: Player/Move (Value Vector2)

    [Header("Bob Settings")]
    [Tooltip("Requiere estar en el suelo para hacer headbob.")]
    public bool requireGrounded = true;

    [Tooltip("Desplazamiento base local (por si tu cámara no parte de 0,0,0).")]
    public Vector3 baseLocalOffset = Vector3.zero;

    [Tooltip("Umbral mínimo de movimiento para activar el bob.")]
    public float moveThreshold = 0.1f;

    [Tooltip("Suavizado del lerp hacia la posición bob (más alto = más suave).")]
    [Range(1f, 30f)] public float smooth = 12f;

    [Header("Walking")]
    public float walkAmplitude = 0.03f;
    public float walkFrequency = 6.5f;

    [Header("Running")]
    public float runAmplitude = 0.055f;
    public float runFrequency = 9.5f;

    [Header("Crouching")]
    public float crouchAmplitude = 0.02f;
    public float crouchFrequency = 5f;

    [Header("Idle Motion")]
    [Tooltip("Enable subtle idle motion when not moving")]
    public bool enableIdleMotion = true;

    [Tooltip("Amplitude of the idle breathing motion")]
    [Range(0.001f, 0.02f)]
    public float idleAmplitude = 0.005f;

    [Tooltip("Frequency of the idle breathing motion")]
    [Range(0.5f, 3f)]
    public float idleFrequency = 1.2f;

    [Tooltip("How quickly to transition to idle motion")]
    [Range(1f, 10f)]
    public float idleTransitionSpeed = 3f;

    [Header("Strafe & Axis Mix")]
    [Tooltip("Peso del eje horizontal en el bob (0 = solo vertical; 1 = vertical+horizontal).")]
    [Range(0f, 1f)] public float horizontalBobFactor = 0.3f;

    [Tooltip("Multiplicador del bob vertical.")]
    public float verticalScale = 1f;

    [Tooltip("Multiplicador del bob horizontal (side-to-side).")]
    public float horizontalScale = 1f;

    [Header("Speed Influence")]
    [Tooltip("Multiplica la amplitud por la velocidad del jugador (CharacterController.velocity).")]
    public bool scaleWithSpeed = true;
    public float speedAmplitudeFactor = 0.25f;

    // Internals
    private Vector3 _initialLocalPos;
    private float _bobTimer;
    private float _idleTimer;
    private bool _hasMove;
    private bool _isSprinting;
    private float _idleBlendWeight = 0f;
    
    // UI managers
    private PauseMenuManager pauseMenuManager;
    private MobilePhoneToggle mobilePhoneToggle;
    private Inventory3DController inventory3DController;
    private NoteUIManager noteUIManager;

    void Reset()
    {
        cameraTarget = Camera.main ? Camera.main.transform : null;
        characterController = GetComponentInParent<CharacterController>();
    }

    void Awake()
    {
        if (cameraTarget == null) cameraTarget = transform;

        _initialLocalPos = cameraTarget.localPosition;

        // Habilitar acciones si vienen por referencia
        if (moveAction != null && moveAction.action != null && !moveAction.action.enabled)
            moveAction.action.Enable();
        
        // Cache UI managers
        pauseMenuManager = FindObjectOfType<PauseMenuManager>();
        mobilePhoneToggle = FindObjectOfType<MobilePhoneToggle>();
        inventory3DController = FindObjectOfType<Inventory3DController>();
        noteUIManager = FindObjectOfType<NoteUIManager>(true);
    }

    void OnDisable()
    {
        if (moveAction != null && moveAction.action != null && moveAction.action.enabled)
            moveAction.action.Disable();
    }

    void Update()
    {
        if (cameraTarget == null) return;

        if (CinematicManager.IsCinematicActive)
        {
            _bobTimer = 0f;
            _idleBlendWeight = 0f;
            return;
        }
        
        // Check if any UI is open - disable headbob
        if (pauseMenuManager != null && pauseMenuManager.IsPaused)
        {
            _bobTimer = 0f;
            _idleBlendWeight = 0f;
            return;
        }
        
        if (mobilePhoneToggle != null && mobilePhoneToggle.IsPhoneVisible)
        {
            _bobTimer = 0f;
            _idleBlendWeight = 0f;
            return;
        }
        
        if (inventory3DController != null && inventory3DController.IsInventoryOpen)
        {
            _bobTimer = 0f;
            _idleBlendWeight = 0f;
            return;
        }
        
        if (noteUIManager != null && noteUIManager.IsNoteActive)
        {
            _bobTimer = 0f;
            _idleBlendWeight = 0f;
            return;
        }

        // 1) Leer input del Input System
        Vector2 move = Vector2.zero;
        if (moveAction != null && moveAction.action != null)
            move = moveAction.action.ReadValue<Vector2>();

        _hasMove = move.sqrMagnitude > (moveThreshold * moveThreshold);

        // Read actual movement state from PlayerController (accounts for mutual exclusion logic)
        bool isCrouching = playerController != null && playerController.IsCrouching;
        _isSprinting = playerController != null && playerController.IsRunning;

        // 2) Chequear grounded si procede
        if (requireGrounded && characterController != null && !characterController.isGrounded)
        {
            // Sin bob si no estás en el suelo
            _bobTimer = 0f;
            _idleBlendWeight = 0f;
            Vector3 targetNoBob = _initialLocalPos + baseLocalOffset;
            cameraTarget.localPosition = Vector3.Lerp(cameraTarget.localPosition, targetNoBob, Time.deltaTime * smooth);
            return;
        }

        Vector3 target = _initialLocalPos + baseLocalOffset;

        // 3) Handle movement or idle state
        if (!_hasMove)
        {
            // Player is not moving - apply idle motion
            _bobTimer = 0f;

            if (enableIdleMotion)
            {
                // Blend into idle motion
                _idleBlendWeight = Mathf.Lerp(_idleBlendWeight, 1f, Time.deltaTime * idleTransitionSpeed);

                // Update idle timer
                _idleTimer += Time.deltaTime * idleFrequency;

                // Calculate idle breathing motion (subtle vertical movement)
                float idleVertical = Mathf.Sin(_idleTimer) * idleAmplitude * _idleBlendWeight;

                // Optional: very subtle horizontal sway
                float idleHorizontal = Mathf.Sin(_idleTimer * 0.7f) * idleAmplitude * 0.3f * _idleBlendWeight;

                target.y += idleVertical;
                target.x += idleHorizontal;
            }
            else
            {
                _idleBlendWeight = 0f;
            }
        }
        else
        {
            // Player is moving - apply movement bob
            _idleBlendWeight = Mathf.Lerp(_idleBlendWeight, 0f, Time.deltaTime * idleTransitionSpeed * 2f);

            // Elegir parámetros según estado (Priority: Crouch > Run > Walk)
            float amp, freq;
            if (isCrouching)
            {
                amp = crouchAmplitude;
                freq = crouchFrequency;
            }
            else if (_isSprinting)
            {
                amp = runAmplitude;
                freq = runFrequency;
            }
            else
            {
                amp = walkAmplitude;
                freq = walkFrequency;
            }

            // Opcional: escalar por velocidad real
            float speedFactor = 1f;
            if (scaleWithSpeed && characterController != null)
            {
                // Horizontal speed solamente
                Vector3 vel = characterController.velocity;
                vel.y = 0f;
                float v = vel.magnitude;
                // Limitar un poco la contribución
                speedFactor += Mathf.Clamp01(v) * speedAmplitudeFactor;
            }

            amp *= speedFactor;

            // 5) Avanzar tiempo del bob
            _bobTimer += Time.deltaTime * freq;

            // 6) Calcular offsets seno/coseno (clásico headbob)
            float bobVertical = Mathf.Sin(_bobTimer) * amp * verticalScale;
            float bobHorizontal = Mathf.Cos(_bobTimer * 0.5f) * amp * horizontalScale * horizontalBobFactor;

            // Apply movement bob
            target.x += bobHorizontal;
            target.y += bobVertical;
        }

        // 7) Interpolar suave
        cameraTarget.localPosition = Vector3.Lerp(cameraTarget.localPosition, target, Time.deltaTime * smooth);
    }
}
