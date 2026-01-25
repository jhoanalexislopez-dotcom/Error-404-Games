using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
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

    public void PlayDialogue(LocalizedString[] localizedLines, System.Action onComplete = null)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(PlayDialogueSequence(localizedLines, onComplete));
    }

    private IEnumerator PlayDialogueSequence(LocalizedString[] localizedLines, System.Action onComplete)
    {
        foreach (LocalizedString localizedLine in localizedLines)
        {
            yield return StartCoroutine(LoadAndTypeText(localizedLine));
            yield return new WaitForSeconds(lineDelay);
        }

        yield return new WaitForSeconds(autoHideDelay);

        if (dialogueText != null)
            dialogueText.text = "";

        onComplete?.Invoke();

        Destroy(gameObject);
    }

    private IEnumerator LoadAndTypeText(LocalizedString localizedText)
    {
        if (dialogueText == null)
        {
            Debug.LogWarning("DialogueText is not assigned on DialogueUI");
            yield break;
        }

        var loadOperation = localizedText.GetLocalizedStringAsync();
        yield return loadOperation;

        string text = loadOperation.Result;
        
        yield return StartCoroutine(TypeText(text));
    }

    private IEnumerator TypeText(string text)
    {
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
