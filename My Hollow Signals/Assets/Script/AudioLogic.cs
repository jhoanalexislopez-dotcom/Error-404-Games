using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioScript : MonoBehaviour
{

    public AudioMixer masterMixer;

    public void SetSoundMaster(float soundLevel)
    {
        masterMixer.SetFloat("masterVolume", soundLevel);
    }
    public void SetSoundSFX(float soundLevel)
    {
        masterMixer.SetFloat("sfxVolume", soundLevel);
    }
    public void SetSoundBgm(float soundLevel)
    {
        masterMixer.SetFloat("bgmVolume", soundLevel);
    }
}