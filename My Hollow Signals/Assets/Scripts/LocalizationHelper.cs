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

        Debug.Log($"Configured {localizeEvents.Length} LocalizeStringEvent components to wait for completion.");
    }
}
