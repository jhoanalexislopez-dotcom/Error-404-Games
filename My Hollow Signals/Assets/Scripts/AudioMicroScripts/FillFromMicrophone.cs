/*******************************************************
 * Author: [Bianca Marinica]
 * Last Modified: [4/12/2025]
 * Description:
 *    This script fills a UI Image (audioBar) based on the loudness detected from the microphone input. It uses a sensitivity slider to adjust how responsive the audio bar is to changes in loudness. The script also includes a threshold to prevent the audio bar from filling up due to very low-level noise, ensuring that only significant sounds will affect the visual representation.
 *******************************************************/

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FillFromMicrophone : MonoBehaviour
{
    public Image audioBar;
    public Slider sensitivitySlider;
    public AudioLoudnessDetector detector;

    public float minimumSensibility = 100;
    public float maximumSensibility = 1000;
    public float currentLoudnessSensibility = 500;
    public float threshold = 0.1f;

    private void Start()
    {
        if (sensitivitySlider == null)
        {
            return;
        }

        sensitivitySlider.value = 0.5f;
        SetLoudnessSensibility(sensitivitySlider.value);
    }

    private void Update()
    {
        float loudness = detector.GetLoudnessFromMicrophone() * currentLoudnessSensibility;
        
        if (loudness < threshold)
        {
            loudness = 0.01f;
        }

        audioBar.fillAmount = loudness;
    }

    public void SetLoudnessSensibility(float t)
    {
        currentLoudnessSensibility = Mathf.Lerp(minimumSensibility, maximumSensibility, t);
    }
}
