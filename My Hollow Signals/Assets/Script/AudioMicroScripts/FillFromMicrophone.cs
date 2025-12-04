using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewBehaviourScript : MonoBehaviour
{
    public Image audioBar;
    public AudioLoudnessDetector detector;

    public float CurrentLoudnessSensibility = 100f;
    public float threshold = 0.1f;
    private void Update()
    {
        float loudness = detector.GetLoudnessFromMicrophone() * CurrentLoudnessSensibility;
        
        if (loudness < threshold)
        {
            loudness = 0.01f;
        }

        audioBar.fillAmount = loudness;
    }

    public void SetLoudnessSensibility(float t)
    {

    }
}
