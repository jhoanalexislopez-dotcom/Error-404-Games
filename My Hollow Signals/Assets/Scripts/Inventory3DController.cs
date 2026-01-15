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
    [SerializeField] private TextMeshProUGUI contentText;
    
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
    private PlayerInput playerInput;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        
        inputActions.Player.Previous.performed += OnPreviousPressed;
        inputActions.Player.Next.performed += OnNextPressed;
        inputActions.Player.Inventory.performed += OnInventoryPressed;
        
        if (inventoryContainer != null)
        {
            targetPosition = inventoryContainer.localPosition;
        }
        
        CloseInventory();
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
        ToggleInventory();
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
        CacheInventoryItems();
        
        if (inventoryItems.Count == 0)
        {
            Debug.Log("No notes collected yet!");
            return;
        }
        
        isInventoryOpen = true;
        
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(true);
        }
        
        if (inventoryCamera != null)
        {
            inventoryCamera.enabled = true;
        }
        
        playerInput = FindObjectOfType<FirstPersonController>()?.GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = false;
        }
        
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        UpdateDisplayText();
    }
    
    private void CloseInventory()
    {
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
        
        if (playerInput != null)
        {
            playerInput.enabled = true;
            playerInput = null;
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

    private void OnEnable()
    {
        inputActions?.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions?.Player.Disable();
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

    private void OnPreviousPressed(InputAction.CallbackContext context)
    {
        NavigateToPreviousItem();
    }

    private void OnNextPressed(InputAction.CallbackContext context)
    {
        NavigateToNextItem();
    }

    private void NavigateToPreviousItem()
    {
        if (currentItemIndex > 0)
        {
            currentItemIndex--;
            MoveToItem(currentItemIndex);
        }
    }

    private void NavigateToNextItem()
    {
        if (currentItemIndex < inventoryItems.Count - 1)
        {
            currentItemIndex++;
            MoveToItem(currentItemIndex);
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
        
        if (contentText != null)
        {
            contentText.text = currentNote.noteContent;
        }
    }

    private void ClearDisplayText()
    {
        if (titleText != null)
        {
            titleText.text = "";
        }
        
        if (contentText != null)
        {
            contentText.text = "";
        }
    }

    private void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.Player.Previous.performed -= OnPreviousPressed;
            inputActions.Player.Next.performed -= OnNextPressed;
            inputActions.Player.Inventory.performed -= OnInventoryPressed;
            inputActions.Dispose();
        }
    }
}
