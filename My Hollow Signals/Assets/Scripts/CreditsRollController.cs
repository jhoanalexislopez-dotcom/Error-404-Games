using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class CreditsRollController : MonoBehaviour
{
    [Header("CSV Data")]
    [Tooltip("The CSV file containing departments (top row) and credited people")]
    public TextAsset creditsCSV;

    [Header("Logo Settings")]
    public Image logoImage;
    public float logoDisplayDuration = 3f;
    public float logoFadeDuration = 1f;

    [Header("Scroll Settings")]
    public RectTransform scrollContent;
    public float scrollSpeed = 50f;
    public float startDelay = 1f;
    public float endPauseDuration = 2f;

    [Header("Department Text Style")]
    public TMP_FontAsset departmentFont;
    public float departmentFontSize = 48f;
    public Color departmentColor = Color.white;
    public FontStyles departmentFontStyle = FontStyles.Bold;
    public float departmentSpacing = 80f;

    [Header("Credited People Text Style")]
    public TMP_FontAsset creditedPeopleFont;
    public float creditedPeopleFontSize = 32f;
    public Color creditedPeopleColor = Color.gray;
    public FontStyles creditedPeopleFontStyle = FontStyles.Normal;
    public float creditedPeopleLineSpacing = 10f;

    [Header("Scene Transition")]
    public string nextSceneName = "MainMenu";
    public float fadeOutDuration = 1f;
    public Image fadeImage;

    private CanvasGroup logoCanvasGroup;
    private CanvasGroup creditsCanvasGroup;
    private bool isScrolling = false;
    private float scrollStartPosition;
    private float scrollEndPosition;

    private void Start()
    {
        SetupCanvasGroups();
        StartCoroutine(RunCreditsSequence());
    }

    private void SetupCanvasGroups()
    {
        if (logoImage != null && logoImage.GetComponent<CanvasGroup>() == null)
        {
            logoCanvasGroup = logoImage.gameObject.AddComponent<CanvasGroup>();
        }
        else if (logoImage != null)
        {
            logoCanvasGroup = logoImage.GetComponent<CanvasGroup>();
        }

        if (scrollContent != null && scrollContent.GetComponent<CanvasGroup>() == null)
        {
            creditsCanvasGroup = scrollContent.gameObject.AddComponent<CanvasGroup>();
        }
        else if (scrollContent != null)
        {
            creditsCanvasGroup = scrollContent.GetComponent<CanvasGroup>();
        }

        if (logoCanvasGroup != null)
        {
            logoCanvasGroup.alpha = 0f;
        }

        if (creditsCanvasGroup != null)
        {
            creditsCanvasGroup.alpha = 0f;
        }

        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
        }
    }

    private IEnumerator RunCreditsSequence()
    {
        yield return StartCoroutine(ShowLogo());
        yield return StartCoroutine(HideLogo());
        
        ParseAndBuildCredits();
        
        yield return StartCoroutine(ShowCredits());
        yield return new WaitForSeconds(startDelay);
        
        yield return StartCoroutine(ScrollCredits());
        
        yield return new WaitForSeconds(endPauseDuration);
        
        yield return StartCoroutine(TransitionToNextScene());
    }

    private IEnumerator ShowLogo()
    {
        if (logoCanvasGroup == null)
        {
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < logoFadeDuration)
        {
            elapsed += Time.deltaTime;
            logoCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / logoFadeDuration);
            yield return null;
        }

        logoCanvasGroup.alpha = 1f;
        yield return new WaitForSeconds(logoDisplayDuration);
    }

    private IEnumerator HideLogo()
    {
        if (logoCanvasGroup == null)
        {
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < logoFadeDuration)
        {
            elapsed += Time.deltaTime;
            logoCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / logoFadeDuration);
            yield return null;
        }

        logoCanvasGroup.alpha = 0f;
        
        if (logoImage != null)
        {
            logoImage.gameObject.SetActive(false);
        }
    }

    private IEnumerator ShowCredits()
    {
        if (creditsCanvasGroup == null)
        {
            yield break;
        }

        float elapsed = 0f;
        float fadeDuration = 0.5f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            creditsCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        creditsCanvasGroup.alpha = 1f;
    }

    private void ParseAndBuildCredits()
    {
        if (creditsCSV == null || scrollContent == null)
        {
            Debug.LogError("Credits CSV or Scroll Content is not assigned!");
            return;
        }

        ClearScrollContent();

        string csvText = creditsCSV.text;
        string[] lines = csvText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        if (lines.Length == 0)
        {
            Debug.LogError("CSV file is empty!");
            return;
        }

        string[] departments = lines[0].Split(',');
        List<List<string>> departmentCredits = new List<List<string>>();

        for (int i = 0; i < departments.Length; i++)
        {
            departmentCredits.Add(new List<string>());
        }

        for (int row = 1; row < lines.Length; row++)
        {
            string[] cells = lines[row].Split(',');
            
            for (int col = 0; col < Mathf.Min(cells.Length, departments.Length); col++)
            {
                string cellValue = cells[col].Trim();
                if (!string.IsNullOrEmpty(cellValue))
                {
                    departmentCredits[col].Add(cellValue);
                }
            }
        }

        float currentYPosition = -100f;

        for (int i = 0; i < departments.Length; i++)
        {
            string department = departments[i].Trim();
            
            if (string.IsNullOrEmpty(department))
            {
                continue;
            }

            CreateDepartmentText(department, ref currentYPosition);

            foreach (string person in departmentCredits[i])
            {
                CreateCreditedPersonText(person, ref currentYPosition);
                currentYPosition -= creditedPeopleLineSpacing;
            }

            currentYPosition -= departmentSpacing;
        }

        scrollContent.sizeDelta = new Vector2(scrollContent.sizeDelta.x, Mathf.Abs(currentYPosition));
        
        Canvas.ForceUpdateCanvases();
        
        scrollStartPosition = -Screen.height - 600f;
        scrollContent.anchoredPosition = new Vector2(0f, scrollStartPosition);
        scrollEndPosition = scrollContent.sizeDelta.y;
    }

    private void ClearScrollContent()
    {
        foreach (Transform child in scrollContent)
        {
            Destroy(child.gameObject);
        }
    }

    private void CreateDepartmentText(string departmentName, ref float yPosition)
    {
        GameObject textObj = new GameObject(departmentName + "_Department");
        textObj.transform.SetParent(scrollContent, false);

        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = departmentName;
        textComponent.font = departmentFont;
        textComponent.fontSize = departmentFontSize;
        textComponent.color = departmentColor;
        textComponent.fontStyle = departmentFontStyle;
        textComponent.alignment = TextAlignmentOptions.Center;

        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = new Vector2(0f, yPosition);
        rectTransform.sizeDelta = new Vector2(-100f, departmentFontSize + 20f);

        yPosition -= (departmentFontSize + 40f);
    }

    private void CreateCreditedPersonText(string personName, ref float yPosition)
    {
        GameObject textObj = new GameObject(personName + "_Credit");
        textObj.transform.SetParent(scrollContent, false);

        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = personName;
        textComponent.font = creditedPeopleFont;
        textComponent.fontSize = creditedPeopleFontSize;
        textComponent.color = creditedPeopleColor;
        textComponent.fontStyle = creditedPeopleFontStyle;
        textComponent.alignment = TextAlignmentOptions.Center;

        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = new Vector2(0f, yPosition);
        rectTransform.sizeDelta = new Vector2(-100f, creditedPeopleFontSize + 10f);

        yPosition -= (creditedPeopleFontSize + 10f);
    }

    private IEnumerator ScrollCredits()
    {
        isScrolling = true;

        while (scrollContent.anchoredPosition.y < scrollEndPosition)
        {
            float newY = scrollContent.anchoredPosition.y + (scrollSpeed * Time.deltaTime);
            scrollContent.anchoredPosition = new Vector2(scrollContent.anchoredPosition.x, newY);
            yield return null;
        }

        isScrolling = false;
    }

    private IEnumerator TransitionToNextScene()
    {
        if (fadeImage != null)
        {
            float elapsed = 0f;
            
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeOutDuration);
                fadeImage.color = new Color(0, 0, 0, alpha);
                yield return null;
            }

            fadeImage.color = new Color(0, 0, 0, 1);
        }

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && 
            (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
        {
            StopAllCoroutines();
            StartCoroutine(TransitionToNextScene());
        }
    }
}
