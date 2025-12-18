using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class NoteInventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject noteListContainer;
    [SerializeField] private GameObject noteButtonPrefab;
    [SerializeField] private TextMeshProUGUI noteContentText;
    [SerializeField] private TextMeshProUGUI noteTitleText;

    private bool isInventoryOpen = false;
    private List<GameObject> spawnedButtons = new List<GameObject>();
    private PauseMenuManager pauseMenuManager;
    private InputSystem_Actions inputActions;

    private void Awake()
    {
        pauseMenuManager = FindObjectOfType<PauseMenuManager>();
        inputActions = new InputSystem_Actions();

        inputActions.Player.Inventory.performed += OnInventoryPressed;

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
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
        inventoryPanel.SetActive(true);

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

        PopulateNoteList();
    }

    private void CloseInventory()
    {
        isInventoryOpen = false;
        inventoryPanel.SetActive(false);

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

    private void PopulateNoteList()
    {
        ClearNoteList();

        if (PlayerInventory.Instance == null)
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
            int index = i;
            GameObject buttonObj = Instantiate(noteButtonPrefab, noteListContainer.transform);
            spawnedButtons.Add(buttonObj);

            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = notes[i].noteTitle;
            }

            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => DisplayNote(index));
            }
        }

        if (notes.Count > 0)
        {
            DisplayNote(notes.Count - 1);
        }
    }

    private void DisplayNote(int noteIndex)
    {
        if (PlayerInventory.Instance == null)
            return;

        List<NoteData> notes = PlayerInventory.Instance.GetCollectedNotes();

        if (noteIndex < 0 || noteIndex >= notes.Count)
            return;

        NoteData selectedNote = notes[noteIndex];

        if (noteTitleText != null)
            noteTitleText.text = selectedNote.noteTitle;

        if (noteContentText != null)
            noteContentText.text = selectedNote.noteContent;
    }

    private void ClearNoteList()
    {
        foreach (GameObject button in spawnedButtons)
        {
            if (button != null)
                Destroy(button);
        }
        spawnedButtons.Clear();
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
