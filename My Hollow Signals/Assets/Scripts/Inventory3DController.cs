/*******************************************************
 * Author: [Ignacio Lopez]
 * Last Modified: [27/01/2026]
 * Description:
 *    This script manages the 3D inventory system, allowing players to view and interact with collected items such as notes and the flashlight. It handles opening and closing the inventory, navigating between items, displaying item information, and integrating with the note UI for reading notes. The script also includes functionality for rotating the displayed items and playing audio feedback for inventory interactions.
 *******************************************************/


using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Localization;

public class Inventory3DController : MonoBehaviour
{
    [Header("Container Settings")]
    [SerializeField] private Transform inventoryContainer;
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private Camera inventoryCamera;
    
    [Header("Flashlight Inventory")]
    [Tooltip("The 3D flashlight model to show in inventory when collected")]
    [SerializeField] private GameObject flashlightInventoryModel;
    
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
    
    [Header("Localized UI Text")]
    [Tooltip("Localized text for 'Press Interact to Read' hint")]
    [SerializeField] private LocalizedString localizedInteractHintText;
    [Tooltip("Localized text for 'Flashlight' title")]
    [SerializeField] private LocalizedString localizedFlashlightText;
    
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
        
        // Handle flashlight inventory model (now selectable)
        if (flashlightInventoryModel != null)
        {
            bool hasFlashlight = PlayerInventory.Instance.HasFlashlight;
            Debug.Log($"[Inventory] Has flashlight: {hasFlashlight}, FlashlightInventoryModel: {flashlightInventoryModel.name}");
            
            if (hasFlashlight)
            {
                flashlightInventoryModel.SetActive(true);
                inventoryItems.Add(flashlightInventoryModel.transform);
                originalEulerAngles[flashlightInventoryModel.transform] = flashlightInventoryModel.transform.localEulerAngles;
                Debug.Log($"[Inventory] Flashlight model activated and added to selectable items");
            }
            else
            {
                flashlightInventoryModel.SetActive(false);
                Debug.Log($"[Inventory] Flashlight model deactivated");
            }
        }
        else
        {
            Debug.LogWarning("[Inventory] FlashlightInventoryModel field is NULL! Please assign it in the Inspector.");
        }
        
        // Handle notes (added after flashlight)
        collectedNotes = PlayerInventory.Instance.GetCollectedNotes();
        
        int childIndex = 0;
        foreach (Transform child in inventoryContainer)
        {
            // Skip the flashlight model since we already handled it
            if (flashlightInventoryModel != null && child.gameObject == flashlightInventoryModel)
            {
                continue;
            }
            
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
        
        if (!isInventoryOpen || currentlySelectedItem == null)
        {
            return;
        }
        
        // Check if flashlight is selected - don't open note for flashlight
        if (flashlightInventoryModel != null && currentlySelectedItem.gameObject == flashlightInventoryModel)
        {
            Debug.Log("Flashlight selected - no interaction available");
            return;
        }
        
        // Otherwise, try to open the note
        OpenSelectedNote();
    }
    
    private void OpenSelectedNote()
    {
        if (noteUIManager == null)
        {
            Debug.LogWarning("NoteUIManager not found! Cannot open note.");
            return;
        }
        
        bool hasFlashlight = PlayerInventory.Instance != null && PlayerInventory.Instance.HasFlashlight;
        int noteIndex = hasFlashlight ? currentItemIndex - 1 : currentItemIndex;
        
        if (noteIndex < 0 || noteIndex >= collectedNotes.Count)
        {
            Debug.LogWarning("Invalid note index: " + noteIndex);
            return;
        }
        
        NoteData selectedNote = collectedNotes[noteIndex];
        string noteTitleText = selectedNote.noteTitle != null && !selectedNote.noteTitle.IsEmpty 
            ? selectedNote.noteTitle.GetLocalizedString() 
            : "Untitled";
        Debug.Log("Opening note: " + noteTitleText);
        
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
        
        // Check if player has any items (notes OR flashlight)
        bool hasFlashlight = PlayerInventory.Instance != null && PlayerInventory.Instance.HasFlashlight;
        bool hasNotes = inventoryItems.Count > 0;
        
        if (!hasFlashlight && !hasNotes)
        {
            Debug.Log("No items collected yet!");
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
    
    public void CloseInventory()
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
        // Check if flashlight is collected to determine offset
        // If flashlight is NOT collected, notes are at X=2,4,6 instead of 0,2,4
        // Container moves opposite direction, so offset is NEGATIVE
        bool hasFlashlight = PlayerInventory.Instance != null && PlayerInventory.Instance.HasFlashlight;
        float offset = hasFlashlight ? 0f : -moveDistance;
        
        float targetX = (-itemIndex * moveDistance) + offset;
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
        if (currentlySelectedItem == null)
        {
            ClearDisplayText();
            return;
        }
        
        // Check if the selected item is the flashlight
        if (flashlightInventoryModel != null && currentlySelectedItem.gameObject == flashlightInventoryModel)
        {
            if (titleText != null)
            {
                string flashlightText = localizedFlashlightText != null && !localizedFlashlightText.IsEmpty 
                    ? localizedFlashlightText.GetLocalizedString() 
                    : "Flashlight";
                titleText.text = flashlightText;
            }
            
            if (hintText != null)
            {
                hintText.text = "";
            }
            return;
        }
        
        // Otherwise, it's a note - calculate the note index
        // The note index is the current item index minus 1 if flashlight is collected (since flashlight is first)
        bool hasFlashlight = PlayerInventory.Instance != null && PlayerInventory.Instance.HasFlashlight;
        int noteIndex = hasFlashlight ? currentItemIndex - 1 : currentItemIndex;
        
        if (noteIndex < 0 || noteIndex >= collectedNotes.Count)
        {
            ClearDisplayText();
            return;
        }
        
        NoteData currentNote = collectedNotes[noteIndex];
        
        if (titleText != null)
        {
            string noteTitleText = currentNote.noteTitle != null && !currentNote.noteTitle.IsEmpty 
                ? currentNote.noteTitle.GetLocalizedString() 
                : "Untitled";
            titleText.text = noteTitleText;
        }
        
        if (hintText != null)
        {
            string interactHint = localizedInteractHintText != null && !localizedInteractHintText.IsEmpty 
                ? localizedInteractHintText.GetLocalizedString() 
                : "Press Interact to Read";
            hintText.text = interactHint;
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
