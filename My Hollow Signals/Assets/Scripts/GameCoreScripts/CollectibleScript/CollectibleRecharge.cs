/*******************************************************
 * Author: [Ignacio Lopez]
 * Last Modified: [21/11/2025]
 * Description:
 *    Represents rechargable items that players can pick up. Implements the IInteractable interface.
 *******************************************************/

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

public class CollectibleRecharge : MonoBehaviour, IInteractable
{
    [SerializeField] private LocalizedString localizedDescription;
    [SerializeField] private FlashlightController flashlight;

    [Header("Sanity Settings")]
    [Tooltip("Amount of sanity to lower when this item is collected")]
    [SerializeField] private float sanityLossAmount = 0f;

    void Start()
    {

    }

    public LocalizedString GetLocalizedDescription()
    {
        return localizedDescription;
    }

    public void Interact()
    {
        if (sanityLossAmount > 0f)
        {
            SanityManager sanityManager = FindObjectOfType<SanityManager>(true);
            if (sanityManager != null)
            {
                sanityManager.LowerSanity(sanityLossAmount);
            }
        }

        flashlight.RechargeBattery();

        Destroy(gameObject);
    }
}
