using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

using UnityEngine.UI;


public class MenuManager : MonoBehaviour
{
    public Slider mMasterSlider;
    public Slider mSFXSlider;
    public Slider mBGMSlider;
    public int scene;

    public GameObject AudioSettings;
    public GameObject MainMenuObject;
    public GameObject ControlsMenu;

    public Button AudiobacktoMenuButton;
    public Button PlayMainMenuButton;

    public AudioMixer mixer;
    float volume, exposedParam;

    void Start()
    {
        mixer.GetFloat("masterVolume", out exposedParam);
        mMasterSlider.value = exposedParam;

        mixer.GetFloat("bgmVolume", out exposedParam);
        mBGMSlider.value = exposedParam;

        mixer.GetFloat("sfxVolume", out exposedParam);
        mSFXSlider.value = exposedParam;
    }


    // Update is called once per frame
    void Update()
    {

    }

    public void EnterAudioSettings()
    {
        ControlsMenu.SetActive(true);
        MainMenuObject.SetActive(false);

        AudiobacktoMenuButton.Select();
    }

    public void EnterControls()
    {
        AudioSettings.SetActive(true);
        MainMenuObject.SetActive(false);

        AudiobacktoMenuButton.Select();
    }

    public void ReturnToMainMenu()
    {
        MainMenuObject.SetActive(true);
        AudioSettings.SetActive(false);
        //ControlsMenu.SetActive(false);

        PlayMainMenuButton.Select();
    }

    public void loadScene()
    {
        SceneManager.LoadScene(scene);
    }

    public void Exit()
    {
        //EditorApplication.ExitPlaymode();
        Application.Quit();
    }

}