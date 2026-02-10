/*******************************************************
 * Author: [Ignacio Lopez]
 * Last Modified: [09/02/2026]
 * Description:
 *    This script manages the in-game options menu, allowing players to adjust settings such as mouse and gamepad sensitivity. It handles the opening and closing of the options menu, updates player preferences, and ensures that changes are applied to the player's controls in real-time. The script also integrates with the pause menu for seamless navigation.
 *******************************************************/


using UnityEngine;
using UnityEngine.UI;

public class InGameOptionsController : MonoBehaviour
{
    [Header("UI Navigation")]
    public GameObject optionsMenuRoot;
    public GameObject pauseMenuRoot;
    public Button backButton;
    
    [Header("Pause Menu Reference")]
    public PauseMenuManager pauseMenuManager;
    
    private OptionsMenuController optionsController;
    private FirstPersonController playerController;
    
    private const string MOUSE_SENSITIVITY_KEY = "MouseSensitivity";
    private const string GAMEPAD_SENSITIVITY_KEY = "GamepadSensitivity";
    
    void Awake()
    {
        optionsController = GetComponent<OptionsMenuController>();
        playerController = FindObjectOfType<FirstPersonController>();
        
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(CloseOptions);
        }
        
        if (optionsMenuRoot != null)
        {
            optionsMenuRoot.SetActive(false);
        }
        
        SubscribeToSensitivityChanges();
    }
    
    void OnDestroy()
    {
        UnsubscribeFromSensitivityChanges();
    }
    
    private void SubscribeToSensitivityChanges()
    {
        if (optionsController != null)
        {
            if (optionsController.mouseSensitivitySlider != null)
                optionsController.mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
            
            if (optionsController.gamepadSensitivitySlider != null)
                optionsController.gamepadSensitivitySlider.onValueChanged.AddListener(OnGamepadSensitivityChanged);
        }
    }
    
    private void UnsubscribeFromSensitivityChanges()
    {
        if (optionsController != null)
        {
            if (optionsController.mouseSensitivitySlider != null)
                optionsController.mouseSensitivitySlider.onValueChanged.RemoveListener(OnMouseSensitivityChanged);
            
            if (optionsController.gamepadSensitivitySlider != null)
                optionsController.gamepadSensitivitySlider.onValueChanged.RemoveListener(OnGamepadSensitivityChanged);
        }
    }
    
    private void OnMouseSensitivityChanged(float value)
    {
        if (playerController != null)
        {
            playerController.mouseSensitivity = value;
        }
    }
    
    private void OnGamepadSensitivityChanged(float value)
    {
        if (playerController != null)
        {
            playerController.gamepadSensitivity = value;
        }
    }
    
    public void OpenOptions()
    {
        if (pauseMenuRoot != null)
        {
            pauseMenuRoot.SetActive(false);
        }
        
        if (optionsMenuRoot != null)
        {
            optionsMenuRoot.SetActive(true);
        }
        
        if (optionsController != null)
        {
            optionsController.SetInitialSelection();
        }
        
        ApplyCurrentSensitivityToPlayer();
    }
    
    private void ApplyCurrentSensitivityToPlayer()
    {
        if (playerController != null)
        {
            float mouseSens = PlayerPrefs.GetFloat(MOUSE_SENSITIVITY_KEY, 1f);
            float gamepadSens = PlayerPrefs.GetFloat(GAMEPAD_SENSITIVITY_KEY, 3f);
            
            playerController.mouseSensitivity = mouseSens;
            playerController.gamepadSensitivity = gamepadSens;
        }
    }
    
    public void CloseOptions()
    {
        if (optionsMenuRoot != null)
        {
            optionsMenuRoot.SetActive(false);
        }
        
        if (pauseMenuRoot != null)
        {
            pauseMenuRoot.SetActive(true);
        }
        
        if (pauseMenuManager != null && pauseMenuManager.optionsButton != null)
        {
            pauseMenuManager.optionsButton.Select();
        }
    }
}

