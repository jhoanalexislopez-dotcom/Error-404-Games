using UnityEngine;

public class Collectible : MonoBehaviour, IInteractable
{
    [SerializeField] private string description = "Pick up";
    [SerializeField] private int value = 1; // por si algún objeto vale más de 1

    public string GetDescription()
    {
        return description;
    }

    public void Interact()
    {
        // Sumar al inventario si existe
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.AddItem(value);
        }

        // Destruir el objeto recogido
        Destroy(gameObject);
    }
}