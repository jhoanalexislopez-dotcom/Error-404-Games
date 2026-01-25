using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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

    [Tooltip("First button to select in main menu for gamepad navigation (optional)")]
    [SerializeField] private Button mainMenuFirstButton;

    [Header("Transition Settings")]
    [Tooltip("Duration of fade transitions in seconds")]
    [SerializeField] private float transitionDuration = 0.5f;

    [Header("Settings")]
    [SerializeField] private float startupDelay = 0.5f;
    [SerializeField] private bool showEveryTime = false;

    private const string PREF_CALIBRATION_SHOWN = "MicCalibrationShown";
    private bool hasShownCalibration = false;
    private bool isTransitioning = false;
    
    private CanvasGroup warningCanvasGroup;
    private CanvasGroup calibrationCanvasGroup;
    private CanvasGroup microSettingCanvasGroup;
    private CanvasGroup mainMenuCanvasGroup;

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
        warningCanvasGroup = EnsureCanvasGroup(warningScreen);
        calibrationCanvasGroup = EnsureCanvasGroup(microphoneCalibrationScreen);
        microSettingCanvasGroup = EnsureCanvasGroup(microSettingCanvas);
        mainMenuCanvasGroup = EnsureCanvasGroup(mainMenuCanvas);

        if (warningContinueButton != null)
            warningContinueButton.onClick.AddListener(OnWarningContinue);

        if (calibrationContinueButton != null)
            calibrationContinueButton.onClick.AddListener(OnCalibrationContinue);

        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(false);
            if (mainMenuCanvasGroup != null)
                mainMenuCanvasGroup.alpha = 0f;
        }
        
        if (microSettingCanvas != null)
        {
            microSettingCanvas.SetActive(true);
            if (microSettingCanvasGroup != null)
                microSettingCanvasGroup.alpha = 1f;
        }

        if (warningScreen != null)
        {
            warningScreen.SetActive(false);
            if (warningCanvasGroup != null)
                warningCanvasGroup.alpha = 0f;
        }

        if (microphoneCalibrationScreen != null)
        {
            microphoneCalibrationScreen.SetActive(false);
            if (calibrationCanvasGroup != null)
                calibrationCanvasGroup.alpha = 0f;
        }
        
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

    private CanvasGroup EnsureCanvasGroup(GameObject obj)
    {
        if (obj == null) return null;
        
        var canvasGroup = obj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = obj.AddComponent<CanvasGroup>();
        }
        return canvasGroup;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float targetAlpha, float duration)
    {
        if (canvasGroup == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }
        canvasGroup.alpha = targetAlpha;
    }

    private void ShowWarningScreen()
    {
        Debug.Log("MicrophoneStartupManager: Showing warning screen");
        StartCoroutine(FadeInWarningScreen());
    }

    private IEnumerator FadeInWarningScreen()
    {
        if (warningScreen != null)
        {
            warningScreen.SetActive(true);
            if (warningCanvasGroup != null)
            {
                yield return StartCoroutine(FadeCanvasGroup(warningCanvasGroup, 0f, 1f, transitionDuration));
            }
            SetSelectedButton(warningContinueButton);
        }
    }

    private void OnWarningContinue()
    {
        if (isTransitioning) return;
        
        Debug.Log("MicrophoneStartupManager: Warning continue button clicked");
        StartCoroutine(TransitionToCalibration());
    }

    private IEnumerator TransitionToCalibration()
    {
        isTransitioning = true;

        if (warningCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(warningCanvasGroup, 1f, 0f, transitionDuration));
        }

        if (warningScreen != null)
            warningScreen.SetActive(false);

        ShowMicrophoneCalibrationScreen();
        
        isTransitioning = false;
    }

    private void ShowMicrophoneCalibrationScreen()
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
            Debug.Log("MicrophoneStartupManager: No microphone detected, showing message");
            ShowNoMicrophoneMessage();
        }

        StartCoroutine(FadeInCalibrationScreen());
    }

    private IEnumerator FadeInCalibrationScreen()
    {
        if (microphoneCalibrationScreen != null)
        {
            microphoneCalibrationScreen.SetActive(true);
            if (calibrationCanvasGroup != null)
            {
                yield return StartCoroutine(FadeCanvasGroup(calibrationCanvasGroup, 0f, 1f, transitionDuration));
            }
            SetSelectedButton(calibrationContinueButton);
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
        if (isTransitioning) return;

        Debug.Log("MicrophoneStartupManager: Calibration continue button clicked");
        PlayerPrefs.SetInt(PREF_CALIBRATION_SHOWN, 1);
        PlayerPrefs.Save();
        StartCoroutine(TransitionToMainMenu());
    }

    private IEnumerator TransitionToMainMenu()
    {
        isTransitioning = true;

        if (calibrationCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(calibrationCanvasGroup, 1f, 0f, transitionDuration));
        }

        if (microSettingCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(microSettingCanvasGroup, 1f, 0f, transitionDuration));
        }

        if (microphoneCalibrationScreen != null)
            microphoneCalibrationScreen.SetActive(false);

        if (microSettingCanvas != null)
            microSettingCanvas.SetActive(false);

        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(true);
            if (mainMenuCanvasGroup != null)
            {
                yield return StartCoroutine(FadeCanvasGroup(mainMenuCanvasGroup, 0f, 1f, transitionDuration));
            }
            SetSelectedButton(mainMenuFirstButton);
        }

        Debug.Log("MicrophoneStartupManager: Switched to main menu");
        isTransitioning = false;
    }

    private void GoToMainMenu()
    {
        StartCoroutine(GoToMainMenuImmediate());
    }

    private IEnumerator GoToMainMenuImmediate()
    {
        if (microSettingCanvasGroup != null)
            microSettingCanvasGroup.alpha = 0f;

        if (microSettingCanvas != null)
            microSettingCanvas.SetActive(false);

        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(true);
            if (mainMenuCanvasGroup != null)
            {
                yield return StartCoroutine(FadeCanvasGroup(mainMenuCanvasGroup, 0f, 1f, transitionDuration));
            }
            SetSelectedButton(mainMenuFirstButton);
        }
        
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

    private void SetSelectedButton(Button button)
    {
        if (button == null) return;

        EventSystem eventSystem = EventSystem.current;
        if (eventSystem != null)
        {
            eventSystem.SetSelectedGameObject(button.gameObject);
            Debug.Log($"MicrophoneStartupManager: Selected button '{button.name}' for gamepad navigation");
        }
        else
        {
            Debug.LogWarning("MicrophoneStartupManager: No EventSystem found in scene for gamepad navigation");
        }
    }
}
