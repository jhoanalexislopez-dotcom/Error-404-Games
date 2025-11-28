/*******************************************************
 * Author: [Ignacio Lopez]
 * Last Modified: [21/11/2025]
 * Description:
 *    Manages game-wide audio system including footsteps, flashlight sounds, and environmental audio based on player state.
 *******************************************************/

using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Opcional: CharacterController para grounded/velocidad.")]
    [SerializeField] private CharacterController characterController;
    [Tooltip("Objeto de la linterna que se enciende/apaga (opcional).")]
    [SerializeField] private GameObject flashlightObject;

    [Header("Input (Input System)")]
    [Tooltip("Vector2 de movimiento (WASD/Stick).")]
    [SerializeField] private InputActionReference moveAction;        // Value Vector2
    [Tooltip("Sprint (Button).")]
    [SerializeField] private InputActionReference sprintAction;      // Button
    [Tooltip("Crouch (Button). Mantener o toggle, como prefieras.")]
    [SerializeField] private InputActionReference crouchAction;      // Button
    [Tooltip("Toggle de linterna (Button).")]
    [SerializeField] private InputActionReference flashlightToggle;  // Button

    [Header("Audio Sources")]
    [Tooltip("Source para efectos (PlayOneShot). Si est� vac�o, se crea uno.")]
    [SerializeField] private AudioSource sfxSource;
    [Tooltip("Pitch aleatorio de pasos (min/max).")]
    [SerializeField] private Vector2 footstepPitchRange = new Vector2(0.95f, 1.05f);

    [Header("Footsteps - Clips")]
    [Tooltip("Clips de pasos caminando.")]
    [SerializeField] private AudioClip[] walkFootsteps;
    [Tooltip("Clips de pasos corriendo (pueden ser los mismos).")]
    [SerializeField] private AudioClip[] runFootsteps;
    [Tooltip("Clips de pasos agachado (m�s suaves).")]
    [SerializeField] private AudioClip[] crouchFootsteps;

    [Header("Footsteps - Timings & Volumes")]
    [Tooltip("Intervalo entre pasos caminando (seg).")]
    [SerializeField] private float walkStepInterval = 0.5f;
    [Tooltip("Intervalo entre pasos corriendo (seg).")]
    [SerializeField] private float runStepInterval = 0.35f;
    [Tooltip("Intervalo entre pasos agachado (seg).")]
    [SerializeField] private float crouchStepInterval = 0.65f;

    [Range(0f, 1f)][SerializeField] private float walkVolume = 0.8f;
    [Range(0f, 1f)][SerializeField] private float runVolume = 1.0f;
    [Range(0f, 1f)][SerializeField] private float crouchVolume = 0.5f;

    [Header("Footsteps - Misc")]
    [Tooltip("Umbral m�nimo de input para considerar que te mueves.")]
    [SerializeField] private float moveThreshold = 0.1f;
    [Tooltip("Requiere estar en el suelo para sonar pasos.")]
    [SerializeField] private bool requireGrounded = true;

    [Header("Flashlight SFX")]
    [SerializeField] private AudioClip flashlightOnClip;
    [SerializeField] private AudioClip flashlightOffClip;
    [SerializeField] private AudioClip flashlightRechargeClip;
    [Range(0f, 1f)][SerializeField] private float flashlightVolume = 0.8f;

    public GameObject BlackLayoutTransitionUI;

    // Internos
    private float stepTimer;
    private bool isCrouched;
    private bool flashlightOn;

    private void Reset()
    {
        characterController = GetComponentInParent<CharacterController>();
    }

    private void Awake()
    {
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 1f; // 3D por defecto; ajusta a 0 si lo quieres 2D
        }

        EnableAction(moveAction);
        EnableAction(sprintAction);
        EnableAction(crouchAction);
        EnableAction(flashlightToggle);
    }

    private void Start()
    {
        if (BlackLayoutTransitionUI != null)
            BlackLayoutTransitionUI.SetActive(true);
    }

    private void OnEnable()
    {
        if (flashlightToggle != null && flashlightToggle.action != null)
            flashlightToggle.action.performed += OnFlashlightToggle;

        if (crouchAction != null && crouchAction.action != null)
            crouchAction.action.performed += OnCrouchPerformed;
    }

    private void OnDisable()
    {
        if (flashlightToggle != null && flashlightToggle.action != null)
            flashlightToggle.action.performed -= OnFlashlightToggle;

        if (crouchAction != null && crouchAction.action != null)
            crouchAction.action.performed -= OnCrouchPerformed;
    }

    private void Update()
    {
        HandleFootsteps();
        // Si tu crouch es "hold" en vez de toggle, puedes leerlo aqu�:
        // isCrouched = crouchAction?.action?.IsPressed() ?? isCrouched;
    }

    // ---------- Footsteps ----------
    private void HandleFootsteps()
    {
        if (CinematicManager.IsCinematicActive)
        {
            stepTimer = 0f;
            return;
        }

        Vector2 move = moveAction != null && moveAction.action != null
            ? moveAction.action.ReadValue<Vector2>()
            : Vector2.zero;

        bool moving = move.sqrMagnitude > (moveThreshold * moveThreshold);

        bool grounded = true;
        float horizontalSpeed = 0f;
        if (characterController != null)
        {
            grounded = characterController.isGrounded;
            Vector3 vel = characterController.velocity; vel.y = 0f;
            horizontalSpeed = vel.magnitude;
        }

        if (requireGrounded && !grounded) { stepTimer = 0f; return; }
        if (!moving) { stepTimer = 0f; return; }

        bool running = sprintAction != null && sprintAction.action != null && sprintAction.action.IsPressed();

        float interval = isCrouched ? crouchStepInterval : (running ? runStepInterval : walkStepInterval);
        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            // Elige set de clips seg�n estado
            AudioClip[] pool = isCrouched ? crouchFootsteps : (running ? runFootsteps : walkFootsteps);
            if (pool != null && pool.Length > 0 && sfxSource != null)
            {
                var clip = pool[Random.Range(0, pool.Length)];
                float vol = isCrouched ? crouchVolume : (running ? runVolume : walkVolume);

                // sutil variaci�n de pitch
                sfxSource.pitch = Random.Range(footstepPitchRange.x, footstepPitchRange.y);

                // Reproducir en la posici�n del CharacterController si existe
                if (characterController != null) sfxSource.transform.position = characterController.transform.position;

                sfxSource.PlayOneShot(clip, vol);
            }

            // Ajuste simple con la velocidad para que aumente la cadencia si vas m�s r�pido
            float speedFactor = Mathf.Clamp01(horizontalSpeed); // 0..1 aprox
            float dynamicInterval = Mathf.Lerp(interval * 0.75f, interval * 1.25f, 1f - speedFactor);

            stepTimer = dynamicInterval;
        }
    }

    // ---------- Flashlight ----------
    private void OnFlashlightToggle(InputAction.CallbackContext ctx)
    {
        flashlightOn = !flashlightOn;

        if (flashlightObject != null)
            flashlightObject.SetActive(flashlightOn);

        var clip = flashlightOn ? flashlightOnClip : flashlightOffClip;
        if (clip != null && sfxSource != null)
        {
            sfxSource.pitch = 1f;
            sfxSource.PlayOneShot(clip, flashlightVolume);
        }
    }
    public void PlayFlashlightRecharge()
    {
        if (flashlightRechargeClip != null && sfxSource != null)
        {
            sfxSource.pitch = 1f;
            sfxSource.PlayOneShot(flashlightRechargeClip, flashlightVolume);
        }
    }


    // Si tu crouch es toggle: cambia estado cuando se pulse
    private void OnCrouchPerformed(InputAction.CallbackContext ctx)
    {
        // Si quieres "hold", comenta esta l�nea y usa isCrouched = crouchAction.action.IsPressed() en Update()
        isCrouched = !isCrouched;
    }

    // ---------- Helpers ----------
    private static void EnableAction(InputActionReference actionRef)
    {
        if (actionRef != null && actionRef.action != null && !actionRef.action.enabled)
            actionRef.action.Enable();
    }

    // ---------- API p�blica opcional ----------
    public void SetCrouched(bool crouched) => isCrouched = crouched;
    public void ForceFlashlight(bool on)
    {
        flashlightOn = on;
        if (flashlightObject != null) flashlightObject.SetActive(on);
        var clip = on ? flashlightOnClip : flashlightOffClip;
        if (clip != null && sfxSource != null) 
        { 
            sfxSource.pitch = 1f; 
            sfxSource.PlayOneShot(clip, flashlightVolume); 
        }
    }
}
