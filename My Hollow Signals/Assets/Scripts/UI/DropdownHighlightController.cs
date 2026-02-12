/*******************************************************
 * Author: [Ignacio Lopez]
 * Last Modified: [10/02/2026]
 * Description:
 *    Enhances dropdown menu highlighting when using a controller.
 *    Adds a visual border/outline effect to selected dropdowns and
 *    their list items for better gamepad navigation feedback.
 *******************************************************/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

[RequireComponent(typeof(TMP_Dropdown))]
public class DropdownHighlightController : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerClickHandler, ISubmitHandler
{
    [Header("Highlight Settings")]
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 0f, 1f); // Yellow
    [SerializeField] private float highlightBorderWidth = 3f;
    
    [Header("List Item Highlight Settings")]
    [SerializeField] private Color itemHighlightColor = new Color(1f, 1f, 0f, 1f); // Yellow
    [SerializeField] private float itemHighlightBorderWidth = 2f;
    
    [Header("References (Auto-assigned if null)")]
    [SerializeField] private Image dropdownImage;
    [SerializeField] private Outline outline;
    
    private TMP_Dropdown dropdown;
    private Color originalColor;
    private bool wasHighlighted = false;
    private List<DropdownItemHighlight> itemHighlights = new List<DropdownItemHighlight>();
    private bool isListening = false;
    private bool hasLoggedDropdownSearch = false;
    private bool isDropdownOpen = false;

    private void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        
        Debug.Log($"[DropdownHighlight] Awake called on {gameObject.name}");
        
        // Auto-assign dropdown image if not set
        if (dropdownImage == null)
        {
            dropdownImage = GetComponent<Image>();
        }
        
        // Add or get Outline component
        if (outline == null)
        {
            outline = GetComponent<Outline>();
            if (outline == null)
            {
                outline = gameObject.AddComponent<Outline>();
            }
        }
        
        // Configure outline
        outline.effectColor = highlightColor;
        outline.effectDistance = new Vector2(highlightBorderWidth, highlightBorderWidth);
        outline.enabled = false; // Start disabled
        
        // Store original color
        if (dropdownImage != null)
        {
            originalColor = dropdownImage.color;
        }
        
        Debug.Log($"[DropdownHighlight] Initialized on {gameObject.name}, outline={outline != null}");
    }

    private void OnEnable()
    {
        if (dropdown != null && !isListening)
        {
            dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            isListening = true;
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // Dropdown was clicked with mouse
        Debug.Log($"[DropdownHighlight] Dropdown clicked (mouse), starting monitoring");
        OnDropdownOpened();
    }
    
    public void OnSubmit(BaseEventData eventData)
    {
        // Dropdown was opened with gamepad/keyboard
        Debug.Log($"[DropdownHighlight] Dropdown submitted (gamepad), starting monitoring");
        OnDropdownOpened();
    }
    
    private void OnDropdownOpened()
    {
        isDropdownOpen = true;
        StartCoroutine(WaitAndSetupHighlights());
    }
    
    private System.Collections.IEnumerator WaitAndSetupHighlights()
    {
        // Wait a few frames for the dropdown list to be created
        yield return null;
        yield return null;
        
        // Search for the blocker - TMP_Dropdown creates a full-screen blocker GameObject
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            // Look for all active gameobjects in the canvas
            foreach (Transform child in canvas.transform)
            {
                Debug.Log($"[DropdownHighlight] Checking canvas child: {child.name}, active: {child.gameObject.activeSelf}");
                
                // The dropdown creates objects like "Dropdown List", "Blocker", etc
                if (child.gameObject.activeSelf)
                {
                    // Search this object and its children for toggles (search recursively)
                    Toggle[] toggles = child.GetComponentsInChildren<Toggle>(true);
                    if (toggles.Length > 0)
                    {
                        Debug.Log($"[DropdownHighlight] Found {toggles.Length} toggles in '{child.name}'");
                        SetupItemHighlightsFromToggles(toggles);
                        yield break; // Found and set up, exit coroutine
                    }
                    else
                    {
                        Debug.Log($"[DropdownHighlight] No toggles found in '{child.name}'");
                    }
                }
            }
            
            Debug.LogWarning($"[DropdownHighlight] Could not find any toggles in canvas children!");
        }
    }

    private void OnDisable()
    {
        if (dropdown != null && isListening)
        {
            dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
            isListening = false;
        }
        
        // Ensure highlight is disabled when dropdown is disabled
        if (outline != null)
        {
            outline.enabled = false;
            wasHighlighted = false;
        }
        
        CleanupItemHighlights();
    }

    private void SetupItemHighlights(Transform dropdownList)
    {
        // Find all toggle items in the dropdown list
        Toggle[] toggles = dropdownList.GetComponentsInChildren<Toggle>(true);
        SetupItemHighlightsFromToggles(toggles);
    }
    
    private void SetupItemHighlightsFromToggles(Toggle[] toggles)
    {
        Debug.Log($"[DropdownHighlight] Found {toggles.Length} toggles in dropdown list");
        
        foreach (Toggle toggle in toggles)
        {
            Debug.Log($"[DropdownHighlight] Processing toggle: {toggle.gameObject.name}");
            
            // Check if we already added highlight to this toggle
            DropdownItemHighlight existingHighlight = toggle.GetComponent<DropdownItemHighlight>();
            if (existingHighlight == null)
            {
                DropdownItemHighlight itemHighlight = toggle.gameObject.AddComponent<DropdownItemHighlight>();
                itemHighlight.Initialize(itemHighlightColor, itemHighlightBorderWidth);
                itemHighlights.Add(itemHighlight);
                Debug.Log($"[DropdownHighlight] Added highlight to toggle: {toggle.gameObject.name}");
            }
            else
            {
                Debug.Log($"[DropdownHighlight] Toggle already has highlight: {toggle.gameObject.name}");
            }
        }
    }

    private void CleanupItemHighlights()
    {
        foreach (var itemHighlight in itemHighlights)
        {
            if (itemHighlight != null)
            {
                Destroy(itemHighlight);
            }
        }
        itemHighlights.Clear();
    }

    private void OnDropdownValueChanged(int value)
    {
        // Dropdown selection changed and will close
        Debug.Log($"[DropdownHighlight] Dropdown value changed to {value}, will close soon");
        isDropdownOpen = false;
        // Delay cleanup to allow the dropdown to close properly
        StartCoroutine(DelayedCleanup());
    }
    
    private System.Collections.IEnumerator DelayedCleanup()
    {
        yield return new WaitForSeconds(0.2f);
        if (!isDropdownOpen)
        {
            CleanupItemHighlights();
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        // Enable highlight when selected (gamepad navigation)
        if (outline != null)
        {
            outline.enabled = true;
            wasHighlighted = true;
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        // Disable highlight when deselected
        if (outline != null && wasHighlighted)
        {
            outline.enabled = false;
            wasHighlighted = false;
        }
    }
    
    // Public methods to manually control highlight
    public void SetHighlightColor(Color color)
    {
        highlightColor = color;
        if (outline != null)
        {
            outline.effectColor = color;
        }
    }
    
    public void SetHighlightWidth(float width)
    {
        highlightBorderWidth = width;
        if (outline != null)
        {
            outline.effectDistance = new Vector2(width, width);
        }
    }

    // Inner class to handle individual dropdown item highlighting
    private class DropdownItemHighlight : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        private Color highlightColor;
        private Image backgroundImage;
        private Color originalColor;

        public void Initialize(Color color, float width)
        {
            highlightColor = color;
            
            // Find the background image - toggles usually have "Item Background" child
            Transform bgTransform = transform.Find("Item Background");
            if (bgTransform != null)
            {
                backgroundImage = bgTransform.GetComponent<Image>();
                Debug.Log($"[DropdownItemHighlight] Found Item Background on {gameObject.name}");
            }
            
            // If no background found, try to get image from this object
            if (backgroundImage == null)
            {
                backgroundImage = GetComponent<Image>();
                Debug.Log($"[DropdownItemHighlight] Using Image on {gameObject.name}");
            }
            
            if (backgroundImage != null)
            {
                originalColor = backgroundImage.color;
                Debug.Log($"[DropdownItemHighlight] Initialized color highlight on {backgroundImage.gameObject.name}, original: {originalColor}, highlight: {highlightColor}");
            }
            else
            {
                Debug.LogWarning($"[DropdownItemHighlight] Could not find background image for {gameObject.name}");
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            Debug.Log($"[DropdownItemHighlight] OnSelect called on {gameObject.name}");
            if (backgroundImage != null)
            {
                backgroundImage.color = highlightColor;
                Debug.Log($"[DropdownItemHighlight] Background color changed to {highlightColor} on {gameObject.name}");
            }
        }

        public void OnDeselect(BaseEventData eventData)
        {
            Debug.Log($"[DropdownItemHighlight] OnDeselect called on {gameObject.name}");
            if (backgroundImage != null)
            {
                backgroundImage.color = originalColor;
            }
        }

        private void OnDestroy()
        {
            // Restore original color when destroyed
            if (backgroundImage != null)
            {
                backgroundImage.color = originalColor;
            }
        }
    }
}
