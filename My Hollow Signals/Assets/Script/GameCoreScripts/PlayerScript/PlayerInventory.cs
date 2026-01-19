/*******************************************************
 * Author: [Jhoan Alexis Lopez]
 * Last Modified: [21/11/2025]
 * Description:
 *    Singleton inventory system tracking collected items.
 *******************************************************/
using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class NoteData
{
    public string noteTitle;
    public string noteContent;

    public NoteData(string title, string content)
    {
        noteTitle = title;
        noteContent = content;
    }
}

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    [Header("Conteo de coleccionables")]
    [Tooltip("Objetos recogidos actualmente")]
    public int collected = 0;

    [Tooltip("Objetivo total (p. ej., 3)")]
    public int target = 3;

    [Header("UI (opcional)")]
    [Tooltip("Arrastra aqu� un TextMeshProUGUI para mostrar el contador")]
    public TextMeshProUGUI counterText;
    public TextMeshProUGUI batteryUI;

    [SerializeField] private FlashlightController flashlight;

    private List<NoteData> collectedNotes = new List<NoteData>();
    private bool hasFlashlight = false;

    public bool HasFlashlight => hasFlashlight;

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

    public void CollectFlashlight()
    {
        hasFlashlight = true;
        Debug.Log("[PlayerInventory] Flashlight added to inventory! HasFlashlight is now: " + hasFlashlight);
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

    public void AddNote(string noteTitle, string noteContent)
    {
        NoteData newNote = new NoteData(noteTitle, noteContent);
        collectedNotes.Add(newNote);
        Debug.Log($"Note '{noteTitle}' added to inventory. Total notes: {collectedNotes.Count}");
    }

    public List<NoteData> GetCollectedNotes()
    {
        return new List<NoteData>(collectedNotes);
    }

    public int GetNoteCount()
    {
        return collectedNotes.Count;
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
        // Aqu� puedes lanzar un evento, cargar escena, mostrar mensaje, etc.
        Debug.Log("�Has recogido todos los objetos!");
    }
}