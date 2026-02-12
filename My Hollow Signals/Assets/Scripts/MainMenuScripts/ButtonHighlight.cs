/*******************************************************
 * Author: [Alejandro Vila]
 * Last Modified: [21/11/2025]
 * Description:
 *    Adds visual highlight effects to menu buttons when hovered over
 *******************************************************/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class ButtonHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Highlight Settings")]
    [Tooltip("Prefab to instantiate when hovering over the button")]
    public GameObject highlightPrefab;

    [Header("Behavior")]
    [Tooltip("Should show highlight when selected via gamepad/keyboard")]
    public bool showHighlightOnSelect = true;

    [Tooltip("Automatically destroy highlight when parent becomes inactive")]
    public bool destroyOnParentDisabled = true;

    private Button button;
    private GameObject currentHighlight;
    private bool isHovered = false;
    private Transform parentToWatch;

    private static ButtonHighlight currentActiveHighlight;

    private void Awake()
    {
        button = GetComponent<Button>();
        parentToWatch = transform.parent;
    }

    private void Start()
    {
        if (button != null && showHighlightOnSelect)
        {
            EventTrigger eventTrigger = GetComponent<EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = gameObject.AddComponent<EventTrigger>();
            }

            EventTrigger.Entry selectEntry = new EventTrigger.Entry();
            selectEntry.eventID = EventTriggerType.Select;
            selectEntry.callback.AddListener((data) => { OnSelect(); });
            eventTrigger.triggers.Add(selectEntry);

            EventTrigger.Entry deselectEntry = new EventTrigger.Entry();
            deselectEntry.eventID = EventTriggerType.Deselect;
            deselectEntry.callback.AddListener((data) => { OnDeselect(); });
            eventTrigger.triggers.Add(deselectEntry);
        }
    }

    private void Update()
    {
        if (destroyOnParentDisabled && currentHighlight != null)
        {
            if (CheckIfAnyParentDisabled())
            {
                DestroyHighlight();
            }
        }
    }

    private bool CheckIfAnyParentDisabled()
    {
        Transform current = transform;
        while (current != null)
        {
            if (!current.gameObject.activeInHierarchy)
            {
                return true;
            }
            current = current.parent;
        }
        return false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isHovered && button.interactable)
        {
            isHovered = true;
            ClearOtherHighlights();
            ShowHighlight();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        HideHighlight();
    }

    public void OnSelect()
    {
        if (showHighlightOnSelect && !isHovered && button.interactable)
        {
            ClearOtherHighlights();
            ShowHighlight();
        }
    }

    public void OnDeselect()
    {
        if (!isHovered)
        {
            HideHighlight();
        }
    }

    private void ShowHighlight()
    {
        if (highlightPrefab == null)
            return;

        if (currentHighlight != null)
            return;

        currentHighlight = Instantiate(highlightPrefab, transform);
        currentActiveHighlight = this;
    }

    private void ClearOtherHighlights()
    {
        if (currentActiveHighlight != null && currentActiveHighlight != this)
        {
            currentActiveHighlight.ForceHideHighlight();
        }
    }

    private void ForceHideHighlight()
    {
        isHovered = false;
        HideHighlight();
    }

    private void HideHighlight()
    {
        DestroyHighlight();
    }

    private void DestroyHighlight()
    {
        if (currentHighlight != null)
        {
            Destroy(currentHighlight);
            currentHighlight = null;
        }
        else
        {
            Transform existingHighlight = transform.Find(highlightPrefab != null ? highlightPrefab.name + "(Clone)" : "HighlightPrefab(Clone)");
            if (existingHighlight != null)
            {
                Destroy(existingHighlight.gameObject);
            }
        }

        if (currentActiveHighlight == this)
        {
            currentActiveHighlight = null;
        }
    }

    private void OnEnable()
    {
        DestroyHighlight();
        isHovered = false;
    }

    private void OnDisable()
    {
        DestroyHighlight();
        isHovered = false;
    }

    private void OnDestroy()
    {
        DestroyHighlight();
    }

    public void TriggerHighlight()
    {
        ShowHighlight();
    }

    public void RemoveHighlight()
    {
        HideHighlight();
    }
}
