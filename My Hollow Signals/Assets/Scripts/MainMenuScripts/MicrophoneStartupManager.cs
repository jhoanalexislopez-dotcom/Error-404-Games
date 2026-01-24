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

    [Header("Screens")]
    [Tooltip("First screen - Warning message")]
    [SerializeField] private GameObject warningScreen;

    [Tooltip("Second screen - Microphone calibration")]
    [SerializeField] private GameObject microphoneCalibrationScreen;

    [Header("No Microphone Elements")]
    [Tooltip("GameObject to show when no microphone is detected (will be hidden if mic detected)")]
    [SerializeField] private GameObject noMicTextObject;
    
    [Tooltip("Panels to show when mic IS detected (source, calibration, sens panels)")]
    [SerializeField] private GameObject[] micControlPanels;
    
    [Tooltip("Background image or other elements that should ALWAYS be visible")]
    [SerializeField] private GameObject[] alwaysVisibleElements;

    [Header("Buttons")]
    [Tooltip("Button on warning screen to proceed to calibration")]
    [SerializeField] private Button warningContinueButton;

    [Tooltip("Button on calibration screen to close and go to main menu")]
    [SerializeField] private Button calibrationContinueButton;

    [Header("Settings")]
    [SerializeField] private float startupDelay = 0.5f;
    [SerializeField] private bool showEveryTime = false;

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
            StartCoroutine(ShowWarningAfterDelay());
        }
        else
        {
            Debug.Log("MicrophoneStartupManager: Calibration already completed, going to main menu.");
            GoToMainMenu();
        }
    }

    private IEnumerator ShowWarningAfterDelay()
    {
        Debug.Log($"MicrophoneStartupManager: Waiting {startupDelay} seconds before showing warning...");
        yield return new WaitForSeconds(startupDelay);
        ShowWarningScreen();
    }

    private void SetupUI()
    {
        if (warningContinueButton != null)
            warningContinueButton.onClick.AddListener(OnWarningContinue);

        if (calibrationContinueButton != null)
            calibrationContinueButton.onClick.AddListener(OnCalibrationContinue);

        if (mainMenuCanvas != null)
            mainMenuCanvas.SetActive(false);
        
        if (microSettingCanvas != null)
            microSettingCanvas.SetActive(true);

        if (warningScreen != null)
            warningScreen.SetActive(false);

        if (microphoneCalibrationScreen != null)
            microphoneCalibrationScreen.SetActive(false);
        
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

    private void ShowWarningScreen()
    {
        Debug.Log("MicrophoneStartupManager: Showing warning screen");
        
        if (warningScreen != null)
            warningScreen.SetActive(true);

        if (microphoneCalibrationScreen != null)
            microphoneCalibrationScreen.SetActive(false);
    }

    private void OnWarningContinue()
    {
        Debug.Log("MicrophoneStartupManager: Warning continue button clicked");
        
        if (warningScreen != null)
            warningScreen.SetActive(false);

        ShowMicrophoneCalibrationScreen();
    }

    private void ShowMicrophoneCalibrationScreen()
    {
        if (hasShownCalibration)
        {
            Debug.Log("MicrophoneStartupManager: Already shown calibration, skipping.");
            return;
        }

        hasShownCalibration = true;

        if (microphoneCalibrationScreen != null)
            microphoneCalibrationScreen.SetActive(true);

        bool hasMicrophone = Microphone.devices.Length > 0;
        Debug.Log($"MicrophoneStartupManager: Microphone detected = {hasMicrophone}, Device count = {Microphone.devices.Length}");

        if (hasMicrophone)
        {
            Debug.Log("MicrophoneStartupManager: Showing microphone controls");
            ShowMicrophoneControls();
        }
        else
        {
            Debug.Log("MicrophoneStartupManager: No microphone detected, showing message");
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
            Debug.Log("MicrophoneStartupManager: NoMicText shown (using default text from TextMeshPro component)");
        }

        foreach (var panel in micControlPanels)
        {
            if (panel != null)
                panel.SetActive(false);
        }
    }

    private void OnCalibrationContinue()
    {
        Debug.Log("MicrophoneStartupManager: Calibration continue button clicked");
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
        ShowWarningScreen();
    }

    public void ResetCalibration()
    {
        PlayerPrefs.DeleteKey(PREF_CALIBRATION_SHOWN);
        PlayerPrefs.Save();
        hasShownCalibration = false;
    }
}
