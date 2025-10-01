using UnityEngine;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    [Header("Conteo de coleccionables")]
    [Tooltip("Objetos recogidos actualmente")]
    public int collected = 0;

    [Tooltip("Objetivo total (p. ej., 3)")]
    public int target = 3;

    [Header("UI (opcional)")]
    [Tooltip("Arrastra aquí un TextMeshProUGUI para mostrar el contador")]
    public TextMeshProUGUI counterText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        UpdateUI();
    }

    public void AddItem(int amount = 1)
    {
        collected += amount;
        UpdateUI();

        if (collected >= target)
        {
            OnAllCollected();
        }
    }

    private void UpdateUI()
    {
        if (counterText != null)
        {
            counterText.text = $"{collected}/{target}";
        }
    }

    private void OnAllCollected()
    {
        // Aquí puedes lanzar un evento, cargar escena, mostrar mensaje, etc.
        Debug.Log("¡Has recogido todos los objetos!");
    }
}