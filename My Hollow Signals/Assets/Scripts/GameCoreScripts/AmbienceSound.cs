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
    
    private float fixedYPosition;
    private AudioSource[] audioSources;
    private float[] originalVolumes;
    private float targetVolumeMultiplier = 1f;
    private float currentVolumeMultiplier = 1f;

    void Start()
    {
        if (Player != null && ignoreVerticalPosition)
        {
            fixedYPosition = Player.transform.position.y;
        }
        
        audioSources = GetComponents<AudioSource>();
        originalVolumes = new float[audioSources.Length];
        
        for (int i = 0; i < audioSources.Length; i++)
        {
            originalVolumes[i] = audioSources[i].volume;
        }
    }

    void Update()
    {
        if (Player == null) return;
        
        Vector3 trackPosition = Player.transform.position;
        
        if (ignoreVerticalPosition)
        {
            trackPosition.y = fixedYPosition;
        }
        
        // Check if player is inside the zone
        Vector3 closestPoint = Area.ClosestPoint(trackPosition);
        bool isInside = Vector3.Distance(closestPoint, trackPosition) < 0.01f;
        
        // Set target volume based on whether player is inside
        targetVolumeMultiplier = isInside ? insideVolume : outsideVolume;
        
        // Smoothly transition volume
        currentVolumeMultiplier = Mathf.Lerp(currentVolumeMultiplier, targetVolumeMultiplier, Time.deltaTime * fadeSpeed);
        
        // Apply volume to all audio sources
        for (int i = 0; i < audioSources.Length; i++)
        {
            if (audioSources[i] != null)
            {
                audioSources[i].volume = originalVolumes[i] * currentVolumeMultiplier;
            }
        }
        
        // Set position to closest point to the player (for 3D audio panning)
        transform.position = closestPoint;
    }
}