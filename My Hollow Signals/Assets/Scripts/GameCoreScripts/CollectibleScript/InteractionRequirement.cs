/*******************************************************
 * Author: [Bianca Marinica]
 * Last Modified: [21/11/2025]
 * Description:
 *    Defines requirements that must be met before an object can be interacted with.
 *    Supports collectible requirements (flashlight, items) and event-based requirements.
 *******************************************************/

using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class InteractionRequirement
{
    [Header("Collectible Requirements")]
    [Tooltip("Requires the player to have collected the flashlight")]
    public bool requiresFlashlight = false;
    
    [Tooltip("Minimum number of collectible items required")]
    public int minimumItemsRequired = 0;
    
    [Header("Event Requirements")]
    [Tooltip("Custom event flag that must be true before interaction")]
    public string eventFlagName = "";
    
    [Header("Feedback")]
    [Tooltip("Message to display when requirements are not met")]
    public string lockedMessage = "This door is locked.";
    
    public bool AreRequirementsMet()
    {
        if (PlayerInventory.Instance == null)
        {
            Debug.LogWarning("PlayerInventory instance not found!");
            return false;
        }
        
        if (requiresFlashlight && !PlayerInventory.Instance.HasFlashlight)
        {
            return false;
        }
        
        if (minimumItemsRequired > 0 && PlayerInventory.Instance.collected < minimumItemsRequired)
        {
            return false;
        }
        
        if (!string.IsNullOrEmpty(eventFlagName))
        {
            bool eventFlag = GameEventManager.Instance?.GetEventFlag(eventFlagName) ?? false;
            if (!eventFlag)
            {
                return false;
            }
        }
        
        return true;
    }
    
    public string GetLockReason()
    {
        if (!string.IsNullOrEmpty(lockedMessage))
        {
            return lockedMessage;
        }
        
        if (requiresFlashlight && (PlayerInventory.Instance == null || !PlayerInventory.Instance.HasFlashlight))
        {
            return "You need a flashlight to open this.";
        }
        
        if (minimumItemsRequired > 0 && (PlayerInventory.Instance == null || PlayerInventory.Instance.collected < minimumItemsRequired))
        {
            return $"You need {minimumItemsRequired} items to open this.";
        }
        
        if (!string.IsNullOrEmpty(eventFlagName))
        {
            return "This door is locked.";
        }
        
        return "Requirements not met.";
    }
}
