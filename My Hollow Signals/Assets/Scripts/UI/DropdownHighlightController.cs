/*******************************************************
 * Author: [Ignacio Lopez]
 * Last Modified: [10/02/2026]
 * Description:
 *    Enhances dropdown menu highlighting when using a controller.
 *    Adds a visual border/outline effect to selected dropdowns for
 *    better gamepad navigation feedback.
 *******************************************************/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(TMP_Dropdown))]
public class DropdownHighlightController : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [Header("Highlight Settings")]
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 0f, 1f); // Yellow
    [SerializeField] private float highlightBorderWidth = 3f;
    
    [Header("References (Auto-assigned if null)")]
    [SerializeField] private Image dropdownImage;
    [SerializeField] private Outline outline;
    
    private TMP_Dropdown dropdown;
    private Color originalColor;
    private bool wasHighlighted = false;

    private void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        
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

    private void OnDisable()
    {
        // Ensure highlight is disabled when dropdown is disabled
        if (outline != null)
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
}
