/*******************************************************
 * Author: [Ignacio Lopez]
 * Last Modified: [25/01/2026]
 * Description:
 *    This script provides helper functionality for localization in the game. It includes a method to automatically set the WaitForCompletion property to true on all LocalizeStringEvent components in the children of the GameObject this script is attached to. This ensures that localized strings will wait for the localization process to complete before being displayed, which can help prevent issues with missing or incorrect text during localization.
 *******************************************************/


using UnityEngine;
using UnityEngine.Localization.Components;

public class LocalizationHelper : MonoBehaviour
{
    [Header("Auto-Configure Wait For Completion")]
    [Tooltip("Automatically set WaitForCompletion to true on all LocalizeStringEvent components in children")]
    public bool configureOnAwake = true;

    private void Awake()
    {
        if (configureOnAwake)
        {
            ConfigureAllLocalizedStrings();
        }
    }

    [ContextMenu("Configure All Localized Strings")]
    public void ConfigureAllLocalizedStrings()
    {
        LocalizeStringEvent[] localizeEvents = GetComponentsInChildren<LocalizeStringEvent>(true);

        foreach (LocalizeStringEvent localizeEvent in localizeEvents)
        {
            if (localizeEvent.StringReference != null)
            {
                localizeEvent.StringReference.WaitForCompletion = true;
            }
        }
    }
}
