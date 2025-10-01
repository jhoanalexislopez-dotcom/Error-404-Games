using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class ButtonPressDown : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("Press Down Settings")]
    [Tooltip("Should the button trigger on press down instead of release")]
    public bool triggerOnPressDown = true;

    [Tooltip("Should the normal button click be disabled")]
    public bool disableNormalClick = true;

    [Header("Visual Feedback")]
    [Tooltip("Should the button show pressed state while held down")]
    public bool showPressedState = true;

    private Button button;
    private bool isPressed = false;
    private bool hasTriggeredOnPress = false;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable || eventData.button != PointerEventData.InputButton.Left)
            return;

        isPressed = true;
        hasTriggeredOnPress = false;

        if (showPressedState)
        {
            button.targetGraphic?.CrossFadeColor(button.colors.pressedColor, button.colors.fadeDuration, false, true);
        }

        if (triggerOnPressDown)
        {
            button.onClick.Invoke();
            hasTriggeredOnPress = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPressed || eventData.button != PointerEventData.InputButton.Left)
            return;

        isPressed = false;

        if (showPressedState && button.interactable)
        {
            button.targetGraphic?.CrossFadeColor(button.colors.normalColor, button.colors.fadeDuration, false, true);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // If we already triggered on press down and want to disable normal click,
        // prevent the click event from executing again
        if (disableNormalClick && hasTriggeredOnPress)
        {
            eventData.Use();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isPressed && showPressedState && button.interactable)
        {
            button.targetGraphic?.CrossFadeColor(button.colors.normalColor, button.colors.fadeDuration, false, true);
        }
        isPressed = false;
        hasTriggeredOnPress = false;
    }

    private void OnDisable()
    {
        isPressed = false;
        hasTriggeredOnPress = false;
    }

    public void SetTriggerOnPressDown(bool enabled)
    {
        triggerOnPressDown = enabled;
    }
}
