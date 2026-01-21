using UnityEngine;

public class FlashlightCollectible : MonoBehaviour, IInteractable
{
    [SerializeField] private string description = "Pick up Flashlight";
    
    [Header("Player Flashlight")]
    [Tooltip("Reference to the player's Hand/flashlight GameObject to enable")]
    [SerializeField] private GameObject playerFlashlightObject;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pickupSound;
    
    public string GetDescription()
    {
        return description;
    }

    public void Interact()
    {
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.CollectFlashlight();
            
            // Enable the player's flashlight in hand
            if (playerFlashlightObject != null)
            {
                playerFlashlightObject.SetActive(true);
                Debug.Log("Flashlight enabled in player's hand!");
            }
            else
            {
                // Fallback: Try to find it automatically
                FlashlightController flashlightController = FindObjectOfType<FlashlightController>();
                if (flashlightController != null)
                {
                    flashlightController.gameObject.SetActive(true);
                    Debug.Log("Flashlight GameObject activated!");
                }
                else
                {
                    Debug.LogWarning("Player flashlight object not assigned and couldn't find FlashlightController!");
                }
            }
            
            if (audioSource != null && pickupSound != null)
            {
                audioSource.PlayOneShot(pickupSound);
            }
            
            Debug.Log("Flashlight collected!");
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("PlayerInventory.Instance is null!");
        }
    }
}
