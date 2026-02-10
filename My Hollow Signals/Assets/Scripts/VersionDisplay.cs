/*******************************************************
 * Author: [Ignacio Lopez]
 * Last Modified: [31/02/2026]
 * Description:
 *    This script displays the current game version on a TextMeshProUGUI component. It allows for customizable prefix and suffix to format the version display as desired. The script automatically updates the version text when the game starts and also provides an option to update it in the editor when changes are made to the prefix or suffix. This is useful for ensuring that players can easily see the game version, which can be helpful for debugging and support purposes.
 *******************************************************/

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
