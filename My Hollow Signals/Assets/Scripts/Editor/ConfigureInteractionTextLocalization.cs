using UnityEngine;
using UnityEditor;
using UnityEngine.Localization.Components;
using TMPro;
using UnityEditor.SceneManagement;

public class ConfigureInteractionTextLocalization
{
    [MenuItem("Tools/Configure InteractionText Localization")]
    public static void Configure()
    {
        var activeScene = EditorSceneManager.GetActiveScene();
        var rootObjects = activeScene.GetRootGameObjects();
        
        GameObject interactionText = null;
        
        foreach (var root in rootObjects)
        {
            var found = FindGameObjectByPath(root.transform, "Canvas/GameUI/InteractionUI/InteractionText");
            if (found != null)
            {
                interactionText = found.gameObject;
                break;
            }
        }
        
        if (interactionText == null)
        {
            Debug.LogError("Could not find InteractionText at path: Canvas/GameUI/InteractionUI/InteractionText");
            return;
        }
        
        var localizeEvent = interactionText.GetComponent<LocalizeStringEvent>();
        if (localizeEvent == null)
        {
            Debug.LogError("LocalizeStringEvent component not found on InteractionText");
            return;
        }
        
        var tmpText = interactionText.GetComponent<TextMeshProUGUI>();
        if (tmpText == null)
        {
            Debug.LogError("TextMeshProUGUI component not found on InteractionText");
            return;
        }
        
        if (localizeEvent.StringReference != null)
        {
            localizeEvent.StringReference.WaitForCompletion = true;
        }
        
        localizeEvent.OnUpdateString.RemoveAllListeners();
        localizeEvent.OnUpdateString.AddListener((string value) => tmpText.text = value);
        
        EditorUtility.SetDirty(interactionText);
        EditorSceneManager.MarkSceneDirty(activeScene);
        
        Debug.Log("Successfully configured InteractionText LocalizeStringEvent with WaitForCompletion=true and OnUpdateString event wired to TextMeshProUGUI.SetText");
    }
    
    private static Transform FindGameObjectByPath(Transform root, string path)
    {
        var parts = path.Split('/');
        Transform current = root;
        
        foreach (var part in parts)
        {
            bool found = false;
            for (int i = 0; i < current.childCount; i++)
            {
                var child = current.GetChild(i);
                if (child.name == part)
                {
                    current = child;
                    found = true;
                    break;
                }
            }
            
            if (!found)
                return null;
        }
        
        return current;
    }
}
