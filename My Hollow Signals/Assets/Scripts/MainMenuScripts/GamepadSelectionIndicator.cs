using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GamepadSelectionIndicator : MonoBehaviour
{
    [Header("Selection Indicator")]
    [SerializeField] private RectTransform selectionFrame;
    [SerializeField] private Color selectionColor = new Color(1f, 0.8f, 0f, 1f);
    [SerializeField] private float padding = 10f;
    [SerializeField] private float animationSpeed = 10f;
    
    [Header("Input Detection")]
    [SerializeField] private bool showOnlyWithGamepad = true;
    
    private RectTransform currentTarget;
    private Vector2 targetPosition;
    private Vector2 targetSize;
    private bool isGamepadActive;
    private Image[] borderImages;

    private void Start()
    {
        if (selectionFrame != null)
        {
            borderImages = selectionFrame.GetComponentsInChildren<Image>();
            
            foreach (Image img in borderImages)
            {
                img.color = selectionColor;
            }
            
            selectionFrame.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        CheckGamepadInput();
        UpdateSelectionIndicator();
    }

    private void CheckGamepadInput()
    {
        if (Gamepad.current != null)
        {
            isGamepadActive = true;
        }
    }

    private void UpdateSelectionIndicator()
    {
        if (selectionFrame == null || EventSystem.current == null)
            return;

        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        
        bool shouldShow = selectedObject != null && (!showOnlyWithGamepad || isGamepadActive);
        
        if (shouldShow)
        {
            RectTransform selectedRect = selectedObject.GetComponent<RectTransform>();
            
            if (selectedRect != null)
            {
                if (selectedRect != currentTarget)
                {
                    currentTarget = selectedRect;
                    CalculateTargetTransform();
                }
                
                if (!selectionFrame.gameObject.activeSelf)
                {
                    selectionFrame.gameObject.SetActive(true);
                    selectionFrame.anchoredPosition = targetPosition;
                    selectionFrame.sizeDelta = targetSize;
                }
                
                selectionFrame.anchoredPosition = Vector2.Lerp(
                    selectionFrame.anchoredPosition, 
                    targetPosition, 
                    Time.unscaledDeltaTime * animationSpeed
                );
                
                selectionFrame.sizeDelta = Vector2.Lerp(
                    selectionFrame.sizeDelta, 
                    targetSize, 
                    Time.unscaledDeltaTime * animationSpeed
                );
            }
            else
            {
                if (selectionFrame.gameObject.activeSelf)
                {
                    selectionFrame.gameObject.SetActive(false);
                }
                currentTarget = null;
            }
        }
        else
        {
            if (selectionFrame.gameObject.activeSelf)
            {
                selectionFrame.gameObject.SetActive(false);
            }
            currentTarget = null;
        }
    }

    private void CalculateTargetTransform()
    {
        if (currentTarget == null || selectionFrame == null)
            return;

        RectTransform parent = selectionFrame.parent as RectTransform;
        if (parent == null)
            return;

        Vector3[] targetCorners = new Vector3[4];
        currentTarget.GetWorldCorners(targetCorners);

        Vector3[] parentCorners = new Vector3[4];
        parent.GetWorldCorners(parentCorners);

        Vector2 targetMin = targetCorners[0];
        Vector2 targetMax = targetCorners[2];
        Vector2 parentMin = parentCorners[0];

        Vector2 targetCenter = (targetMin + targetMax) / 2f;
        Vector2 targetWorldSize = targetMax - targetMin;

        Vector2 localCenter = targetCenter - (Vector2)parentMin;

        targetPosition = localCenter;
        targetSize = targetWorldSize + new Vector2(padding * 2f, padding * 2f);
    }

    public void SetSelectionColor(Color color)
    {
        selectionColor = color;
        
        if (borderImages != null)
        {
            foreach (Image img in borderImages)
            {
                if (img != null)
                {
                    img.color = color;
                }
            }
        }
    }
}
