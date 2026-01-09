using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

public class Inventory3DManager : MonoBehaviour
{
    [Header("3D Inventory References")]
    [SerializeField] private GameObject inventory3DContainer;
    [SerializeField] private Camera inventoryCamera;
    [SerializeField] private GameObject note3DModel;
    
    [Header("Materials")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material selectedMaterial;
    
    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private TextMeshProUGUI noteContentText;
    [SerializeField] private TextMeshProUGUI noteTitleText;
    
    [Header("3D Layout Settings")]
    [SerializeField] private float noteSpacing = 2f;
    [SerializeField] private float arcRadius = 4f;
    [SerializeField] private float arcAngle = 60f;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float autoRotateSpeed = 20f;
    
    private bool isInventoryOpen = false;
    private List<Interactive3DNote> spawnedNotes = new List<Interactive3DNote>();
    private int currentSelectedNoteIndex = -1;
    private InputSystem_Actions inputActions;
    private PauseMenuManager pauseMenuManager;
    
    private void Awake()
    {
        pauseMenuManager = FindObjectOfType<PauseMenuManager>();
        inputActions = new InputSystem_Actions();
        inputActions.Player.Inventory.performed += OnInventoryPressed;
        
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
        
        if (inventory3DContainer != null)
        {
            inventory3DContainer.SetActive(false);
        }
        
        if (inventoryCamera != null)
        {
            inventoryCamera.enabled = false;
        }
    }
    
    private void OnEnable()
    {
        inputActions?.Player.Enable();
    }
    
    private void OnDisable()
    {
        inputActions?.Player.Disable();
    }
    
    private void OnInventoryPressed(InputAction.CallbackContext context)
    {
        if (isInventoryOpen)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();
        }
    }
    
    private void OpenInventory()
    {
        if (PlayerInventory.Instance == null)
        {
            Debug.LogWarning("PlayerInventory instance not found!");
            return;
        }
        
        isInventoryOpen = true;
        
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
        }
        
        if (inventory3DContainer != null)
        {
            inventory3DContainer.SetActive(true);
        }
        
        if (inventoryCamera != null)
        {
            inventoryCamera.enabled = true;
        }
        
        Time.timeScale = 0f;
        
        if (pauseMenuManager != null)
        {
            pauseMenuManager.enabled = false;
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        var playerController = FindObjectOfType<FirstPersonController>();
        if (playerController != null)
        {
            var playerInput = playerController.GetComponent<PlayerInput>();
            if (playerInput != null)
                playerInput.enabled = false;
        }
        
        SpawnNotes3D();
    }
    
    private void CloseInventory()
    {
        isInventoryOpen = false;
        
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
        
        if (inventory3DContainer != null)
        {
            inventory3DContainer.SetActive(false);
        }
        
        if (inventoryCamera != null)
        {
            inventoryCamera.enabled = false;
        }
        
        DestroySpawnedNotes();
        
        Time.timeScale = 1f;
        
        if (pauseMenuManager != null)
        {
            pauseMenuManager.enabled = true;
        }
        
        var playerController = FindObjectOfType<FirstPersonController>();
        if (playerController != null)
        {
            var playerInput = playerController.GetComponent<PlayerInput>();
            if (playerInput != null)
                playerInput.enabled = true;
        }
    }
    
    private void SpawnNotes3D()
    {
        DestroySpawnedNotes();
        
        if (PlayerInventory.Instance == null || note3DModel == null || inventory3DContainer == null)
            return;
        
        List<NoteData> notes = PlayerInventory.Instance.GetCollectedNotes();
        
        if (notes.Count == 0)
        {
            if (noteTitleText != null)
                noteTitleText.text = "No Notes Collected";
            if (noteContentText != null)
                noteContentText.text = "You haven't picked up any notes yet.";
            return;
        }
        
        for (int i = 0; i < notes.Count; i++)
        {
            GameObject noteObj = Instantiate(note3DModel, inventory3DContainer.transform);
            
            float angle = 0f;
            if (notes.Count > 1)
            {
                angle = Mathf.Lerp(-arcAngle / 2f, arcAngle / 2f, i / (float)(notes.Count - 1));
            }
            
            float radians = angle * Mathf.Deg2Rad;
            Vector3 position = new Vector3(
                Mathf.Sin(radians) * arcRadius,
                0f,
                Mathf.Cos(radians) * arcRadius
            );
            
            noteObj.transform.localPosition = position;
            noteObj.transform.localRotation = Quaternion.Euler(0f, -angle, 0f);
            
            Interactive3DNote interactive = noteObj.GetComponent<Interactive3DNote>();
            if (interactive == null)
            {
                interactive = noteObj.AddComponent<Interactive3DNote>();
            }
            
            interactive.Initialize(notes[i], i, normalMaterial, selectedMaterial, this);
            
            spawnedNotes.Add(interactive);
        }
        
        if (notes.Count > 0)
        {
            SelectNote(notes.Count - 1);
        }
    }
    
    public void SelectNote(int index)
    {
        if (index < 0 || index >= spawnedNotes.Count)
            return;
        
        for (int i = 0; i < spawnedNotes.Count; i++)
        {
            spawnedNotes[i].SetSelected(i == index);
        }
        
        currentSelectedNoteIndex = index;
        DisplayNoteContent(index);
    }
    
    private void DisplayNoteContent(int index)
    {
        if (index < 0 || index >= spawnedNotes.Count)
            return;
        
        NoteData note = spawnedNotes[index].GetNoteData();
        
        if (noteTitleText != null)
            noteTitleText.text = note.noteTitle;
        
        if (noteContentText != null)
            noteContentText.text = note.noteContent;
    }
    
    private void DestroySpawnedNotes()
    {
        foreach (var note in spawnedNotes)
        {
            if (note != null)
                Destroy(note.gameObject);
        }
        spawnedNotes.Clear();
        currentSelectedNoteIndex = -1;
    }
    
    private void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.Player.Inventory.performed -= OnInventoryPressed;
            inputActions.Dispose();
        }
        
        if (isInventoryOpen)
        {
            Time.timeScale = 1f;
            if (pauseMenuManager != null)
            {
                pauseMenuManager.enabled = true;
            }
        }
    }
}
