/*******************************************************
 * Author: [Bianca Marinica]
 * Last Modified: [21/11/2025]
 * Description:
 *    Singleton manager for tracking game events and flags.
 *    Used to determine if specific story events have occurred.
 *******************************************************/

using UnityEngine;
using System.Collections.Generic;

public class GameEventManager : MonoBehaviour
{
    public static GameEventManager Instance { get; private set; }
    
    private Dictionary<string, bool> eventFlags = new Dictionary<string, bool>();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    public void SetEventFlag(string flagName, bool value)
    {
        if (eventFlags.ContainsKey(flagName))
        {
            eventFlags[flagName] = value;
        }
        else
        {
            eventFlags.Add(flagName, value);
        }
        
        Debug.Log($"Event flag '{flagName}' set to {value}");
    }
    
    public bool GetEventFlag(string flagName)
    {
        if (eventFlags.ContainsKey(flagName))
        {
            return eventFlags[flagName];
        }
        return false;
    }
    
    public void ClearAllFlags()
    {
        eventFlags.Clear();
        Debug.Log("All event flags cleared");
    }
    
    public void ClearFlag(string flagName)
    {
        if (eventFlags.ContainsKey(flagName))
        {
            eventFlags.Remove(flagName);
            Debug.Log($"Event flag '{flagName}' cleared");
        }
    }
}
