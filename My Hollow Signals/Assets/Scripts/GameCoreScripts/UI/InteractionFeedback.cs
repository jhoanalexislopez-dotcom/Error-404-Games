/*******************************************************
 * Author: [Bianca Marinica]
 * Last Modified: [21/11/2025]
 * Description:
 *    Displays feedback messages for interactions (e.g., locked doors).
 *******************************************************/

using UnityEngine;
using TMPro;
using System.Collections;

public class InteractionFeedback : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("TextMeshProUGUI component to display messages")]
    [SerializeField] private TextMeshProUGUI messageText;
    
    [Header("Display Settings")]
    [Tooltip("How long to display the message in seconds")]
    [SerializeField] private float displayDuration = 2f;
    
    [Tooltip("Fade in/out speed")]
    [SerializeField] private float fadeSpeed = 2f;
    
    private Coroutine currentMessageCoroutine;
    
    private void Start()
    {
        if (messageText != null)
        {
            SetAlpha(0f);
        }
    }
    
    public void ShowMessage(string message)
    {
        if (messageText == null)
        {
            Debug.LogWarning("Message text not assigned!");
            return;
        }
        
        if (currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine);
        }
        
        currentMessageCoroutine = StartCoroutine(DisplayMessageCoroutine(message));
    }
    
    private IEnumerator DisplayMessageCoroutine(string message)
    {
        messageText.text = message;
        
        yield return FadeIn();
        
        yield return new WaitForSecondsRealtime(displayDuration);
        
        yield return FadeOut();
        
        messageText.text = "";
        currentMessageCoroutine = null;
    }
    
    private IEnumerator FadeIn()
    {
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.unscaledDeltaTime * fadeSpeed;
            SetAlpha(alpha);
            yield return null;
        }
        SetAlpha(1f);
    }
    
    private IEnumerator FadeOut()
    {
        float alpha = 1f;
        while (alpha > 0f)
        {
            alpha -= Time.unscaledDeltaTime * fadeSpeed;
            SetAlpha(alpha);
            yield return null;
        }
        SetAlpha(0f);
    }
    
    private void SetAlpha(float alpha)
    {
        if (messageText != null)
        {
            Color color = messageText.color;
            color.a = Mathf.Clamp01(alpha);
            messageText.color = color;
        }
    }
}
