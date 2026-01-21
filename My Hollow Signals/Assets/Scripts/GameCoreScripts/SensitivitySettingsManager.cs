/*******************************************************
 * Author: [Ignacio Lopez]
 * Last Modified: [21/11/2025]
 * Description:
 *    Manages sensitivity/settings panel and UI navigation between pause menu and settings.
 *******************************************************/
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SensitivitySettingsManager : MonoBehaviour
{
    [Header("UI Navigation")]
    public GameObject settingsPanel;
    public List<GameObject> pauseMenuElements = new List<GameObject>(); // Drag pause menu UI elements here

    [Header("UI Elements")]
    public Slider mouseSensitivitySlider;
    public Slider gamepadSensitivitySlider;
    public Button backButton;

    [Header("Settings Display")]
    public TMPro.TextMeshProUGUI mouseSensitivityText;
    public TMPro.TextMeshProUGUI gamepadSensitivityText;

    [Header("Default Values")]
    public float defaultMouseSensitivity = 1f;
    public float defaultGamepadSensitivity = 3f;

    [Header("Sensitivity Ranges")]
    public float minSensitivity = 0.1f;
    public float maxSensitivity = 5f;

    private const string MOUSE_SENSITIVITY_KEY = "MouseSensitivity";
    private const string GAMEPAD_SENSITIVITY_KEY = "GamepadSensitivity";

    private FirstPersonController playerController;

    void Awake()
    {
        playerController = FindObjectOfType<FirstPersonController>();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    void Start()
    {
        SetupSliders();
        SetupButtonEvents();
        LoadSettings();
        UpdateDisplayTexts();
    }

    void SetupSliders()
    {
        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.minValue = minSensitivity;
            mouseSensitivitySlider.maxValue = maxSensitivity;
            mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
        }

        if (gamepadSensitivitySlider != null)
        {
            gamepadSensitivitySlider.minValue = minSensitivity;
            gamepadSensitivitySlider.maxValue = maxSensitivity;
            gamepadSensitivitySlider.onValueChanged.AddListener(OnGamepadSensitivityChanged);
        }
    }

    void SetupButtonEvents()
    {
        if (backButton != null)
            backButton.onClick.AddListener(CloseSettings);
    }

    public void LoadSettings()
    {
        float mouseSens = PlayerPrefs.GetFloat(MOUSE_SENSITIVITY_KEY, defaultMouseSensitivity);
        float gamepadSens = PlayerPrefs.GetFloat(GAMEPAD_SENSITIVITY_KEY, defaultGamepadSensitivity);

        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.value = mouseSens;

        if (gamepadSensitivitySlider != null)
            gamepadSensitivitySlider.value = gamepadSens;

        ApplySettingsToPlayer(mouseSens, gamepadSens);
    }

    public void SaveSettings()
    {
        if (mouseSensitivitySlider != null)
            PlayerPrefs.SetFloat(MOUSE_SENSITIVITY_KEY, mouseSensitivitySlider.value);

        if (gamepadSensitivitySlider != null)
            PlayerPrefs.SetFloat(GAMEPAD_SENSITIVITY_KEY, gamepadSensitivitySlider.value);

        PlayerPrefs.Save();
    }

    void OnMouseSensitivityChanged(float value)
    {
        if (playerController != null)
            playerController.mouseSensitivity = value;

        UpdateDisplayTexts();
        SaveSettings();
    }

    void OnGamepadSensitivityChanged(float value)
    {
        if (playerController != null)
            playerController.gamepadSensitivity = value;

        UpdateDisplayTexts();
        SaveSettings();
    }

    void UpdateDisplayTexts()
    {
        if (mouseSensitivityText != null && mouseSensitivitySlider != null)
            mouseSensitivityText.text = $"Mouse: {mouseSensitivitySlider.value:F1}";

        if (gamepadSensitivityText != null && gamepadSensitivitySlider != null)
            gamepadSensitivityText.text = $"Gamepad: {gamepadSensitivitySlider.value:F1}";
    }

    void ApplySettingsToPlayer(float mouseSens, float gamepadSens)
    {
        if (playerController != null)
        {
            playerController.mouseSensitivity = mouseSens;
            playerController.gamepadSensitivity = gamepadSens;
        }
    }

    public void OpenSettings()
    {
        // Hide main pause menu elements
        foreach (GameObject element in pauseMenuElements)
        {
            if (element != null)
                element.SetActive(false);
        }

        // Show settings panel
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);

            if (mouseSensitivitySlider != null)
                mouseSensitivitySlider.Select();
        }
    }

    public void CloseSettings()
    {
        // Hide settings panel
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        // Show main pause menu elements
        foreach (GameObject element in pauseMenuElements)
        {
            if (element != null)
                element.SetActive(true);
        }

        // Try to select options button for controller navigation
        Button optionsButton = FindObjectOfType<PauseMenuManager>()?.optionsButton;
        if (optionsButton != null)
            optionsButton.Select();
    }

    public void ResetToDefaults()
    {
        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.value = defaultMouseSensitivity;

        if (gamepadSensitivitySlider != null)
            gamepadSensitivitySlider.value = defaultGamepadSensitivity;

        SaveSettings();
    }

    public static float GetMouseSensitivity()
    {
        return PlayerPrefs.GetFloat(MOUSE_SENSITIVITY_KEY, 1f);
    }

    public static float GetGamepadSensitivity()
    {
        return PlayerPrefs.GetFloat(GAMEPAD_SENSITIVITY_KEY, 3f);
    }
}