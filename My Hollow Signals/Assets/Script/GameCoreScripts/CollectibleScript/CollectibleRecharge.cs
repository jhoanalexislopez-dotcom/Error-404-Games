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
