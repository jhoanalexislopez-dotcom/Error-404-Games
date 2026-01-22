using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI dialogueText;

    [Header("Typing Settings")]
    [Range(0.01f, 0.2f)]
    public float typingSpeed = 0.05f;
    public float lineDelay = 1.2f;
    public float autoHideDelay = 2f;

    private Coroutine typingCoroutine;

    public void PlayDialogue(string[] lines, System.Action onComplete = null)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(PlayDialogueSequence(lines, onComplete));
    }

    private IEnumerator PlayDialogueSequence(string[] lines, System.Action onComplete)
    {
        foreach (string line in lines)
        {
            yield return StartCoroutine(TypeText(line));
            yield return new WaitForSeconds(lineDelay);
        }

        yield return new WaitForSeconds(autoHideDelay);

        if (dialogueText != null)
            dialogueText.text = "";

        onComplete?.Invoke();

        Destroy(gameObject);
    }

    private IEnumerator TypeText(string text)
    {
        if (dialogueText == null)
        {
            Debug.LogWarning("DialogueText is not assigned on DialogueUI");
            yield break;
        }

        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private void OnDestroy()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
    }
}
