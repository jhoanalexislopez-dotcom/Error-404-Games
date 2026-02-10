/*******************************************************
 * Author: [Ignacio López]
 * Last Modified: [26/01/2026]
 * Description:
 *    Component that can set event flags when triggered.
 *    Useful for unlocking doors after story events.
 *    Supports lock flags to prevent triggering until requirements are met.
 *******************************************************/

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;

public class GameEventTrigger : MonoBehaviour
{
    [Header("Event Settings")]
    [Tooltip("Name of the event flag to set")]
    [SerializeField] private string eventFlagName = "";
    
    [Tooltip("Value to set the flag to")]
    [SerializeField] private bool flagValue = true;
    
    [Header("Trigger Settings")]
    [Tooltip("Trigger on Start")]
    [SerializeField] private bool triggerOnStart = false;
    
    [Tooltip("Trigger when player enters collider")]
    [SerializeField] private bool triggerOnPlayerEnter = false;
    
    [Header("Lock Requirements")]
    [Tooltip("Enable to require specific flags before this event can trigger")]
    [SerializeField] private bool useRequirements = false;
    
    [Tooltip("Requirements that must be met before this event can trigger")]
    [SerializeField] private InteractionRequirement requirements;
    
    [Header("Optional Callbacks")]
    [Tooltip("Unity event to invoke when triggered")]
    [SerializeField] private UnityEvent onTriggered;
    
    [Tooltip("Unity event to invoke when trigger is attempted but requirements not met")]
    [SerializeField] private UnityEvent onRequirementsNotMet;
    
    private bool hasTriggered = false;
    
    private void Start()
    {
        if (triggerOnStart)
        {
            TriggerEvent();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!triggerOnPlayerEnter || hasTriggered)
            return;
        
        if (other.CompareTag("Player"))
        {
            TriggerEvent();
        }
    }
    
    public void TriggerEvent()
    {
        if (hasTriggered)
            return;
        
        if (useRequirements && !CheckRequirements())
        {
            onRequirementsNotMet?.Invoke();
            return;
        }
        
        hasTriggered = true;
        
        if (!string.IsNullOrEmpty(eventFlagName))
        {
            if (GameEventManager.Instance != null)
            {
                GameEventManager.Instance.SetEventFlag(eventFlagName, flagValue);
            }
            else
            {
                Debug.LogWarning("GameEventManager instance not found!");
            }
        }
        
        onTriggered?.Invoke();
    }
    
    private bool CheckRequirements()
    {
        if (requirements == null)
        {
            Debug.LogWarning($"Requirements enabled but not configured on {gameObject.name}");
            return false;
        }
        
        bool requirementsMet = requirements.AreRequirementsMet();
        
        if (!requirementsMet)
        {
            LocalizedString lockReason = requirements.GetLockReason();
            if (lockReason != null && !lockReason.IsEmpty)
            {
                Debug.Log($"Event trigger requirements not met: {lockReason.GetLocalizedString()}");
            }
        }
        
        return requirementsMet;
    }
    
    public bool CanTrigger()
    {
        if (hasTriggered)
            return false;
        
        if (useRequirements)
            return CheckRequirements();
        
        return true;
    }
    
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}
