/*******************************************************
 * Author: [Ignacio López]
 * Last Modified: [30/01/2026]
 * Description:
 *   This script manages ambient sounds in the game, allowing for smooth volume transitions as the player enters or exits defined zones. It uses a trigger collider to determine when the player is within the area of effect and adjusts the volume of attached audio sources accordingly. The script also supports ignoring vertical position changes to prevent crouching from affecting the sound, and it smoothly fades between inside and outside volumes for a more immersive experience.
 *******************************************************/

using UnityEngine;
using System.Collections.Generic;

public class AmbienceSound : MonoBehaviour
{
    [Tooltip("Area of the sound to be in")]
    public Collider Area;
    [Tooltip("Character to track")]
    public GameObject Player;
    
    [Tooltip("If true, ignores vertical (Y) position changes to prevent crouch affecting volume")]
    public bool ignoreVerticalPosition = true;
    
    [Tooltip("Volume when inside the zone")]
    [Range(0f, 1f)]
    public float insideVolume = 1f;
    
    [Tooltip("Volume when outside the zone")]
    [Range(0f, 1f)]
    public float outsideVolume = 0f;
    
    [Tooltip("Speed of volume fade transition")]
    public float fadeSpeed = 2f;

    [Header("Priority Settings")]
    [Tooltip("Priority level - higher priority zones override lower priority ones when overlapping")]
    public int priority = 0;
    
    [Header("Debug")]
    [Tooltip("Enable debug logging for this ambience zone")]
    public bool debugMode = false;

    [Header("Event Requirements")]
    [Tooltip("Optional event flag that must be true for this ambience to play")]
    public string requiredEventFlag = "";
    
    private float fixedYPosition;
    private AudioSource[] audioSources;
    private float[] originalVolumes;
    private float targetVolumeMultiplier = 1f;
    private float currentVolumeMultiplier = 1f;
    
    private static List<AmbienceSound> allAmbienceSounds = new List<AmbienceSound>();

    void OnEnable()
    {
        if (!allAmbienceSounds.Contains(this))
        {
            allAmbienceSounds.Add(this);
        }
    }

    void OnDisable()
    {
        allAmbienceSounds.Remove(this);
    }

    void Start()
    {
        audioSources = GetComponents<AudioSource>();
        originalVolumes = new float[audioSources.Length];
        
        for (int i = 0; i < audioSources.Length; i++)
        {
            originalVolumes[i] = audioSources[i].volume;
        }

        if (ignoreVerticalPosition && Area != null)
        {
            fixedYPosition = Area.bounds.center.y;
        }

        if (Player != null)
        {
            InitializeVolume();
        }
        else
        {
            currentVolumeMultiplier = outsideVolume;
            targetVolumeMultiplier = outsideVolume;
            ApplyVolume();
        }
    }

    private void InitializeVolume()
    {
        Vector3 trackPosition = Player.transform.position;
        
        if (ignoreVerticalPosition)
        {
            trackPosition.y = fixedYPosition;
        }

        Vector3 closestPoint = Area.ClosestPoint(trackPosition);
        bool isInside = Vector3.Distance(closestPoint, trackPosition) < 0.01f;

        currentVolumeMultiplier = isInside ? insideVolume : outsideVolume;
        targetVolumeMultiplier = currentVolumeMultiplier;

        ApplyVolume();
        transform.position = closestPoint;
    }

    private void ApplyVolume()
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            if (audioSources[i] != null)
            {
                audioSources[i].volume = originalVolumes[i] * currentVolumeMultiplier;
            }
        }
    }

    void Update()
    {
        if (Player == null) return;

        bool flagRequirementMet = true;
        if (!string.IsNullOrEmpty(requiredEventFlag))
        {
            if (GameEventManager.Instance != null)
            {
                flagRequirementMet = GameEventManager.Instance.GetEventFlag(requiredEventFlag);
            }
            else
            {
                flagRequirementMet = false;
            }
        }
        
        Vector3 trackPosition = Player.transform.position;
        
        if (ignoreVerticalPosition)
        {
            trackPosition.y = fixedYPosition;
        }
        
        Vector3 closestPoint = Area.ClosestPoint(trackPosition);
        bool isInside = Vector3.Distance(closestPoint, trackPosition) < 0.01f;
        
        bool isOverridden = false;
        if (isInside && flagRequirementMet)
        {
            isOverridden = CheckIfOverriddenByHigherPriority(trackPosition);
        }
        
        if (debugMode)
        {
            Debug.Log($"[{gameObject.name}] isInside={isInside}, flagMet={flagRequirementMet}, isOverridden={isOverridden}, target={targetVolumeMultiplier}, current={currentVolumeMultiplier}");
        }
        
        if (flagRequirementMet && isInside && !isOverridden)
        {
            targetVolumeMultiplier = insideVolume;
        }
        else
        {
            targetVolumeMultiplier = outsideVolume;
        }
        
        currentVolumeMultiplier = Mathf.Lerp(currentVolumeMultiplier, targetVolumeMultiplier, Time.deltaTime * fadeSpeed);
        
        ApplyVolume();
        
        transform.position = closestPoint;
    }

    private bool CheckIfOverriddenByHigherPriority(Vector3 playerPosition)
    {
        foreach (AmbienceSound other in allAmbienceSounds)
        {
            if (other == this || other == null || other.Area == null || other.Player == null)
                continue;
            
            if (other.priority <= this.priority)
                continue;
            
            bool otherFlagRequirementMet = true;
            if (!string.IsNullOrEmpty(other.requiredEventFlag))
            {
                if (GameEventManager.Instance != null)
                {
                    otherFlagRequirementMet = GameEventManager.Instance.GetEventFlag(other.requiredEventFlag);
                }
                else
                {
                    otherFlagRequirementMet = false;
                }
            }
            
            if (!otherFlagRequirementMet)
                continue;
            
            Vector3 checkPosition = playerPosition;
            if (other.ignoreVerticalPosition)
            {
                checkPosition.y = other.Area.bounds.center.y;
            }
            
            Vector3 otherClosestPoint = other.Area.ClosestPoint(checkPosition);
            bool isInsideOther = Vector3.Distance(otherClosestPoint, checkPosition) < 0.01f;
            
            if (isInsideOther)
            {
                return true;
            }
        }
        
        return false;
    }
}