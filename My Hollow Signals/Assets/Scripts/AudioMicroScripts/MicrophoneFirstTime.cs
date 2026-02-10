/*******************************************************
 * Author: [Bianca Marinica]
 * Last Modified: [2/2/2026]
 * Description:
 *    This script checks if a microphone is available on the device. If a microphone is detected, it enables the microphone selection UI and audio meter. If no microphone is found, it displays a message to the user and hides the microphone-related UI elements. This ensures that users are informed about the availability of audio input features and can adjust their settings accordingly.
 *******************************************************/

using UnityEngine;
using UnityEngine.UI;

public class MicrophoneFirstTime : MonoBehaviour
{
    public GameObject microphoneSelector;
    public GameObject audioMeter;
    public GameObject microphoneCalibration;
    public GameObject noMicroText;

    void Start()
    {
        CheckMicrophone();
    }

    void CheckMicrophone()
    {
        if (Microphone.devices.Length > 0)
        {
            // Micro detectado → mostrar opciones de micro
            microphoneSelector.SetActive(true);
            audioMeter.SetActive(true);
            microphoneCalibration.SetActive(true);
            noMicroText.SetActive(false);
        }
        else
        {
            // No hay micro → mostrar mensaje y ocultar opciones
            microphoneSelector.SetActive(false);
            audioMeter.SetActive(false);
            microphoneCalibration.SetActive(false);
            noMicroText.SetActive(true);
        }
    }
}
