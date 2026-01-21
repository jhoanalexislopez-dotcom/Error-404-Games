/*******************************************************
 * Author: [Alejandro Vila]
 * Last Modified: [21/11/2025]
 * Description:
 *    Centralized manager providing default sound effects for all buttons.
 *******************************************************/

using UnityEngine;
using UnityEngine.UI;

public class ButtonSFXManager : MonoBehaviour
{
    [Header("Default Sound Effects")]
    [Tooltip("Default hover sound for all buttons")]
    public AudioClip defaultHoverSound;

    [Tooltip("Default click sound for all buttons")]
    public AudioClip defaultClickSound;

    [Header("Default Highlight")]
    [Tooltip("Default highlight prefab for all buttons")]
    public GameObject defaultHighlightPrefab;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float defaultHoverVolume = 0.7f;

    [Range(0f, 1f)]
    public float defaultClickVolume = 1f;

    [Header("Setup Options")]
    [Tooltip("Automatically add ButtonSFX to all buttons on Start")]
    public bool autoSetupAllButtons = true;

    [Tooltip("Automatically add ButtonHighlight to all buttons on Start")]
    public bool autoSetupHighlights = true;

    [Tooltip("Automatically add ButtonPressDown to all buttons on Start")]
    public bool autoSetupPressDown = true;

    [Tooltip("Only setup buttons that don't already have ButtonSFX")]
    public bool skipExistingButtonSFX = true;

    [Tooltip("Only setup buttons that don't already have ButtonHighlight")]
    public bool skipExistingButtonHighlight = true;

    [Tooltip("Only setup buttons that don't already have ButtonPressDown")]
    public bool skipExistingButtonPressDown = true;

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

        Debug.Log($"ButtonSFXManager: Set up audio, highlights, and press down for {allButtons.Length} buttons.");
    }

    public void SetupButton(Button button)
    {
        if (button == null) return;

        SetupButtonSFX(button);

        if (autoSetupHighlights)
        {
            SetupButtonHighlight(button);
        }

        if (autoSetupPressDown)
        {
            SetupButtonPressDown(button);
        }
    }

    private void SetupButtonSFX(Button button)
    {
        ButtonSFX existingButtonSFX = button.GetComponent<ButtonSFX>();

        if (existingButtonSFX != null && skipExistingButtonSFX)
        {
            return;
        }

        if (existingButtonSFX == null)
        {
            existingButtonSFX = button.gameObject.AddComponent<ButtonSFX>();
        }

        if (existingButtonSFX.hoverSound == null)
            existingButtonSFX.hoverSound = defaultHoverSound;

        if (existingButtonSFX.clickSound == null)
            existingButtonSFX.clickSound = defaultClickSound;

        existingButtonSFX.hoverVolume = defaultHoverVolume;
        existingButtonSFX.clickVolume = defaultClickVolume;
    }

    private void SetupButtonHighlight(Button button)
    {
        ButtonHighlight existingButtonHighlight = button.GetComponent<ButtonHighlight>();

        if (existingButtonHighlight != null && skipExistingButtonHighlight)
        {
            return;
        }

        if (existingButtonHighlight == null)
        {
            existingButtonHighlight = button.gameObject.AddComponent<ButtonHighlight>();
        }

        if (existingButtonHighlight.highlightPrefab == null)
            existingButtonHighlight.highlightPrefab = defaultHighlightPrefab;
    }

    private void SetupButtonPressDown(Button button)
    {
        ButtonPressDown existingButtonPressDown = button.GetComponent<ButtonPressDown>();

        if (existingButtonPressDown != null && skipExistingButtonPressDown)
        {
            return;
        }

        if (existingButtonPressDown == null)
        {
            existingButtonPressDown = button.gameObject.AddComponent<ButtonPressDown>();
        }
    }

    [ContextMenu("Setup Only Press Down")]
    public void SetupAllButtonPressDown()
    {
        Button[] allButtons = FindObjectsOfType<Button>(true);

        foreach (Button button in allButtons)
        {
            SetupButtonPressDown(button);
        }

        Debug.Log($"ButtonSFXManager: Set up press down behavior for {allButtons.Length} buttons.");
    }
}
