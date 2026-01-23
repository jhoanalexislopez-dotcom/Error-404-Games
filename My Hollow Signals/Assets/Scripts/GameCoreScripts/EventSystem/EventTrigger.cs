/*******************************************************
 * Author: [Bianca Marinica]
 * Last Modified: [21/11/2025]
 * Description:
 *    Component that can set event flags when triggered.
 *    Useful for unlocking doors after story events.
 *******************************************************/

using UnityEngine;
using UnityEngine.Events;

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
    
    [Header("Optional Callbacks")]
    [Tooltip("Unity event to invoke when triggered")]
    [SerializeField] private UnityEvent onTriggered;
    
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
    
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}
