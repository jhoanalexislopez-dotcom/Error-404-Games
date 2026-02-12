/*******************************************************
 * Author: [Bianca Marinica]
 * Last Modified: [09/02/2026]
 * Description:
 *    Main menu controller handling settings, scene loading, and menu navigation.
 *******************************************************/

using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider mMasterSlider;
    [SerializeField] private Slider mSFXSlider;
    [SerializeField] private Slider mBGMSlider;
    public GameObject BlackLayoutTransitionUI;

    [Header("Menu Objects")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionsMenuLayout;
    [SerializeField] private GameObject audioPanel;
    [SerializeField] private GameObject controlsPanel;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button audioBackButton;
    [SerializeField] private Button controlsBackButton;

    [Header("Audio")]
    [SerializeField] private AudioMixer mixer;

    [Header("Scene")]
    [SerializeField] private int sceneToLoad = 1;

    [Header("Transition Settings")]
    [Tooltip("Animator for scene transition")]
    public Animator transitionAnimator;

    [Tooltip("Time to wait after triggering transition before loading scene")]
    public float transitionDelayBeforeSceneLoad = 2f;

    [Tooltip("Name of the animator trigger for the transition")]
    public string transitionTriggerName = "StartTransition";

    private InputModeManager inputModeManager;
    private bool isTransitioning = false;

    private void Start()
    {
        // Ensure cursor is unlocked and visible when main menu loads
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Get or add InputModeManager
        inputModeManager = FindObjectOfType<InputModeManager>();
        if (inputModeManager == null)
        {
            inputModeManager = gameObject.AddComponent<InputModeManager>();
        }

        // Auto-assign transition animator if not set
        if (transitionAnimator == null)
        {
            transitionAnimator = GameObject.Find("LayoutCanvas/Image")?.GetComponent<Animator>();
            if (transitionAnimator == null)
            {
                Debug.LogWarning("Transition Animator not found! Please assign the transitionAnimator field in the MenuManager component.");
            }
        }

        // Initialize sliders from mixer (only if old sliders are assigned)
        if (mMasterSlider != null && mSFXSlider != null && mBGMSlider != null && mixer != null)
        {
            // Keep slider ranges at default 0-1 for natural feel
            mMasterSlider.minValue = 0.0001f;  // Avoid log10(0)
            mMasterSlider.maxValue = 1f;
            mSFXSlider.minValue = 0.0001f;
            mSFXSlider.maxValue = 1f;
            mBGMSlider.minValue = 0.0001f;
            mBGMSlider.maxValue = 1f;
            
            float value;
            if (mixer.GetFloat("masterVolume", out value)) mMasterSlider.value = DecibelToLinear(value);
            if (mixer.GetFloat("bgmVolume", out value)) mBGMSlider.value = DecibelToLinear(value);
            if (mixer.GetFloat("sfxVolume", out value)) mSFXSlider.value = DecibelToLinear(value);

            // Hook up listeners
            mMasterSlider.onValueChanged.AddListener(SetMasterVolume);
            mBGMSlider.onValueChanged.AddListener(SetBGMVolume);
            mSFXSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        ShowMainMenu();

        if (BlackLayoutTransitionUI != null)
            BlackLayoutTransitionUI.SetActive(true);
    }

    // ---------------- AUDIO ----------------
    private void SetMasterVolume(float value) => mixer.SetFloat("masterVolume", LinearToDecibel(value));
    private void SetBGMVolume(float value) => mixer.SetFloat("bgmVolume", LinearToDecibel(value));
    private void SetSFXVolume(float value) => mixer.SetFloat("sfxVolume", LinearToDecibel(value));
    
    private float LinearToDecibel(float linear)
    {
        // Convert linear slider value (0.0001-1) to decibels (-80 to 0)
        // Using logarithmic formula for natural audio perception
        float dB = 20f * Mathf.Log10(Mathf.Max(linear, 0.0001f));
        return Mathf.Clamp(dB, -80f, 0f);
    }
    
    private float DecibelToLinear(float dB)
    {
        // Convert decibels (-80 to 0) back to linear (0.0001-1)
        return Mathf.Pow(10f, dB / 20f);
    }

    // ---------------- MENUS ----------------
    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        if (optionsMenuLayout != null) optionsMenuLayout.SetActive(false);
        if (audioPanel != null) audioPanel.SetActive(false);
        controlsPanel.SetActive(false);

        // Only auto-select if in gamepad mode
        if (inputModeManager != null && inputModeManager.IsGamepadMode && playButton != null)
        {
            EventSystem.current.SetSelectedGameObject(playButton.gameObject);
        }
    }

    public void ShowOptionsMenu()
    {
        mainMenuPanel.SetActive(false);
        if (optionsMenuLayout != null) optionsMenuLayout.SetActive(true);
        if (audioPanel != null) audioPanel.SetActive(false);
        controlsPanel.SetActive(false);
    }

    public void ShowAudioMenu()
    {
        if (audioPanel == null) return;
        
        mainMenuPanel.SetActive(false);
        audioPanel.SetActive(true);
        controlsPanel.SetActive(false);

        // Only auto-select if in gamepad mode
        if (inputModeManager != null && inputModeManager.IsGamepadMode && mMasterSlider != null)
        {
            EventSystem.current.SetSelectedGameObject(mMasterSlider.gameObject);
        }
    }

    public void ShowControlsMenu()
    {
        mainMenuPanel.SetActive(false);
        audioPanel.SetActive(false);
        controlsPanel.SetActive(true);

        // Only auto-select if in gamepad mode
        if (inputModeManager != null && inputModeManager.IsGamepadMode && controlsBackButton != null)
        {
            EventSystem.current.SetSelectedGameObject(controlsBackButton.gameObject);
        }
    }

    // ---------------- SCENES ----------------
    public void PlayGame()
    {
        if (isTransitioning)
            return;

        isTransitioning = true;

        StartCoroutine(PlayGameWithSFX());
    }

    private IEnumerator PlayGameWithSFX()
    {
        // Wait a few frames to allow the button click SFX to trigger
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        // Now disable all inputs after the SFX has had time to play
        DisableAllInputs();

        // Start the transition
        StartCoroutine(TriggerTransitionAndLoadScene());
    }

    private void DisableAllInputs()
    {
        // Disable the InputModeManager's input actions
        if (inputModeManager != null)
        {
            inputModeManager.enabled = false;
        }

        // Disable all UI interactions by disabling the EventSystem
        if (EventSystem.current != null)
        {
            EventSystem.current.enabled = false;
        }

        // Make all buttons non-interactable
        Button[] allButtons = FindObjectsOfType<Button>();
        foreach (Button button in allButtons)
        {
            button.interactable = false;
        }

        // Make all sliders non-interactable
        Slider[] allSliders = FindObjectsOfType<Slider>();
        foreach (Slider slider in allSliders)
        {
            slider.interactable = false;
        }

        Debug.Log("All inputs have been disabled during scene transition.");
    }

    private IEnumerator TriggerTransitionAndLoadScene()
    {
        if (transitionAnimator != null)
        {
            Debug.Log($"Triggering transition animation with '{transitionTriggerName}' trigger...");
            transitionAnimator.SetTrigger(transitionTriggerName);
        }
        else
        {
            Debug.LogWarning("transitionAnimator is null, loading scene without transition animation!");
        }

        Debug.Log($"Waiting {transitionDelayBeforeSceneLoad} seconds before loading scene...");
        yield return new WaitForSeconds(transitionDelayBeforeSceneLoad);

        Debug.Log($"Loading scene: {sceneToLoad}");
        SceneManager.LoadScene(sceneToLoad);
    }

    public void ExitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
}
