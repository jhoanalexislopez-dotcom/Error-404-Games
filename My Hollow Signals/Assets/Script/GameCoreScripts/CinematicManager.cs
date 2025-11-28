using UnityEngine;
using UnityEngine.InputSystem;

public class CinematicManager : MonoBehaviour
{
    private static CinematicManager instance;
    
    public static bool IsCinematicActive { get; private set; }
    
    private static FlashlightController flashlightController;
    private static PlayerInput playerInput;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public static void StartCinematic()
    {
        IsCinematicActive = true;
        
        if (flashlightController == null)
        {
            flashlightController = FindObjectOfType<FlashlightController>();
        }
        
        if (flashlightController != null)
        {
            flashlightController.enabled = false;
        }
        
        if (playerInput == null)
        {
            playerInput = FindObjectOfType<PlayerInput>();
        }
        
        if (playerInput != null)
        {
            playerInput.enabled = false;
        }
    }
    
    public static void EndCinematic()
    {
        IsCinematicActive = false;
        
        if (flashlightController != null)
        {
            flashlightController.enabled = true;
        }
        
        if (playerInput != null)
        {
            playerInput.enabled = true;
        }
    }
}
