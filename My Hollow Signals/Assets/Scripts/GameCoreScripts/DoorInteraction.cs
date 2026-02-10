/*******************************************************
 * Author: [Bianca Marinica]
 * Last Modified: [23/01/2026]
 * Description:
 *   Handles player interaction with doors, allowing them to open and close. It also emits noise when the door is used, which can affect the player's sanity if they are nearby. The script supports smooth opening/closing animations and optional audio feedback for door sounds. Additionally, it includes UnityEvents for custom behavior when the door is opened or closed.
 *******************************************************/

using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(EnvironmentalNoiseEmitter))]
public class DoorInteraction : MonoBehaviour
{
    [Header("Door Settings")]
    public bool isOpen = false;
    public float openAngle = 90f;
    public float openSpeed = 2f;
    
    [Header("Noise Settings")]
    [Tooltip("Sanity impact when opening/closing door")]
    [Range(0f, 1f)]
    public float doorNoiseIntensity = 0.3f;
    
    [Header("Audio (Optional)")]
    public AudioSource doorAudioSource;
    public AudioClip openSound;
    public AudioClip closeSound;
    
    [Header("Events")]
    public UnityEvent onDoorOpen;
    public UnityEvent onDoorClose;
    
    private EnvironmentalNoiseEmitter noiseEmitter;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isAnimating = false;
    
    void Awake()
    {
        noiseEmitter = GetComponent<EnvironmentalNoiseEmitter>();
        closedRotation = transform.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
    }
    
    void Update()
    {
        if (isAnimating)
        {
            Quaternion targetRotation = isOpen ? openRotation : closedRotation;
            transform.localRotation = Quaternion.Lerp(
                transform.localRotation, 
                targetRotation, 
                Time.deltaTime * openSpeed
            );
            
            if (Quaternion.Angle(transform.localRotation, targetRotation) < 0.5f)
            {
                transform.localRotation = targetRotation;
                isAnimating = false;
            }
        }
    }
    
    public void ToggleDoor()
    {
        if (isAnimating)
        {
            return;
        }
        
        isOpen = !isOpen;
        isAnimating = true;
        
        noiseEmitter.EmitNoise(doorNoiseIntensity);
        
        if (isOpen)
        {
            if (doorAudioSource != null && openSound != null)
            {
                doorAudioSource.PlayOneShot(openSound);
            }
            onDoorOpen.Invoke();
        }
        else
        {
            if (doorAudioSource != null && closeSound != null)
            {
                doorAudioSource.PlayOneShot(closeSound);
            }
            onDoorClose.Invoke();
        }
    }
    
    public void OpenDoor()
    {
        if (!isOpen && !isAnimating)
        {
            ToggleDoor();
        }
    }
    
    public void CloseDoor()
    {
        if (isOpen && !isAnimating)
        {
            ToggleDoor();
        }
    }
}
