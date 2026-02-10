/*******************************************************
 * Author: [Ignacio Lopez]
 * Last Modified: [31/01/2026]
 * Description:
 * This script displays the current frames per second (FPS) in a TextMeshProUGUI element. It calculates the FPS by counting the number of frames rendered over a specified update interval and updates the display accordingly. The script also includes functionality to toggle the visibility of the FPS counter, allowing developers to monitor performance during testing and debugging sessions without cluttering the screen when not needed. 
 *******************************************************/

using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    private const float UPDATE_INTERVAL = 0.1f;
    private const string FPS_FORMAT = "FPS: {0:0}";
    
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private bool showOnStart = true;
    
    private float deltaTime;
    private float timeSinceUpdate;
    private int frameCount;

    private void Start()
    {
        if (!showOnStart && fpsText != null)
        {
            fpsText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        deltaTime += Time.unscaledDeltaTime;
        frameCount++;
        timeSinceUpdate += Time.unscaledDeltaTime;

        if (timeSinceUpdate >= UPDATE_INTERVAL)
        {
            float fps = frameCount / deltaTime;
            UpdateFPSDisplay(fps);
            
            deltaTime = 0f;
            frameCount = 0;
            timeSinceUpdate = 0f;
        }
    }

    /// <summary>
    /// Updates the FPS text display with the current frame rate.
    /// </summary>
    private void UpdateFPSDisplay(float fps)
    {
        if (fpsText != null)
        {
            fpsText.text = string.Format(FPS_FORMAT, fps);
        }
    }

    /// <summary>
    /// Toggles the visibility of the FPS counter.
    /// </summary>
    public void ToggleDisplay()
    {
        if (fpsText != null)
        {
            fpsText.gameObject.SetActive(!fpsText.gameObject.activeSelf);
        }
    }

    /// <summary>
    /// Sets the visibility of the FPS counter.
    /// </summary>
    public void SetDisplayActive(bool active)
    {
        if (fpsText != null)
        {
            fpsText.gameObject.SetActive(active);
        }
    }
}
