/*******************************************************
 * Author: [Ignacio López]
 * Last Modified: [22/01/2026]
 * Description:
 * This script generates a texture representing the waveform of the audio input from the microphone. It captures audio data in real-time and updates a Texture2D with the waveform information, which can then be used in materials for visual effects. The script also includes options for scaling the waveform and debugging logs to monitor its performance and behavior during development.
 *******************************************************/

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WaveformTextureGenerator : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioLoudnessDetector audioDetector;

    [Header("Waveform Texture")]
    [Tooltip("Horizontal resolution of waveform texture (how many samples across)")]
    public int textureWidth = 256;
    public int textureHeight = 1;

    [Tooltip("Scaling applied to audio samples before writing to texture (makes wave bigger).")]
    public float sampleScale = 20f;

    [Header("Material")]
    public Material targetMaterial;
    public string texturePropertyName = "_WaveformTex";

    [Header("Debug")]
    public bool enableDebugLogs = false;

    private Texture2D waveformTex;
    private float maxSampleValue = 0f;
    private int updateCount = 0;
    private PauseMenuManager pauseMenuManager;

    void Start()
    {
        if (audioDetector == null)
        {
            Debug.LogError("WaveformTextureGenerator: No AudioLoudnessDetector assigned!");
            enabled = false;
            return;
        }

        waveformTex = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBAFloat, false);
        waveformTex.wrapMode = TextureWrapMode.Clamp;
        waveformTex.filterMode = FilterMode.Bilinear;

        FillTexWithValue(0.5f);
        
        if (targetMaterial != null)
        {
            targetMaterial.SetTexture(texturePropertyName, waveformTex);
            Debug.Log($"WaveformTextureGenerator: Assigned texture to material '{targetMaterial.name}'");
        }
        else
        {
            Debug.LogWarning("WaveformTextureGenerator: No target material assigned!");
        }

        pauseMenuManager = FindObjectOfType<PauseMenuManager>();
        
        if (enableDebugLogs)
        {
            Debug.Log($"WaveformTextureGenerator: Found PauseMenuManager: {pauseMenuManager != null}");
        }
    }

    void Update()
    {
        if (audioDetector == null || !audioDetector.IsRecording || targetMaterial == null)
        {
            if (enableDebugLogs && updateCount == 0)
            {
                Debug.LogWarning($"WaveformTextureGenerator: Not updating - audioDetector={audioDetector != null}, isRecording={audioDetector?.IsRecording}, hasMaterial={targetMaterial != null}");
            }
            return;
        }

        if (IsPaused())
        {
            return;
        }

        AudioClip micClip = audioDetector.MicrophoneClip;
        string micName = audioDetector.MicrophoneName;

        if (micClip == null || micClip.samples == 0)
        {
            if (enableDebugLogs && updateCount % 60 == 0)
            {
                Debug.LogWarning("WaveformTextureGenerator: MicrophoneClip not ready");
            }
            return;
        }

        int micPos = Microphone.GetPosition(micName);

        if (micPos <= 0) return;
        if (micPos > micClip.samples) return;
        if (textureWidth > micClip.samples) return;

        int samplesNeeded = textureWidth;
        float[] buffer = new float[samplesNeeded];

        int startPos = micPos - samplesNeeded;

        if (startPos < 0)
        {
            int tailCount = -startPos;
            int headCount = samplesNeeded - tailCount;

            if (tailCount > 0 && tailCount <= micClip.samples)
                micClip.GetData(buffer, micClip.samples - tailCount);

            if (headCount > 0)
            {
                float[] headTemp = new float[headCount];
                micClip.GetData(headTemp, 0);
                System.Array.Copy(headTemp, 0, buffer, tailCount, headCount);
            }
        }
        else
        {
            micClip.GetData(buffer, startPos);
        }

        maxSampleValue = 0f;
        for (int i = 0; i < textureWidth; i++)
        {
            float s = buffer[i] * sampleScale;
            s = Mathf.Clamp(s, -1f, 1f);
            maxSampleValue = Mathf.Max(maxSampleValue, Mathf.Abs(s));
            float mapped = 0.5f + (s * 0.5f);
            waveformTex.SetPixel(i, 0, new Color(mapped, mapped, mapped, 1));
        }

        waveformTex.Apply(false, false);
        targetMaterial.SetTexture(texturePropertyName, waveformTex);

        updateCount++;
        if (enableDebugLogs && updateCount % 60 == 0)
        {
            Debug.Log($"WaveformTextureGenerator: Updated {updateCount} times, maxSample={maxSampleValue:F4}, micPos={micPos}/{micClip.samples}");
        }
    }

    void FillTexWithValue(float v)
    {
        Color c = new Color(v, v, v, 1);
        for (int x = 0; x < waveformTex.width; x++)
            waveformTex.SetPixel(x, 0, c);
        waveformTex.Apply();
    }

    private bool IsPaused()
    {
        if (pauseMenuManager != null && pauseMenuManager.IsPaused)
        {
            return true;
        }

        return false;
    }
}
