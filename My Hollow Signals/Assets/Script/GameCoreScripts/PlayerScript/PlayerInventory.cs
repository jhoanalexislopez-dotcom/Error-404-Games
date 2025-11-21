/*******************************************************
 * Author: [Jhoan Alexis Lopez]
 * Last Modified: [21/11/2025]
 * Description:
 *    Singleton inventory system tracking collected items.
 *******************************************************/
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
    public TextMeshProUGUI batteryUI;

    [SerializeField] private FlashlightController flashlight;


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

    void Update()
    {
        if (batteryUI != null && flashlight != null)
        {
            batteryUI.text = $"{Mathf.RoundToInt(flashlight.battery)}%";
        }
    }

    private void UpdateUI()
    {
        if (counterText != null)
        {
            counterText.text = $"{collected}/{target}";
        }
        if (batteryUI != null) {
            batteryUI.text = $"{flashlight.battery}";
        }
    }

    private void OnAllCollected()
    {
        // Aquí puedes lanzar un evento, cargar escena, mostrar mensaje, etc.
        Debug.Log("¡Has recogido todos los objetos!");
    }
}