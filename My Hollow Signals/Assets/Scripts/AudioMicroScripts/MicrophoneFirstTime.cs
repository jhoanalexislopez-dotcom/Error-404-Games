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
