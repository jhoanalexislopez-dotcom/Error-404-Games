using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

public class Inventory3DController : MonoBehaviour
{
    [Header("Container Settings")]
    [SerializeField] private Transform inventoryContainer;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveDistance = 2f;
    [SerializeField] private float moveSpeed = 5f;
    
    [Header("Item Bounds")]
    [SerializeField] private int minItemIndex = 0;
    [SerializeField] private int maxItemIndex = 4;
    
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
    private List<Transform> inventoryItems = new List<Transform>();
    private Dictionary<Transform, Vector3> originalEulerAngles = new Dictionary<Transform, Vector3>();
    private Transform currentlySelectedItem;
    private Transform previouslySelectedItem;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        
        inputActions.Player.Previous.performed += OnPreviousPressed;
        inputActions.Player.Next.performed += OnNextPressed;
        
        if (inventoryContainer != null)
        {
            targetPosition = inventoryContainer.localPosition;
            CacheInventoryItems();
        }
    }

    private void CacheInventoryItems()
    {
        inventoryItems.Clear();
        originalEulerAngles.Clear();
        
        foreach (Transform child in inventoryContainer)
        {
            inventoryItems.Add(child);
            originalEulerAngles[child] = child.localEulerAngles;
        }
        
        if (inventoryItems.Count > 0)
        {
            currentlySelectedItem = inventoryItems[currentItemIndex];
            UpdateDisplayText();
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

    private void Update()
    {
        if (isMoving && inventoryContainer != null)
        {
            inventoryContainer.localPosition = Vector3.Lerp(
                inventoryContainer.localPosition, 
                targetPosition, 
                Time.deltaTime * moveSpeed
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
            
            currentEuler.y += rotationSpeed * Time.deltaTime;
            
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
                currentEuler.y = Mathf.LerpAngle(currentEuler.y, originalEuler.y, Time.deltaTime * returnRotationSpeed);
                
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
        if (currentItemIndex > minItemIndex)
        {
            currentItemIndex--;
            MoveToItem(currentItemIndex);
        }
    }

    private void NavigateToNextItem()
    {
        if (currentItemIndex < maxItemIndex)
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
        if (currentlySelectedItem == null)
        {
            ClearDisplayText();
            return;
        }
        
        Collectible collectible = currentlySelectedItem.GetComponent<Collectible>();
        if (collectible != null)
        {
            if (titleText != null)
            {
                titleText.text = GetCollectibleTitle(collectible);
            }
            
            if (contentText != null)
            {
                contentText.text = GetCollectibleContent(collectible);
            }
        }
        else
        {
            CollectibleRecharge recharge = currentlySelectedItem.GetComponent<CollectibleRecharge>();
            if (recharge != null)
            {
                if (titleText != null)
                {
                    titleText.text = currentlySelectedItem.name;
                }
                
                if (contentText != null)
                {
                    contentText.text = "Battery item - restores flashlight charge";
                }
            }
            else
            {
                if (titleText != null)
                {
                    titleText.text = currentlySelectedItem.name;
                }
                
                if (contentText != null)
                {
                    contentText.text = "No description available";
                }
            }
        }
    }

    private string GetCollectibleTitle(Collectible collectible)
    {
        var field = collectible.GetType().GetField("noteTitle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            return field.GetValue(collectible) as string ?? "Unknown Item";
        }
        return "Unknown Item";
    }

    private string GetCollectibleContent(Collectible collectible)
    {
        var field = collectible.GetType().GetField("noteText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            return field.GetValue(collectible) as string ?? "No description available";
        }
        return "No description available";
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

    public void SetCurrentItem(int index)
    {
        currentItemIndex = Mathf.Clamp(index, minItemIndex, maxItemIndex);
        MoveToItem(currentItemIndex);
    }

    private void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.Player.Previous.performed -= OnPreviousPressed;
            inputActions.Player.Next.performed -= OnNextPressed;
            inputActions.Dispose();
        }
    }
}
