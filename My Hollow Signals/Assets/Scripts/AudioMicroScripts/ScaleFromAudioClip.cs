/*******************************************************
 * Author: [Ignacio López]
 * Last Modified: [4/12/2025]
 * Description:
 *   This script scales a GameObject based on the loudness detected from an AudioClip. It uses the AudioLoudnessDetector to analyze the audio data and adjusts the scale of the GameObject between specified minimum and maximum values. The script also includes a sensitivity setting to control how responsive the scaling is to changes in loudness, as well as a threshold to prevent scaling from very low-level noise.    
  *******************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleFromAudioClip : MonoBehaviour
{
    public AudioSource source;
    public Vector3 minScale, maxScale;
    public AudioLoudnessDetector detector;

    public float loudnessSensibility = 100f;
    public float threshold = 0.1f;
    private void Update()
    {
        float loudness = detector.GetLoudnessFromAudioClip(source.timeSamples, source.clip) * loudnessSensibility;
        Debug.Log(loudness);
        if (loudness < threshold)
        {
            loudness = 0;
        }

        transform.localScale = Vector3.Lerp(minScale, maxScale, loudness);
    }
}
