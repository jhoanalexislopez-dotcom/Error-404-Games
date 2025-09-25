using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider mMasterSlider;
    [SerializeField] private Slider mSFXSlider;
    [SerializeField] private Slider mBGMSlider;

    [Header("Menu Objects")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject audioPanel;
    [SerializeField] private GameObject controlsPanel;

    [Header("Buttons")]
    [SerializeField] private Button playButton;

    [Header("Audio")]
    [SerializeField] private AudioMixer mixer;

    [Header("Scene")]
    [SerializeField] private int sceneToLoad = 1;

    private void Start()
    {
        // Initialize sliders from mixer
        float value;
        if (mixer.GetFloat("masterVolume", out value)) mMasterSlider.value = value;
        if (mixer.GetFloat("bgmVolume", out value)) mBGMSlider.value = value;
        if (mixer.GetFloat("sfxVolume", out value)) mSFXSlider.value = value;

        // Hook up listeners
        mMasterSlider.onValueChanged.AddListener(SetMasterVolume);
        mBGMSlider.onValueChanged.AddListener(SetBGMVolume);
        mSFXSlider.onValueChanged.AddListener(SetSFXVolume);

        ShowMainMenu();
    }

    // ---------------- AUDIO ----------------
    private void SetMasterVolume(float value) => mixer.SetFloat("masterVolume", value);
    private void SetBGMVolume(float value) => mixer.SetFloat("bgmVolume", value);
    private void SetSFXVolume(float value) => mixer.SetFloat("sfxVolume", value);

    // ---------------- MENUS ----------------
    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        audioPanel.SetActive(false);
        controlsPanel.SetActive(false);
    }

    public void ShowAudioMenu()
    {
        mainMenuPanel.SetActive(false);
        audioPanel.SetActive(true);
        controlsPanel.SetActive(false);
    }

    public void ShowControlsMenu()
    {
        mainMenuPanel.SetActive(false);
        audioPanel.SetActive(false);
        controlsPanel.SetActive(true);
    }

    // ---------------- SCENES ----------------
    public void PlayGame() => SceneManager.LoadScene(sceneToLoad);

    public void ExitGame()
    {
        Application.Quit();
    }
}
