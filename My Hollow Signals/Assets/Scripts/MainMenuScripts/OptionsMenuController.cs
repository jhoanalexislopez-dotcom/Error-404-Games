using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Localization.Settings;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class OptionsMenuController : MonoBehaviour
{
    [Header("Submenu Panels")]
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject graphicsPanel;
    [SerializeField] private GameObject audioPanel;

    [Header("Submenu Buttons")]
    [SerializeField] private Button gameplayButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button graphicsButton;
    [SerializeField] private Button audioButton;
    [SerializeField] private Button restoreDefaultsButton;
    [SerializeField] private Button exitButton;

    [Header("Gameplay Settings")]
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private TMP_Dropdown microphoneDropdown;

    [Header("Controls Settings")]
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Slider gamepadSensitivitySlider;
    [SerializeField] private TMP_Text mouseSensitivityText;
    [SerializeField] private TMP_Text gamepadSensitivityText;

    [Header("Graphics Settings")]
    [SerializeField] private Slider gammaSlider;
    [SerializeField] private TMP_Text gammaValueText;
    [SerializeField] private Image gammaPreviewImage;

    [Header("Audio Settings")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TMP_Text masterVolumeText;
    [SerializeField] private TMP_Text bgmVolumeText;
    [SerializeField] private TMP_Text sfxVolumeText;
    [SerializeField] private AudioMixer audioMixer;

    [Header("Microphone Audio Meter")]
    [SerializeField] private GameObject audioMeterGroup;

    private const string MOUSE_SENSITIVITY_KEY = "MouseSensitivity";
    private const string GAMEPAD_SENSITIVITY_KEY = "GamepadSensitivity";
    private const string GAMMA_KEY = "GammaCorrection";
    private const string LANGUAGE_KEY = "SelectedLanguage";
    private const string MICROPHONE_KEY = "SelectedMicrophone";

    private const float DEFAULT_MOUSE_SENSITIVITY = 1f;
    private const float DEFAULT_GAMEPAD_SENSITIVITY = 3f;
    private const float DEFAULT_GAMMA = 2.2f;

    private MicrophoneSelector microphoneSelector;
    private MenuManager menuManager;

    private void Start()
    {
        menuManager = FindObjectOfType<MenuManager>();
        SetupButtons();
        InitializeSettings();
        ShowGameplayPanel();
    }

    private void SetupButtons()
    {
        if (gameplayButton != null)
            gameplayButton.onClick.AddListener(ShowGameplayPanel);

        if (controlsButton != null)
            controlsButton.onClick.AddListener(ShowControlsPanel);

        if (graphicsButton != null)
            graphicsButton.onClick.AddListener(ShowGraphicsPanel);

        if (audioButton != null)
            audioButton.onClick.AddListener(ShowAudioPanel);

        if (restoreDefaultsButton != null)
            restoreDefaultsButton.onClick.AddListener(RestoreDefaults);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitPressed);
    }

    private void InitializeSettings()
    {
        StartCoroutine(InitializeLocalization());
        InitializeMicrophoneDropdown();
        LoadControlsSettings();
        LoadGraphicsSettings();
        LoadAudioSettings();
    }

    private IEnumerator InitializeLocalization()
    {
        yield return LocalizationSettings.InitializationOperation;

        if (languageDropdown != null)
        {
            var options = new List<TMP_Dropdown.OptionData>();
            int selected = 0;
            int savedLanguageIndex = PlayerPrefs.GetInt(LANGUAGE_KEY, 0);

            for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; ++i)
            {
                var locale = LocalizationSettings.AvailableLocales.Locales[i];
                if (LocalizationSettings.SelectedLocale == locale)
                    selected = i;
                options.Add(new TMP_Dropdown.OptionData(locale.name));
            }

            languageDropdown.options = options;
            languageDropdown.value = savedLanguageIndex;
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);

            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[savedLanguageIndex];
        }
    }

    private void InitializeMicrophoneDropdown()
    {
        if (microphoneDropdown != null)
        {
            var options = new List<TMP_Dropdown.OptionData>();
            int savedMicIndex = PlayerPrefs.GetInt(MICROPHONE_KEY, 0);

            foreach (var microphone in Microphone.devices)
            {
                options.Add(new TMP_Dropdown.OptionData(microphone));
            }

            if (options.Count > 0)
            {
                microphoneDropdown.options = options;
                microphoneDropdown.value = Mathf.Clamp(savedMicIndex, 0, options.Count - 1);
                microphoneDropdown.onValueChanged.AddListener(OnMicrophoneChanged);
            }
        }

        microphoneSelector = FindObjectOfType<MicrophoneSelector>();
    }

    private void LoadControlsSettings()
    {
        float mouseSens = PlayerPrefs.GetFloat(MOUSE_SENSITIVITY_KEY, DEFAULT_MOUSE_SENSITIVITY);
        float gamepadSens = PlayerPrefs.GetFloat(GAMEPAD_SENSITIVITY_KEY, DEFAULT_GAMEPAD_SENSITIVITY);

        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.value = mouseSens;
            mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
        }

        if (gamepadSensitivitySlider != null)
        {
            gamepadSensitivitySlider.value = gamepadSens;
            gamepadSensitivitySlider.onValueChanged.AddListener(OnGamepadSensitivityChanged);
        }

        UpdateSensitivityTexts();
    }

    private void LoadGraphicsSettings()
    {
        float gamma = PlayerPrefs.GetFloat(GAMMA_KEY, DEFAULT_GAMMA);

        if (gammaSlider != null)
        {
            gammaSlider.value = gamma;
            gammaSlider.onValueChanged.AddListener(OnGammaChanged);
        }

        UpdateGammaDisplay();
        ApplyGamma(gamma);
    }

    private void LoadAudioSettings()
    {
        if (audioMixer != null)
        {
            float masterValue, bgmValue, sfxValue;

            if (masterVolumeSlider != null)
            {
                audioMixer.GetFloat("masterVolume", out masterValue);
                masterVolumeSlider.value = masterValue;
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (bgmVolumeSlider != null)
            {
                audioMixer.GetFloat("bgmVolume", out bgmValue);
                bgmVolumeSlider.value = bgmValue;
                bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            }

            if (sfxVolumeSlider != null)
            {
                audioMixer.GetFloat("sfxVolume", out sfxValue);
                sfxVolumeSlider.value = sfxValue;
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
        }

        UpdateVolumeTexts();
    }

    private void OnLanguageChanged(int index)
    {
        if (index >= 0 && index < LocalizationSettings.AvailableLocales.Locales.Count)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
            PlayerPrefs.SetInt(LANGUAGE_KEY, index);
            PlayerPrefs.Save();
        }
    }

    private void OnMicrophoneChanged(int index)
    {
        PlayerPrefs.SetInt(MICROPHONE_KEY, index);
        PlayerPrefs.Save();

        if (microphoneSelector != null)
        {
            microphoneSelector.ChooseMicrophone(index);
        }
    }

    private void OnMouseSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat(MOUSE_SENSITIVITY_KEY, value);
        PlayerPrefs.Save();
        UpdateSensitivityTexts();
    }

    private void OnGamepadSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat(GAMEPAD_SENSITIVITY_KEY, value);
        PlayerPrefs.Save();
        UpdateSensitivityTexts();
    }

    private void OnGammaChanged(float value)
    {
        PlayerPrefs.SetFloat(GAMMA_KEY, value);
        PlayerPrefs.Save();
        UpdateGammaDisplay();
        ApplyGamma(value);
    }

    private void OnMasterVolumeChanged(float value)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat("masterVolume", value);
            UpdateVolumeTexts();
        }
    }

    private void OnBGMVolumeChanged(float value)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat("bgmVolume", value);
            UpdateVolumeTexts();
        }
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat("sfxVolume", value);
            UpdateVolumeTexts();
        }
    }

    private void UpdateSensitivityTexts()
    {
        if (mouseSensitivityText != null && mouseSensitivitySlider != null)
            mouseSensitivityText.text = mouseSensitivitySlider.value.ToString("F2");

        if (gamepadSensitivityText != null && gamepadSensitivitySlider != null)
            gamepadSensitivityText.text = gamepadSensitivitySlider.value.ToString("F2");
    }

    private void UpdateGammaDisplay()
    {
        if (gammaValueText != null && gammaSlider != null)
            gammaValueText.text = gammaSlider.value.ToString("F2");
    }

    private void UpdateVolumeTexts()
    {
        if (masterVolumeText != null && masterVolumeSlider != null)
            masterVolumeText.text = LinearToPercentage(masterVolumeSlider.value);

        if (bgmVolumeText != null && bgmVolumeSlider != null)
            bgmVolumeText.text = LinearToPercentage(bgmVolumeSlider.value);

        if (sfxVolumeText != null && sfxVolumeSlider != null)
            sfxVolumeText.text = LinearToPercentage(sfxVolumeSlider.value);
    }

    private string LinearToPercentage(float linearValue)
    {
        float percentage = Mathf.InverseLerp(-80f, 0f, linearValue) * 100f;
        return percentage.ToString("F0") + "%";
    }

    private void ApplyGamma(float gamma)
    {
        if (gammaPreviewImage != null)
        {
            Color gammaColor = new Color(gamma / 2.2f, gamma / 2.2f, gamma / 2.2f, 1f);
            gammaPreviewImage.color = gammaColor;
        }
    }

    public void ShowGameplayPanel()
    {
        SetActivePanel(gameplayPanel);
        HighlightButton(gameplayButton);
    }

    public void ShowControlsPanel()
    {
        SetActivePanel(controlsPanel);
        HighlightButton(controlsButton);
    }

    public void ShowGraphicsPanel()
    {
        SetActivePanel(graphicsPanel);
        HighlightButton(graphicsButton);
    }

    public void ShowAudioPanel()
    {
        SetActivePanel(audioPanel);
        HighlightButton(audioButton);
    }

    private void SetActivePanel(GameObject activePanel)
    {
        if (gameplayPanel != null) gameplayPanel.SetActive(gameplayPanel == activePanel);
        if (controlsPanel != null) controlsPanel.SetActive(controlsPanel == activePanel);
        if (graphicsPanel != null) graphicsPanel.SetActive(graphicsPanel == activePanel);
        if (audioPanel != null) audioPanel.SetActive(audioPanel == activePanel);
    }

    private void HighlightButton(Button button)
    {
        // Visual feedback can be handled via button states or custom highlighting
    }

    public void RestoreDefaults()
    {
        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.value = DEFAULT_MOUSE_SENSITIVITY;

        if (gamepadSensitivitySlider != null)
            gamepadSensitivitySlider.value = DEFAULT_GAMEPAD_SENSITIVITY;

        if (gammaSlider != null)
            gammaSlider.value = DEFAULT_GAMMA;

        if (audioMixer != null)
        {
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = 0f;
                audioMixer.SetFloat("masterVolume", 0f);
            }

            if (bgmVolumeSlider != null)
            {
                bgmVolumeSlider.value = 0f;
                audioMixer.SetFloat("bgmVolume", 0f);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = 0f;
                audioMixer.SetFloat("sfxVolume", 0f);
            }
        }

        if (languageDropdown != null)
            languageDropdown.value = 0;

        if (microphoneDropdown != null)
            microphoneDropdown.value = 0;

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        UpdateSensitivityTexts();
        UpdateGammaDisplay();
        UpdateVolumeTexts();
    }

    private void OnExitPressed()
    {
        gameObject.SetActive(false);
        
        if (menuManager != null)
        {
            menuManager.ShowMainMenu();
        }
    }
}
