/*******************************************************
 * Author: [Ignacio López]
 * Last Modified: [30/01/2026]
 * Description:
 *   This script manages ambient sounds in the game, allowing for smooth volume transitions as the player enters or exits defined zones. It uses a trigger collider to determine when the player is within the area of effect and adjusts the volume of attached audio sources accordingly. The script also supports ignoring vertical position changes to prevent crouching from affecting the sound, and it smoothly fades between inside and outside volumes for a more immersive experience.
 *******************************************************/

using UnityEngine;

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

    [Header("Event Requirements")]
    [Tooltip("Optional event flag that must be true for this ambience to play")]
    public string requiredEventFlag = "";
    
    private float fixedYPosition;
    private AudioSource[] audioSources;
    private float[] originalVolumes;
    private float targetVolumeMultiplier = 1f;
    private float currentVolumeMultiplier = 1f;

    void Start()
    {
        audioSources = GetComponents<AudioSource>();
        originalVolumes = new float[audioSources.Length];
        
        for (int i = 0; i < audioSources.Length; i++)
        {
            originalVolumes[i] = audioSources[i].volume;
        }

        if (Player != null)
        {
            if (ignoreVerticalPosition)
            {
                fixedYPosition = Player.transform.position.y;
            }

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
        
        if (flagRequirementMet && isInside)
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
}