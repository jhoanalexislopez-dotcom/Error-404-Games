/*******************************************************
 * Author: [Ignacio Lopez]
 * Last Modified: [21/11/2025]
 * Description:
 *    Represents rechargable items that players can pick up. Implements the IInteractable interface.
 *******************************************************/

using UnityEngine;
using UnityEngine.InputSystem;

public class CollectibleRecharge : MonoBehaviour, IInteractable
{
    [SerializeField] private string description = "Recharge";
    [SerializeField] private FlashlightController flashlight;

    void Start()
    {

    }

    public string GetDescription()
    {
        return description;
    }

    public void Interact()
    {
        flashlight.RechargeBattery();

        // Destroy the collectible object
        Destroy(gameObject);
    }
}
