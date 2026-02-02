using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DebugMenu : MonoBehaviour
{
    private const string TOGGLE_KEY = "F12";
    
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private FirstPersonController playerController;
    [SerializeField] private GameObject uiToHide;
    
    [Header("Teleport Locations")]
    [SerializeField] private Transform caveLocation;
    [SerializeField] private Transform campfireLocation;
    [SerializeField] private Transform forestCabinLocation;
    
    [Header("Freecam Settings")]
    [SerializeField] private float freecamSpeed = 10f;
    [SerializeField] private float freecamFastSpeed = 50f;
    [SerializeField] private float freecamZoomSpeed = 10f;
    [SerializeField] private float minFOV = 10f;
    [SerializeField] private float maxFOV = 90f;
    [SerializeField] private Camera freecam;
    
    [Header("Freecam Orbit Settings")]
    [SerializeField] private Transform orbitTarget;
    [SerializeField] private float orbitSpeed = 30f;
    [SerializeField] private float orbitDistance = 5f;
    [SerializeField] private float orbitVerticalSpeed = 20f;
    
    [Header("Freefly Settings")]
    [SerializeField] private float freeflySpeed = 5f;
    [SerializeField] private GameObject freeflyUILeft;
    [SerializeField] private GameObject freeflyUIRight;
    [SerializeField] private TextMeshProUGUI freeflyPositionText;
    
    private InputSystem_Actions inputActions;
    private bool isMenuVisible = false;
    private bool isFreecamActive = false;
    private bool isUIHidden = false;
    private bool isFreeflyActive = false;
    
    private Vector2 freecamMoveInput;
    private Vector2 freecamLookInput;
    private bool freecamFastMode;
    private bool freecamMoveUp;
    private bool freecamMoveDown;
    private bool freecamZoomIn;
    private bool freecamZoomOut;
    private Vector3 freecamRotation;
    private float currentFOV;
    private bool isOrbitActive = false;
    private float orbitAngle = 0f;
    private float orbitHeight = 0f;
    private float orbitLookAtOffset = 0f;
    
    private CharacterController characterController;
    private InputSystem_Actions freecamInputActions;
    private Camera playerCamera;
    private AudioListener playerAudioListener;
    private AudioListener freecamAudioListener;
    private MonoBehaviour[] playerScripts;
    
    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        freecamInputActions = new InputSystem_Actions();
        
        if (playerController != null)
        {
            characterController = playerController.GetComponent<CharacterController>();
            playerScripts = playerController.GetComponents<MonoBehaviour>();
            
            playerCamera = playerController.GetComponentInChildren<Camera>();
            if (playerCamera != null)
            {
                playerAudioListener = playerCamera.GetComponent<AudioListener>();
            }
        }
        
        if (freecam != null)
        {
            freecamAudioListener = freecam.GetComponent<AudioListener>();
            if (freecamAudioListener != null)
            {
                freecamAudioListener.enabled = false;
            }
        }
        
        freecamInputActions.Player.Move.performed += ctx => freecamMoveInput = ctx.ReadValue<Vector2>();
        freecamInputActions.Player.Move.canceled += ctx => freecamMoveInput = Vector2.zero;
        freecamInputActions.Player.Look.performed += ctx => freecamLookInput = ctx.ReadValue<Vector2>();
        freecamInputActions.Player.Look.canceled += ctx => freecamLookInput = Vector2.zero;
        freecamInputActions.Player.Walk.performed += ctx => freecamFastMode = true;
        freecamInputActions.Player.Walk.canceled += ctx => freecamFastMode = false;
        freecamInputActions.Player.Jump.performed += ctx => freecamMoveUp = true;
        freecamInputActions.Player.Jump.canceled += ctx => freecamMoveUp = false;
        freecamInputActions.Player.Crouch.performed += ctx => freecamMoveDown = true;
        freecamInputActions.Player.Crouch.canceled += ctx => freecamMoveDown = false;
        
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
        
        if (freecam != null)
        {
            freecam.gameObject.SetActive(false);
            currentFOV = freecam.fieldOfView;
        }
        
        if (freeflyUILeft != null)
        {
            freeflyUILeft.SetActive(false);
        }
        
        if (freeflyUIRight != null)
        {
            freeflyUIRight.SetActive(false);
        }
    }
    
    private void OnEnable()
    {
        inputActions.Player.Enable();
    }
    
    private void OnDisable()
    {
        inputActions.Player.Disable();
        freecamInputActions.Player.Disable();
    }
    
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f12Key.wasPressedThisFrame)
        {
            ToggleMenu();
        }
        
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame && !isMenuVisible)
        {
            ToggleFreecam();
        }
        
        if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame && !isMenuVisible)
        {
            ToggleUI();
        }
        
        if (Gamepad.current != null && Gamepad.current.selectButton.wasPressedThisFrame && !isMenuVisible && !isFreecamActive)
        {
            ToggleFreefly();
        }
        
        if (isFreecamActive)
        {
            HandleFreecamZoom();
            HandleFreecamOrbit();
            UpdateFreecam();
        }
        
        if (isFreeflyActive)
        {
            UpdateFreefly();
        }
    }
    
    private void ToggleMenu()
    {
        isMenuVisible = !isMenuVisible;
        
        if (menuPanel != null)
        {
            menuPanel.SetActive(isMenuVisible);
        }
        
        if (isMenuVisible)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            if (!isFreecamActive)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
    
    public void ToggleFreecam()
    {
        isFreecamActive = !isFreecamActive;
        
        if (freecam != null && playerController != null)
        {
            freecam.gameObject.SetActive(isFreecamActive);
            
            if (isFreecamActive)
            {
                freecam.transform.position = playerController.cameraRoot.position;
                freecam.transform.rotation = playerController.cameraRoot.rotation;
                freecamRotation = freecam.transform.eulerAngles;
                currentFOV = freecam.fieldOfView;
                isOrbitActive = false;
                orbitAngle = 0f;
                orbitHeight = 0f;
                orbitLookAtOffset = 0f;
                
                if (playerCamera != null)
                {
                    playerCamera.enabled = false;
                }
                
                if (playerAudioListener != null)
                {
                    playerAudioListener.enabled = false;
                }
                
                if (freecamAudioListener != null)
                {
                    freecamAudioListener.enabled = true;
                }
                
                if (characterController != null)
                {
                    characterController.enabled = false;
                }
                
                if (playerScripts != null)
                {
                    foreach (MonoBehaviour script in playerScripts)
                    {
                        if (script != null && script.enabled)
                        {
                            script.enabled = false;
                        }
                    }
                }
                
                freecamInputActions.Player.Enable();
                
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                freecamInputActions.Player.Disable();
                
                if (freecamAudioListener != null)
                {
                    freecamAudioListener.enabled = false;
                }
                
                if (playerScripts != null)
                {
                    foreach (MonoBehaviour script in playerScripts)
                    {
                        if (script != null)
                        {
                            script.enabled = true;
                        }
                    }
                }
                
                if (characterController != null)
                {
                    characterController.enabled = true;
                }
                
                if (playerCamera != null)
                {
                    playerCamera.enabled = true;
                }
                
                if (playerAudioListener != null)
                {
                    playerAudioListener.enabled = true;
                }
                
                if (!isMenuVisible)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }
        
        if (isMenuVisible)
        {
            ToggleMenu();
        }
    }
    
    public void ToggleUI()
    {
        isUIHidden = !isUIHidden;
        
        if (uiToHide != null)
        {
            uiToHide.SetActive(!isUIHidden);
        }
    }
    
    public void TeleportToCave()
    {
        TeleportPlayer(caveLocation);
    }
    
    public void TeleportToCampfire()
    {
        TeleportPlayer(campfireLocation);
    }
    
    public void TeleportToForestCabin()
    {
        TeleportPlayer(forestCabinLocation);
    }
    
    private void TeleportPlayer(Transform destination)
    {
        if (destination == null || playerController == null)
        {
            Debug.LogWarning("Teleport failed: missing destination or player controller");
            return;
        }
        
        if (characterController != null)
        {
            characterController.enabled = false;
            playerController.transform.position = destination.position;
            characterController.enabled = true;
        }
        else
        {
            playerController.transform.position = destination.position;
        }
        
        Debug.Log($"Teleported player to {destination.name}");
    }
    
    public void SetCollectibleFlag(string flagName)
    {
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.SetEventFlag(flagName, true);
            Debug.Log($"Set flag '{flagName}' to true");
        }
        else
        {
            Debug.LogWarning("GameEventManager instance not found");
        }
    }
    
    private void HandleFreecamZoom()
    {
        if (freecam == null) return;
        
        if (Gamepad.current != null)
        {
            if (Gamepad.current.buttonNorth.isPressed)
            {
                freecamZoomIn = true;
            }
            else
            {
                freecamZoomIn = false;
            }
            
            if (Gamepad.current.buttonWest.isPressed)
            {
                freecamZoomOut = true;
            }
            else
            {
                freecamZoomOut = false;
            }
        }
        
        if (freecamZoomIn)
        {
            currentFOV -= freecamZoomSpeed * Time.unscaledDeltaTime;
        }
        else if (freecamZoomOut)
        {
            currentFOV += freecamZoomSpeed * Time.unscaledDeltaTime;
        }
        
        currentFOV = Mathf.Clamp(currentFOV, minFOV, maxFOV);
        freecam.fieldOfView = currentFOV;
    }
    
    private void HandleFreecamOrbit()
    {
        if (freecam == null || orbitTarget == null) return;
        
        if (Gamepad.current != null)
        {
            if (Gamepad.current.leftShoulder.isPressed)
            {
                isOrbitActive = true;
            }
            else
            {
                isOrbitActive = false;
            }
        }
    }
    
    private void UpdateFreecam()
    {
        if (freecam == null) return;
        
        if (isOrbitActive && orbitTarget != null)
        {
            orbitAngle += orbitSpeed * Time.unscaledDeltaTime;
            
            bool moveUp = freecamMoveUp;
            bool moveDown = freecamMoveDown;
            
            if (Gamepad.current != null)
            {
                Vector2 leftStick = Gamepad.current.leftStick.ReadValue();
                orbitLookAtOffset += leftStick.y * orbitVerticalSpeed * Time.unscaledDeltaTime;
                
                if (Gamepad.current.buttonSouth.isPressed)
                {
                    moveUp = true;
                }
                if (Gamepad.current.buttonEast.isPressed)
                {
                    moveDown = true;
                }
            }
            
            if (moveUp)
            {
                orbitHeight += orbitVerticalSpeed * Time.unscaledDeltaTime;
            }
            if (moveDown)
            {
                orbitHeight -= orbitVerticalSpeed * Time.unscaledDeltaTime;
            }
            
            float radians = orbitAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(
                Mathf.Sin(radians) * orbitDistance,
                orbitHeight,
                Mathf.Cos(radians) * orbitDistance
            );
            
            freecam.transform.position = orbitTarget.position + offset;
            
            Vector3 lookAtPoint = orbitTarget.position + Vector3.up * orbitLookAtOffset;
            freecam.transform.LookAt(lookAtPoint);
            
            freecamRotation = freecam.transform.eulerAngles;
        }
        else
        {
            float speed = freecamFastMode ? freecamFastSpeed : freecamSpeed;
            
            Vector3 move = freecam.transform.right * freecamMoveInput.x + freecam.transform.forward * freecamMoveInput.y;
            
            if (freecamMoveUp)
            {
                move += Vector3.up;
            }
            if (freecamMoveDown)
            {
                move += Vector3.down;
            }
            
            freecam.transform.position += move * speed * Time.unscaledDeltaTime;
            
            float mouseX = freecamLookInput.x * 2f * Time.unscaledDeltaTime * 60f;
            float mouseY = freecamLookInput.y * 2f * Time.unscaledDeltaTime * 60f;
            
            freecamRotation.y += mouseX;
            freecamRotation.x -= mouseY;
            freecamRotation.x = Mathf.Clamp(freecamRotation.x, -90f, 90f);
            
            freecam.transform.eulerAngles = freecamRotation;
        }
    }
    
    public void CloseMenu()
    {
        if (isMenuVisible)
        {
            ToggleMenu();
        }
    }
    
    private void ToggleFreefly()
    {
        isFreeflyActive = !isFreeflyActive;
        
        if (freeflyUILeft != null)
        {
            freeflyUILeft.SetActive(isFreeflyActive);
        }
        
        if (freeflyUIRight != null)
        {
            freeflyUIRight.SetActive(isFreeflyActive);
        }
        
        if (isFreeflyActive)
        {
            if (characterController != null)
            {
                characterController.enabled = false;
            }
            
            if (playerController != null)
            {
                playerController.ResetLookInput();
            }
        }
        else
        {
            if (characterController != null)
            {
                characterController.enabled = true;
            }
        }
    }
    
    private void UpdateFreefly()
    {
        if (playerController == null || characterController == null) return;
        
        if (Gamepad.current == null) return;
        
        if (playerController != null)
        {
            playerController.ResetLookInput();
        }
        
        Vector2 leftStick = Gamepad.current.leftStick.ReadValue();
        Vector2 rightStick = Gamepad.current.rightStick.ReadValue();
        
        Transform playerTransform = playerController.transform;
        
        Vector3 moveDirection = Vector3.zero;
        moveDirection += playerTransform.right * leftStick.x;
        moveDirection += playerTransform.forward * leftStick.y;
        moveDirection += Vector3.up * rightStick.y;
        
        playerTransform.position += moveDirection * freeflySpeed * Time.deltaTime;
        
        if (freeflyPositionText != null)
        {
            Vector3 pos = playerTransform.position;
            freeflyPositionText.text = $"Pos : {pos.x:F1}, {pos.y:F1}, {pos.z:F1}";
        }
    }
}
