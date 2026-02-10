/*******************************************************
 * Author: [Ignacio López]
 * Last Modified: [27/01/2026]
 * Description:
 *  Manages cinematic sequences by disabling player control and saving/restoring camera and player states. Provides static methods to start and end cinematics, ensuring a smooth transition in and out of cinematic mode while maintaining the player's original orientation and camera settings.
*******************************************************/

using UnityEngine;
using UnityEngine.InputSystem;

public class CinematicManager : MonoBehaviour
{
    private static CinematicManager instance;
    
    public static bool IsCinematicActive { get; private set; }
    
    private static Transform savedCameraRoot;
    private static Quaternion savedCameraRotation;
    private static float savedXRotation;
    private static Transform savedPlayerTransform;
    private static Quaternion savedPlayerRotation;
    private static Transform savedMainCamera;
    private static Quaternion savedMainCameraRotation;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public static void StartCinematic()
    {
        IsCinematicActive = true;
        
        FirstPersonController playerController = FindObjectOfType<FirstPersonController>();
        if (playerController != null && playerController.cameraRoot != null)
        {
            savedCameraRoot = playerController.cameraRoot;
            savedCameraRotation = savedCameraRoot.localRotation;
            savedXRotation = GetXRotationFromController(playerController);
            
            savedPlayerTransform = playerController.transform;
            savedPlayerRotation = savedPlayerTransform.rotation;
            
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                savedMainCamera = mainCam.transform;
                savedMainCameraRotation = savedMainCamera.localRotation;
            }
            
            playerController.ResetLookInput();
        }
        
        FlashlightController flashlightController = FindObjectOfType<FlashlightController>();
        if (flashlightController != null)
        {
            flashlightController.SetFlashlightInputEnabled(false);
        }
        
        PlayerInput playerInput = FindObjectOfType<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = false;
        }
    }
    
    public static void EndCinematic()
    {
        IsCinematicActive = false;
        
        if (savedCameraRoot != null && savedPlayerTransform != null)
        {
            savedPlayerTransform.rotation = savedPlayerRotation;
            savedCameraRoot.localRotation = savedCameraRotation;
            
            if (savedMainCamera != null)
            {
                savedMainCamera.localRotation = savedMainCameraRotation;
            }
            
            FirstPersonController playerController = FindObjectOfType<FirstPersonController>();
            if (playerController != null)
            {
                SetXRotationOnController(playerController, savedXRotation);
                playerController.ResetLookInput();
            }
            
            savedCameraRoot = null;
            savedPlayerTransform = null;
            savedMainCamera = null;
        }
        
        FlashlightController flashlightController = FindObjectOfType<FlashlightController>();
        if (flashlightController != null)
        {
            flashlightController.SetFlashlightInputEnabled(true);
        }
        
        PlayerInput playerInput = FindObjectOfType<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = true;
        }
    }
    
    private static float GetXRotationFromController(FirstPersonController controller)
    {
        var field = controller.GetType().GetField("xRotation", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            return (float)field.GetValue(controller);
        }
        
        return 0f;
    }
    
    private static void SetXRotationOnController(FirstPersonController controller, float value)
    {
        var field = controller.GetType().GetField("xRotation", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            field.SetValue(controller, value);
        }
    }
}
