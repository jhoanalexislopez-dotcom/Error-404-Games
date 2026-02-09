using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Localization.Settings;
using UnityEngine.EventSystems;
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
    [SerializeField] private Slider microphoneSensitivitySlider;
    [SerializeField] private TMP_Text microphoneSensitivityText;

    [Header("Controls Settings")]
    public Slider mouseSensitivitySlider;
    public Slider gamepadSensitivitySlider;
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
    private const string MIC_SENSITIVITY_KEY = "MicrophoneSensitivity";

    private const float DEFAULT_MOUSE_SENSITIVITY = 1f;
    private const float DEFAULT_GAMEPAD_SENSITIVITY = 3f;
    private const float DEFAULT_GAMMA = 2.2f;
    private const float DEFAULT_MIC_SENSITIVITY = 0.01f;

    private MicrophoneSelector microphoneSelector;
    private MenuManager menuManager;
    private bool isInitialized = false;

    private void OnEnable()
    {
        Debug.Log("OptionsMenuController: OnEnable() called");
        if (!isInitialized)
        {
            menuManager = FindObjectOfType<MenuManager>();
            SetupButtons();
            ShowGameplayPanel();
            isInitialized = true;
        }
        StartCoroutine(InitializeSettingsCoroutine());
        SetInitialSelection();
    }

    public void SetInitialSelection()
    {
        if (gameplayButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(gameplayButton.gameObject);
        }
    }

    private IEnumerator InitializeSettingsCoroutine()
    {
        yield return StartCoroutine(InitializeLocalization());
        InitializeMicrophoneDropdown();
        LoadControlsSettings();
        LoadGraphicsSettings();
        LoadAudioSettings();
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

    private IEnumerator InitializeLocalization()
    {
        Debug.Log("OptionsMenuController: Waiting for localization to initialize...");
        yield return LocalizationSettings.InitializationOperation;

        Debug.Log("Localization initialized");

        if (languageDropdown != null)
        {
            Debug.Log($"Language dropdown: interactable={languageDropdown.interactable}, isActiveAndEnabled={languageDropdown.isActiveAndEnabled}");
            
            var options = new List<TMP_Dropdown.OptionData>();
            int selected = 0;
            int savedLanguageIndex = PlayerPrefs.GetInt(LANGUAGE_KEY, 0);

            Debug.Log($"Available locales count: {LocalizationSettings.AvailableLocales.Locales.Count}");

            for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; ++i)
            {
                var locale = LocalizationSettings.AvailableLocales.Locales[i];
                if (LocalizationSettings.SelectedLocale == locale)
                    selected = i;
                options.Add(new TMP_Dropdown.OptionData(locale.name));
                Debug.Log($"Added locale {i}: {locale.name}");
            }

            Debug.Log($"Saved language index: {savedLanguageIndex}, Options count: {options.Count}");

            languageDropdown.ClearOptions();
            languageDropdown.AddOptions(options);
            languageDropdown.value = savedLanguageIndex;
            languageDropdown.RefreshShownValue();
            
            languageDropdown.onValueChanged.RemoveListener(OnLanguageChanged);
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);

            Debug.Log($"Language dropdown initialized with value: {languageDropdown.value}, options: {languageDropdown.options.Count}");

            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[savedLanguageIndex];
        }
        else
        {
            Debug.LogError("Language dropdown is null!");
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
                microphoneDropdown.ClearOptions();
                microphoneDropdown.AddOptions(options);
                microphoneDropdown.value = Mathf.Clamp(savedMicIndex, 0, options.Count - 1);
                microphoneDropdown.RefreshShownValue();
                
                microphoneDropdown.onValueChanged.RemoveListener(OnMicrophoneChanged);
                microphoneDropdown.onValueChanged.AddListener(OnMicrophoneChanged);
            }
        }

        float micSensitivity = PlayerPrefs.GetFloat(MIC_SENSITIVITY_KEY, DEFAULT_MIC_SENSITIVITY);
        if (microphoneSensitivitySlider != null)
        {
            microphoneSensitivitySlider.value = micSensitivity;
            
            microphoneSensitivitySlider.onValueChanged.RemoveListener(OnMicrophoneSensitivityChanged);
            microphoneSensitivitySlider.onValueChanged.AddListener(OnMicrophoneSensitivityChanged);
        }
        UpdateMicrophoneSensitivityText();

        microphoneSelector = FindObjectOfType<MicrophoneSelector>();
    }

    private void LoadControlsSettings()
    {
        float mouseSens = PlayerPrefs.GetFloat(MOUSE_SENSITIVITY_KEY, DEFAULT_MOUSE_SENSITIVITY);
        float gamepadSens = PlayerPrefs.GetFloat(GAMEPAD_SENSITIVITY_KEY, DEFAULT_GAMEPAD_SENSITIVITY);

        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.value = mouseSens;
            
            mouseSensitivitySlider.onValueChanged.RemoveListener(OnMouseSensitivityChanged);
            mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
        }

        if (gamepadSensitivitySlider != null)
        {
            gamepadSensitivitySlider.value = gamepadSens;
            
            gamepadSensitivitySlider.onValueChanged.RemoveListener(OnGamepadSensitivityChanged);
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
            
            gammaSlider.onValueChanged.RemoveListener(OnGammaChanged);
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
                
                masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (bgmVolumeSlider != null)
            {
                audioMixer.GetFloat("bgmVolume", out bgmValue);
                bgmVolumeSlider.value = bgmValue;
                
                bgmVolumeSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
                bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            }

            if (sfxVolumeSlider != null)
            {
                audioMixer.GetFloat("sfxVolume", out sfxValue);
                sfxVolumeSlider.value = sfxValue;
                
                sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
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

    private void OnMicrophoneSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat(MIC_SENSITIVITY_KEY, value);
        PlayerPrefs.Save();
        UpdateMicrophoneSensitivityText();
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

    private void UpdateMicrophoneSensitivityText()
    {
        if (microphoneSensitivityText != null && microphoneSensitivitySlider != null)
            microphoneSensitivityText.text = microphoneSensitivitySlider.value.ToString("F3");
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
        
        if (EventSystem.current != null && languageDropdown != null)
        {
            EventSystem.current.SetSelectedGameObject(languageDropdown.gameObject);
        }
    }

    public void ShowControlsPanel()
    {
        SetActivePanel(controlsPanel);
        HighlightButton(controlsButton);
        
        if (EventSystem.current != null && mouseSensitivitySlider != null)
        {
            EventSystem.current.SetSelectedGameObject(mouseSensitivitySlider.gameObject);
        }
    }

    public void ShowGraphicsPanel()
    {
        SetActivePanel(graphicsPanel);
        HighlightButton(graphicsButton);
        
        if (EventSystem.current != null && gammaSlider != null)
        {
            EventSystem.current.SetSelectedGameObject(gammaSlider.gameObject);
        }
    }

    public void ShowAudioPanel()
    {
        SetActivePanel(audioPanel);
        HighlightButton(audioButton);
        
        if (EventSystem.current != null && masterVolumeSlider != null)
        {
            EventSystem.current.SetSelectedGameObject(masterVolumeSlider.gameObject);
        }
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

        if (microphoneSensitivitySlider != null)
            microphoneSensitivitySlider.value = DEFAULT_MIC_SENSITIVITY;

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
