/*******************************************************
 * Author: [Bianca Marinica]
 * Last Modified: [16/12/2025]
 * Description:
 *    This script detects the loudness of audio input from the microphone. It captures audio data in real-time and calculates the average loudness over a specified sample window. The script also allows for dynamic selection of different microphone devices, making it versatile for various applications that require audio input analysis.
 *******************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioLoudnessDetector : MonoBehaviour
{
    public int sampleWindow = 64;

    private AudioClip _microphoneClip;
    private string _microphoneName;

    public AudioClip MicrophoneClip => _microphoneClip;
    public string MicrophoneName => _microphoneName;
    public bool IsRecording => !string.IsNullOrEmpty(_microphoneName) && Microphone.IsRecording(_microphoneName);

    private void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            MicrophoneToAudioClip(0);
        }
        else
        {
            Debug.LogWarning("No microphones detected on Start()");
        }
    }


    private void OnEnable()
    {
        MicrophoneSelector.OnMicrophoneChoiceChanged += ChangeMicrophoneSource;
    }

    private void ChangeMicrophoneSource(int deviceIndex)
    {
        MicrophoneToAudioClip(deviceIndex);
    }

    private void OnDisable()
    {
        MicrophoneSelector.OnMicrophoneChoiceChanged -= ChangeMicrophoneSource;
    }
    private void MicrophoneToAudioClip(int microphoneIndex)
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("No microphone devices found");
            return;
        }

        if (microphoneIndex < 0 || microphoneIndex >= Microphone.devices.Length)
        {
            Debug.LogError("Microphone index out of range");
            return;
        }

        _microphoneName = Microphone.devices[microphoneIndex];
        _microphoneClip = Microphone.Start(
            _microphoneName,
            true,
            20,
            AudioSettings.outputSampleRate
        );
    }


    public float GetLoudnessFromMicrophone()
    {
        return GetLoudnessFromAudioClip(Microphone.GetPosition(_microphoneName), _microphoneClip);
    }

    public float GetLoudnessFromAudioClip(int clipPosition, AudioClip clip)
    {
        int startPosition = clipPosition - sampleWindow;

        if (startPosition < 0)
        {
            return 0;
        }

        float[] waveData = new float[sampleWindow];
        clip.GetData(waveData, startPosition);

        float totalLoudness = 0;

        foreach (var sample in waveData)
        {
            totalLoudness += Mathf.Abs(sample);
        }

        return totalLoudness / sampleWindow;
    }    
}
