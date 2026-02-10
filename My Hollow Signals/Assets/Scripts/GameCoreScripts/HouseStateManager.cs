/*******************************************************
 * Author: [Ignacio Lopez]
 * Last Modified: [26/01/2026]
 * Description:
 *   This script manages the state of the house based on event flags. It allows you to specify which GameObjects should be enabled or disabled when certain flags are set to specific values. The script can check for flag changes continuously or only on start, and it provides options for debugging and manual state checks.
 *******************************************************/

using UnityEngine;
using System.Collections.Generic;

public class HouseStateManager : MonoBehaviour
{
    [System.Serializable]
    public class StateChangeConfig
    {
        [Header("Trigger")]
        [Tooltip("Event flag name to monitor")]
        public string eventFlagName = "";
        
        [Tooltip("Required value of the flag to trigger this state change")]
        public bool requiredFlagValue = true;
        
        [Header("Actions")]
        [Tooltip("GameObjects to enable when flag becomes active")]
        public GameObject[] objectsToEnable;
        
        [Tooltip("GameObjects to disable when flag becomes active")]
        public GameObject[] objectsToDisable;
        
        [Header("Debug")]
        [Tooltip("Show debug messages when this state change triggers")]
        public bool debugLog = false;
        
        [HideInInspector]
        public bool hasTriggered = false;
    }
    
    [Header("State Configurations")]
    [Tooltip("List of state changes to monitor and execute")]
    public List<StateChangeConfig> stateConfigs = new List<StateChangeConfig>();
    
    [Header("Settings")]
    [Tooltip("Check for flag changes every frame (enable for immediate response)")]
    public bool continuousMonitoring = true;
    
    [Tooltip("Also check flags on Start")]
    public bool checkOnStart = true;
    
    [Tooltip("Time between checks when continuous monitoring is enabled (0 = every frame)")]
    public float monitoringInterval = 0f;
    
    private float nextCheckTime;
    
    private void Start()
    {
        if (GameEventManager.Instance == null)
        {
            Debug.LogWarning("HouseStateManager: GameEventManager instance not found!");
            return;
        }
        
        if (checkOnStart)
        {
            CheckAllStates();
        }
        
        nextCheckTime = Time.time;
    }
    
    private void Update()
    {
        if (!continuousMonitoring)
            return;
        
        if (GameEventManager.Instance == null)
            return;
        
        if (monitoringInterval > 0f && Time.time < nextCheckTime)
            return;
        
        CheckAllStates();
        
        if (monitoringInterval > 0f)
        {
            nextCheckTime = Time.time + monitoringInterval;
        }
    }
    
    private void CheckAllStates()
    {
        foreach (StateChangeConfig config in stateConfigs)
        {
            if (config.hasTriggered)
                continue;
            
            if (string.IsNullOrEmpty(config.eventFlagName))
                continue;
            
            bool flagValue = GameEventManager.Instance.GetEventFlag(config.eventFlagName);
            
            if (flagValue == config.requiredFlagValue)
            {
                ExecuteStateChange(config);
                config.hasTriggered = true;
            }
        }
    }
    
    private void ExecuteStateChange(StateChangeConfig config)
    {
        if (config.debugLog)
        {
            Debug.Log($"HouseStateManager: Executing state change for flag '{config.eventFlagName}' = {config.requiredFlagValue}");
        }
        
        foreach (GameObject obj in config.objectsToEnable)
        {
            if (obj != null)
            {
                obj.SetActive(true);
                
                if (config.debugLog)
                {
                    Debug.Log($"  Enabled: {obj.name}");
                }
            }
        }
        
        foreach (GameObject obj in config.objectsToDisable)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                
                if (config.debugLog)
                {
                    Debug.Log($"  Disabled: {obj.name}");
                }
            }
        }
    }
    
    public void ManualCheckState(string flagName)
    {
        foreach (StateChangeConfig config in stateConfigs)
        {
            if (config.eventFlagName == flagName && !config.hasTriggered)
            {
                bool flagValue = GameEventManager.Instance?.GetEventFlag(config.eventFlagName) ?? false;
                
                if (flagValue == config.requiredFlagValue)
                {
                    ExecuteStateChange(config);
                    config.hasTriggered = true;
                }
            }
        }
    }
    
    public void ResetAllStates()
    {
        foreach (StateChangeConfig config in stateConfigs)
        {
            config.hasTriggered = false;
        }
    }
    
    public void ResetState(string flagName)
    {
        foreach (StateChangeConfig config in stateConfigs)
        {
            if (config.eventFlagName == flagName)
            {
                config.hasTriggered = false;
            }
        }
    }
}
