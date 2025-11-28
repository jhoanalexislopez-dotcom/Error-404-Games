using UnityEngine;
using UnityEngine.InputSystem;

public class CinematicManager : MonoBehaviour
{
    private static CinematicManager instance;
    
    public static bool IsCinematicActive { get; private set; }
    
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
        
        FlashlightController flashlightController = FindObjectOfType<FlashlightController>();
        if (flashlightController != null)
        {
            flashlightController.enabled = false;
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
        
        FlashlightController flashlightController = FindObjectOfType<FlashlightController>();
        if (flashlightController != null)
        {
            flashlightController.enabled = true;
        }
        
        PlayerInput playerInput = FindObjectOfType<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = true;
        }
    }
}
