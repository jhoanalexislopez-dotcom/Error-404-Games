using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Localization;

public class NoteInventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject noteListContainer;
    [SerializeField] private GameObject noteButtonPrefab;
    [SerializeField] private TextMeshProUGUI noteContentText;
    [SerializeField] private TextMeshProUGUI noteTitleText;

    private bool isInventoryOpen = false;
    private List<Button> spawnedButtons = new List<Button>();
    private PauseMenuManager pauseMenuManager;
    private InputSystem_Actions inputActions;
    private int currentlySelectedNoteIndex = -1;
    private MobilePhoneToggle mobilePhoneToggle;
    private FlashlightController flashlightController;
    
    public bool IsInventoryOpen => isInventoryOpen;

    private void Awake()
    {
        pauseMenuManager = FindObjectOfType<PauseMenuManager>();
        mobilePhoneToggle = FindObjectOfType<MobilePhoneToggle>();
        flashlightController = FindObjectOfType<FlashlightController>();
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
        if (mobilePhoneToggle != null && mobilePhoneToggle.IsPhoneVisible)
        {
            return;
        }
        
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
        
        if (flashlightController != null)
        {
            flashlightController.SetFlashlightInputEnabled(false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        var playerController = FindObjectOfType<FirstPersonController>();
        if (playerController != null)
        {
            playerController.enabled = false;
            playerController.SetPlayerInputEnabled(false);
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
        
        
        if (flashlightController != null)
        {
            flashlightController.SetFlashlightInputEnabled(true);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        var playerController = FindObjectOfType<FirstPersonController>();
        if (playerController != null)
        {
            playerController.enabled = true;
            playerController.SetPlayerInputEnabled(true);
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

            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                string titleText = notes[i].noteTitle != null && !notes[i].noteTitle.IsEmpty 
                    ? notes[i].noteTitle.GetLocalizedString() 
                    : "Untitled";
                buttonText.text = titleText;
            }

            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnNoteButtonClicked(index));
                spawnedButtons.Add(button);
            }
        }

        SetupButtonNavigation();

        if (notes.Count > 0)
        {
            DisplayNote(notes.Count - 1);
            SelectButton(notes.Count - 1);
        }
    }

    private void SetupButtonNavigation()
    {
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            Navigation nav = new Navigation();
            nav.mode = Navigation.Mode.Explicit;

            if (i > 0)
                nav.selectOnUp = spawnedButtons[i - 1];

            if (i < spawnedButtons.Count - 1)
                nav.selectOnDown = spawnedButtons[i + 1];

            spawnedButtons[i].navigation = nav;
        }
    }

    private void OnNoteButtonClicked(int index)
    {
        DisplayNote(index);
        currentlySelectedNoteIndex = index;
    }

    private void SelectButton(int index)
    {
        if (index >= 0 && index < spawnedButtons.Count)
        {
            currentlySelectedNoteIndex = index;
            spawnedButtons[index].Select();
            EventSystem.current.SetSelectedGameObject(spawnedButtons[index].gameObject);
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
        {
            string titleText = selectedNote.noteTitle != null && !selectedNote.noteTitle.IsEmpty 
                ? selectedNote.noteTitle.GetLocalizedString() 
                : "Untitled";
            noteTitleText.text = titleText;
        }

        if (noteContentText != null)
        {
            string contentText = selectedNote.noteContent != null && !selectedNote.noteContent.IsEmpty 
                ? selectedNote.noteContent.GetLocalizedString() 
                : "";
            noteContentText.text = contentText;
        }
    }

    private void ClearNoteList()
    {
        foreach (Button button in spawnedButtons)
        {
            if (button != null)
                Destroy(button.gameObject);
        }
        spawnedButtons.Clear();
        currentlySelectedNoteIndex = -1;
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
