using UnityEngine;
using UnityEngine.UI;

public class ButtonSFXManager : MonoBehaviour
{
    [Header("Default Sound Effects")]
    [Tooltip("Default hover sound for all buttons")]
    public AudioClip defaultHoverSound;

    [Tooltip("Default click sound for all buttons")]
    public AudioClip defaultClickSound;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float defaultHoverVolume = 0.7f;

    [Range(0f, 1f)]
    public float defaultClickVolume = 1f;

    [Header("Setup Options")]
    [Tooltip("Automatically add ButtonSFX to all buttons on Start")]
    public bool autoSetupAllButtons = true;

    [Tooltip("Only setup buttons that don't already have ButtonSFX")]
    public bool skipExistingButtonSFX = true;

    private void Start()
    {
        if (autoSetupAllButtons)
        {
            SetupAllButtons();
        }
    }

    [ContextMenu("Setup All Buttons")]
    public void SetupAllButtons()
    {
        Button[] allButtons = FindObjectsOfType<Button>(true);

        foreach (Button button in allButtons)
        {
            SetupButton(button);
        }

        Debug.Log($"ButtonSFXManager: Set up audio for {allButtons.Length} buttons.");
    }

    public void SetupButton(Button button)
    {
        if (button == null) return;

        ButtonSFX existingButtonSFX = button.GetComponent<ButtonSFX>();

        if (existingButtonSFX != null && skipExistingButtonSFX)
        {
            return; // Skip if already has ButtonSFX and we're set to skip existing
        }

        if (existingButtonSFX == null)
        {
            existingButtonSFX = button.gameObject.AddComponent<ButtonSFX>();
        }

        // Set default sounds if not already assigned
        if (existingButtonSFX.hoverSound == null)
            existingButtonSFX.hoverSound = defaultHoverSound;

        if (existingButtonSFX.clickSound == null)
            existingButtonSFX.clickSound = defaultClickSound;

        existingButtonSFX.hoverVolume = defaultHoverVolume;
        existingButtonSFX.clickVolume = defaultClickVolume;
    }

    public void SetDefaultSounds(AudioClip hoverSound, AudioClip clickSound)
    {
        defaultHoverSound = hoverSound;
        defaultClickSound = clickSound;
    }
}
