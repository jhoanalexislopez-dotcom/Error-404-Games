using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneTransitionManager : MonoBehaviour
{
    public static void CutToBlackAndLoadScene(string sceneName, float delay = 2f)
    {
        GameObject transitionObject = new GameObject("SceneTransition");
        SceneTransitionManager manager = transitionObject.AddComponent<SceneTransitionManager>();
        manager.StartCoroutine(manager.TransitionCoroutine(sceneName, delay));
    }

    private IEnumerator TransitionCoroutine(string sceneName, float delay)
    {
        Canvas canvas = CreateBlackScreen();

        yield return new WaitForSeconds(delay);

        #if UNITY_EDITOR
        Selection.activeObject = null;
        #endif

        yield return null;

        SceneManager.LoadScene(sceneName);
    }

    private Canvas CreateBlackScreen()
    {
        GameObject canvasObject = new GameObject("BlackScreenCanvas");
        canvasObject.transform.SetParent(transform);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject imageObject = new GameObject("BlackImage");
        imageObject.transform.SetParent(canvasObject.transform);

        Image blackImage = imageObject.AddComponent<Image>();
        blackImage.color = new Color(0, 0, 0, 1);

        RectTransform rectTransform = blackImage.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        return canvas;
    }
}
