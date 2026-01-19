using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

public class Inventory3DController : MonoBehaviour
{
    [Header("Container Settings")]
    [SerializeField] private Transform inventoryContainer;
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private Camera inventoryCamera;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveDistance = 2f;
    [SerializeField] private float moveSpeed = 5f;
    
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float returnRotationSpeed = 10f;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI hintText;
    
    [Header("Note UI")]
    [SerializeField] private NoteUIManager noteUIManager;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioClip navigateSound;
    [SerializeField] private AudioClip selectSound;
    
    private InputSystem_Actions inputActions;
    private int currentItemIndex = 0;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private bool isInventoryOpen = false;
    private List<Transform> inventoryItems = new List<Transform>();
    private List<NoteData> collectedNotes = new List<NoteData>();
    private Dictionary<Transform, Vector3> originalEulerAngles = new Dictionary<Transform, Vector3>();
    private Transform currentlySelectedItem;
    private Transform previouslySelectedItem;
    private PauseMenuManager pauseMenuManager;
    private MobilePhoneToggle mobilePhoneToggle;
    private float lastNavigateTime = 0f;
    private float navigateCooldown = 0.2f;
    
    public bool IsInventoryOpen => isInventoryOpen;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        
        inputActions.UI.Navigate.performed += OnNavigate;
        inputActions.Player.Inventory.performed += OnInventoryPressed;
        inputActions.Player.Interact.started += OnInteractPressed;
        
        if (inventoryContainer != null)
        {
            targetPosition = inventoryContainer.localPosition;
        }
        
        if (noteUIManager == null)
        {
            noteUIManager = FindObjectOfType<NoteUIManager>();
        }
        
        if (pauseMenuManager == null)
        {
            pauseMenuManager = FindObjectOfType<PauseMenuManager>();
        }
        
        if (mobilePhoneToggle == null)
        {
            mobilePhoneToggle = FindObjectOfType<MobilePhoneToggle>();
        }
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
        
        CloseInventory();
    }
    
    private void OnEnable()
    {
        inputActions?.UI.Enable();
        inputActions?.Player.Enable();
    }
    
    private void OnDisable()
    {
        inputActions?.UI.Disable();
        inputActions?.Player.Disable();
    }

    private void CacheInventoryItems()
    {
        inventoryItems.Clear();
        originalEulerAngles.Clear();
        
        if (PlayerInventory.Instance == null)
        {
            Debug.LogWarning("PlayerInventory instance not found!");
            return;
        }
        
        collectedNotes = PlayerInventory.Instance.GetCollectedNotes();
        
        int childIndex = 0;
        foreach (Transform child in inventoryContainer)
        {
            if (childIndex < collectedNotes.Count)
            {
                child.gameObject.SetActive(true);
                inventoryItems.Add(child);
                originalEulerAngles[child] = child.localEulerAngles;
            }
            else
            {
                child.gameObject.SetActive(false);
            }
            childIndex++;
        }
        
        if (inventoryItems.Count > 0)
        {
            currentItemIndex = 0;
            currentlySelectedItem = inventoryItems[currentItemIndex];
            MoveToItem(currentItemIndex);
        }
        else
        {
            ClearDisplayText();
        }
    }
    
    private void OnInventoryPressed(InputAction.CallbackContext context)
    {
        if (mobilePhoneToggle != null && mobilePhoneToggle.IsPhoneVisible)
        {
            Debug.Log("Cannot open inventory while phone is visible!");
            return;
        }
        
        ToggleInventory();
    }
    
    private void OnInteractPressed(InputAction.CallbackContext context)
    {
        Debug.Log($"Interact pressed! Inventory open: {isInventoryOpen}, Notes count: {collectedNotes.Count}, Current index: {currentItemIndex}");
        
        if (isInventoryOpen && collectedNotes.Count > 0 && currentItemIndex < collectedNotes.Count)
        {
            OpenSelectedNote();
        }
    }
    
    private void OpenSelectedNote()
    {
        if (noteUIManager == null)
        {
            Debug.LogWarning("NoteUIManager not found! Cannot open note.");
            return;
        }
        
        if (currentItemIndex < 0 || currentItemIndex >= collectedNotes.Count)
        {
            Debug.LogWarning("Invalid note index: " + currentItemIndex);
            return;
        }
        
        NoteData selectedNote = collectedNotes[currentItemIndex];
        Debug.Log("Opening note: " + selectedNote.noteTitle);
        
        PlaySound(selectSound);
        
        CloseInventory();
        
        noteUIManager.gameObject.SetActive(true);
        noteUIManager.SetNoteActive(selectedNote.noteContent);
        
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        FirstPersonController playerController = FindObjectOfType<FirstPersonController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        if (pauseMenuManager != null)
        {
            pauseMenuManager.enabled = false;
        }
    }
    
    public void ToggleInventory()
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
        if (noteUIManager != null && noteUIManager.IsNoteActive)
        {
            Debug.Log("Cannot open inventory while reading a note!");
            return;
        }
        
        if (pauseMenuManager != null && pauseMenuManager.IsPaused)
        {
            Debug.Log("Cannot open inventory while pause menu is open!");
            return;
        }
        
        if (CinematicManager.IsCinematicActive)
        {
            Debug.Log("Cannot open inventory during a cinematic!");
            return;
        }
        
        CacheInventoryItems();
        
        if (inventoryItems.Count == 0)
        {
            Debug.Log("No notes collected yet!");
            return;
        }
        
        isInventoryOpen = true;
        
        PlaySound(openSound);
        
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(true);
        }
        
        if (inventoryCamera != null)
        {
            inventoryCamera.enabled = true;
        }
        
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        UpdateDisplayText();
    }
    
    private void CloseInventory()
    {
        if (isInventoryOpen)
        {
            PlaySound(closeSound);
        }
        
        isInventoryOpen = false;
        
        ResetAllRotations();
        
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(false);
        }
        
        if (inventoryCamera != null)
        {
            inventoryCamera.enabled = false;
        }
        
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void ResetAllRotations()
    {
        foreach (Transform item in inventoryItems)
        {
            if (originalEulerAngles.ContainsKey(item))
            {
                item.localEulerAngles = originalEulerAngles[item];
            }
        }
        
        previouslySelectedItem = null;
    }

    private void Update()
    {
        if (!isInventoryOpen) return;
        
        if (isMoving && inventoryContainer != null)
        {
            inventoryContainer.localPosition = Vector3.Lerp(
                inventoryContainer.localPosition, 
                targetPosition, 
                Time.unscaledDeltaTime * moveSpeed
            );

            if (Vector3.Distance(inventoryContainer.localPosition, targetPosition) < 0.01f)
            {
                inventoryContainer.localPosition = targetPosition;
                isMoving = false;
            }
        }
        
        if (currentlySelectedItem != null && originalEulerAngles.ContainsKey(currentlySelectedItem))
        {
            Vector3 currentEuler = currentlySelectedItem.localEulerAngles;
            Vector3 originalEuler = originalEulerAngles[currentlySelectedItem];
            
            currentEuler.y += rotationSpeed * Time.unscaledDeltaTime;
            
            currentEuler.x = originalEuler.x;
            currentEuler.z = originalEuler.z;
            
            currentlySelectedItem.localEulerAngles = currentEuler;
        }
        
        if (previouslySelectedItem != null && previouslySelectedItem != currentlySelectedItem)
        {
            if (originalEulerAngles.ContainsKey(previouslySelectedItem))
            {
                Vector3 currentEuler = previouslySelectedItem.localEulerAngles;
                Vector3 originalEuler = originalEulerAngles[previouslySelectedItem];
                
                float yDiff = Mathf.DeltaAngle(currentEuler.y, originalEuler.y);
                currentEuler.y = Mathf.LerpAngle(currentEuler.y, originalEuler.y, Time.unscaledDeltaTime * returnRotationSpeed);
                
                currentEuler.x = originalEuler.x;
                currentEuler.z = originalEuler.z;
                
                previouslySelectedItem.localEulerAngles = currentEuler;
                
                if (Mathf.Abs(yDiff) < 0.1f)
                {
                    previouslySelectedItem.localEulerAngles = originalEuler;
                    previouslySelectedItem = null;
                }
            }
        }
    }

    private void OnNavigate(InputAction.CallbackContext context)
    {
        if (!isInventoryOpen) return;
        
        if (Time.unscaledTime - lastNavigateTime < navigateCooldown)
            return;
        
        Vector2 navigation = context.ReadValue<Vector2>();
        
        if (navigation.x < -0.5f)
        {
            NavigateToPreviousItem();
            lastNavigateTime = Time.unscaledTime;
        }
        else if (navigation.x > 0.5f)
        {
            NavigateToNextItem();
            lastNavigateTime = Time.unscaledTime;
        }
    }

    private void NavigateToPreviousItem()
    {
        if (currentItemIndex > 0)
        {
            currentItemIndex--;
            MoveToItem(currentItemIndex);
            PlaySound(navigateSound);
        }
    }

    private void NavigateToNextItem()
    {
        if (currentItemIndex < inventoryItems.Count - 1)
        {
            currentItemIndex++;
            MoveToItem(currentItemIndex);
            PlaySound(navigateSound);
        }
    }

    private void MoveToItem(int itemIndex)
    {
        float targetX = -itemIndex * moveDistance;
        targetPosition = new Vector3(targetX, inventoryContainer.localPosition.y, inventoryContainer.localPosition.z);
        isMoving = true;
        
        if (itemIndex >= 0 && itemIndex < inventoryItems.Count)
        {
            previouslySelectedItem = currentlySelectedItem;
            currentlySelectedItem = inventoryItems[itemIndex];
            UpdateDisplayText();
        }
    }

    private void UpdateDisplayText()
    {
        if (currentlySelectedItem == null || currentItemIndex >= collectedNotes.Count)
        {
            ClearDisplayText();
            return;
        }
        
        NoteData currentNote = collectedNotes[currentItemIndex];
        
        if (titleText != null)
        {
            titleText.text = currentNote.noteTitle;
        }
        
        if (hintText != null)
        {
            hintText.text = "Press Interact to Read";
        }
    }

    private void ClearDisplayText()
    {
        if (titleText != null)
        {
            titleText.text = "";
        }
        
        if (hintText != null)
        {
            hintText.text = "";
        }
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.UI.Navigate.performed -= OnNavigate;
            inputActions.Player.Inventory.performed -= OnInventoryPressed;
            inputActions.Player.Interact.started -= OnInteractPressed;
            inputActions.Dispose();
        }
    }
}
