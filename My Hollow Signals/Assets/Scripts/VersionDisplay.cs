using UnityEngine;
using TMPro;

public class VersionDisplay : MonoBehaviour
{
    [Header("Version Format")]
    [Tooltip("Prefix to show before version number (e.g., 'v', 'Version ')")]
    public string prefix = "v";
    
    [Tooltip("Suffix to show after version number (e.g., '-alpha', '-beta')")]
    public string suffix = "";
    
    private void Start()
    {
        UpdateVersionText();
    }
    
    private void UpdateVersionText()
    {
        TextMeshProUGUI textComponent = GetComponent<TextMeshProUGUI>();
        
        if (textComponent != null)
        {
            string version = Application.version;
            textComponent.text = $"{prefix}{version}{suffix}";
        }
        else
        {
            Debug.LogWarning("VersionDisplay: No TextMeshProUGUI component found on this GameObject.");
        }
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        UpdateVersionText();
    }
#endif
}
