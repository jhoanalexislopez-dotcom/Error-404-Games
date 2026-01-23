using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MicrophoneStartupManager : MonoBehaviour
{
    [Header("UI Canvases")]
    [Tooltip("Your MicroSetting canvas with all mic controls")]
    [SerializeField] private GameObject microSettingCanvas;
    
    [Tooltip("Main menu canvas/panel")]
    [SerializeField] private GameObject mainMenuCanvas;

    [Header("No Microphone Elements")]
    [Tooltip("TextMeshPro to show 'No microphone detected' message (can be inside MicroSetting canvas)")]
    [SerializeField] private GameObject noMicTextObject;
    
    [Tooltip("Panels to show when mic IS detected (source, calibration, sens panels)")]
    [SerializeField] private GameObject[] micControlPanels;
    
    [Tooltip("Background image or other elements that should ALWAYS be visible")]
    [SerializeField] private GameObject[] alwaysVisibleElements;

    [Header("Buttons")]
    [Tooltip("Button to close the mic setup and go to main menu")]
    [SerializeField] private Button continueButton;

    [Header("Settings")]
    [SerializeField] private float startupDelay = 0.5f;
    [SerializeField] private bool showEveryTime = false;
    [SerializeField] private string noMicMessage = "No microphone detected.\n\nUse a microphone for a better experience!\n\nPress any button to continue.";

    private const string PREF_CALIBRATION_SHOWN = "MicCalibrationShown";
    private bool hasShownCalibration = false;

    public static bool HasCompletedCalibration => PlayerPrefs.GetInt(PREF_CALIBRATION_SHOWN, 0) == 1;

    private void Start()
    {
        Debug.Log("MicrophoneStartupManager: Start() called");
        Debug.Log($"MicrophoneStartupManager: HasCompletedCalibration = {HasCompletedCalibration}");
        Debug.Log($"MicrophoneStartupManager: showEveryTime = {showEveryTime}");
        
        SetupUI();
        
        bool shouldShow = showEveryTime || !HasCompletedCalibration;
        Debug.Log($"MicrophoneStartupManager: shouldShow = {shouldShow}");
        
        if (shouldShow)
        {
            StartCoroutine(ShowCalibrationAfterDelay());
        }
        else
        {
            Debug.Log("MicrophoneStartupManager: Calibration already completed, going to main menu.");
            GoToMainMenu();
        }
    }

    private IEnumerator ShowCalibrationAfterDelay()
    {
        Debug.Log($"MicrophoneStartupManager: Waiting {startupDelay} seconds before showing...");
        yield return new WaitForSeconds(startupDelay);
        ShowMicrophoneSetup();
    }

    private void SetupUI()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueToMainMenu);

        if (mainMenuCanvas != null)
            mainMenuCanvas.SetActive(false);
        
        if (microSettingCanvas != null)
            microSettingCanvas.SetActive(true);
        
        if (noMicTextObject != null)
            noMicTextObject.SetActive(false);
        
        foreach (var element in alwaysVisibleElements)
        {
            if (element != null)
                element.SetActive(true);
        }
            
        foreach (var panel in micControlPanels)
        {
            if (panel != null)
                panel.SetActive(false);
        }
    }

    private IEnumerator CheckAndShowCalibration()
    {
        Debug.Log($"MicrophoneStartupManager: Waiting {startupDelay} seconds before checking...");
        yield return new WaitForSeconds(startupDelay);

        bool shouldShow = showEveryTime || !HasCompletedCalibration;
        Debug.Log($"MicrophoneStartupManager: shouldShow = {shouldShow}");

        if (shouldShow)
        {
            ShowMicrophoneSetup();
        }
        else
        {
            Debug.Log("MicrophoneStartupManager: Calibration already completed, skipping.");
            GoToMainMenu();
        }
    }

    private void ShowMicrophoneSetup()
    {
        if (hasShownCalibration)
        {
            Debug.Log("MicrophoneStartupManager: Already shown calibration, skipping.");
            return;
        }

        hasShownCalibration = true;

        bool hasMicrophone = Microphone.devices.Length > 0;
        Debug.Log($"MicrophoneStartupManager: Microphone detected = {hasMicrophone}, Device count = {Microphone.devices.Length}");

        if (hasMicrophone)
        {
            Debug.Log("MicrophoneStartupManager: Showing microphone controls");
            ShowMicrophoneControls();
        }
        else
        {
            Debug.Log("MicrophoneStartupManager: Showing no microphone message");
            ShowNoMicrophoneMessage();
        }
    }

    private void ShowMicrophoneControls()
    {
        Debug.Log("MicrophoneStartupManager: ShowMicrophoneControls called");
        
        if (noMicTextObject != null)
        {
            noMicTextObject.SetActive(false);
            Debug.Log("MicrophoneStartupManager: NoMicText hidden");
        }

        foreach (var panel in micControlPanels)
        {
            if (panel != null)
            {
                panel.SetActive(true);
                Debug.Log($"MicrophoneStartupManager: Mic control panel '{panel.name}' shown");
            }
        }
    }

    private void ShowNoMicrophoneMessage()
    {
        if (noMicTextObject != null)
        {
            noMicTextObject.SetActive(true);
            
            var textComponent = noMicTextObject.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
                textComponent.text = noMicMessage;
        }

        foreach (var panel in micControlPanels)
        {
            if (panel != null)
                panel.SetActive(false);
        }
    }

    private void OnContinueToMainMenu()
    {
        Debug.Log("MicrophoneStartupManager: Continue button clicked");
        PlayerPrefs.SetInt(PREF_CALIBRATION_SHOWN, 1);
        PlayerPrefs.Save();
        GoToMainMenu();
    }

    private void GoToMainMenu()
    {
        if (microSettingCanvas != null)
            microSettingCanvas.SetActive(false);

        if (mainMenuCanvas != null)
            mainMenuCanvas.SetActive(true);
        
        Debug.Log("MicrophoneStartupManager: Switched to main menu");
    }

    public void ForceShowCalibration()
    {
        hasShownCalibration = false;
        ShowMicrophoneSetup();
    }

    public void ResetCalibration()
    {
        PlayerPrefs.DeleteKey(PREF_CALIBRATION_SHOWN);
        PlayerPrefs.Save();
        hasShownCalibration = false;
    }
}
