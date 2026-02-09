using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Slider))]
public class SliderNavigationHelper : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [Header("Edit Mode Settings")]
    [SerializeField] private GameObject editModeIndicator;
    [SerializeField] private Color editModeColor = new Color(1f, 0.8f, 0.2f, 1f);
    
    private Slider slider;
    private Graphic targetGraphic;
    private Color originalColor;
    private bool isInEditMode = false;
    private Navigation originalNavigation;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        targetGraphic = slider.targetGraphic;
        
        if (targetGraphic != null)
        {
            originalColor = targetGraphic.color;
        }

        originalNavigation = slider.navigation;
        SetupNavigation(false);
    }

    private void Update()
    {
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
        {
            if (Gamepad.current != null)
            {
                if (!isInEditMode && Mathf.Abs(Gamepad.current.leftStick.x.ReadValue()) > 0.1f)
                {
                    EnterEditMode();
                }

                if (isInEditMode && Gamepad.current.buttonEast.wasPressedThisFrame)
                {
                    ExitEditMode();
                }
            }
        }
        else if (isInEditMode)
        {
            ExitEditMode();
        }
    }

    private void EnterEditMode()
    {
        if (isInEditMode) return;
        
        isInEditMode = true;
        SetupNavigation(true);
        
        if (editModeIndicator != null)
        {
            editModeIndicator.SetActive(true);
        }
        
        if (targetGraphic != null)
        {
            targetGraphic.color = editModeColor;
        }
    }

    private void ExitEditMode()
    {
        if (!isInEditMode) return;
        
        isInEditMode = false;
        SetupNavigation(false);
        
        if (editModeIndicator != null)
        {
            editModeIndicator.SetActive(false);
        }
        
        if (targetGraphic != null)
        {
            targetGraphic.color = originalColor;
        }
    }

    private void SetupNavigation(bool editMode)
    {
        Navigation nav = slider.navigation;
        
        if (editMode)
        {
            nav.mode = Navigation.Mode.None;
        }
        else
        {
            nav = originalNavigation;
        }
        
        slider.navigation = nav;
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (editModeIndicator != null)
        {
            editModeIndicator.SetActive(false);
        }
        
        if (targetGraphic != null && !isInEditMode)
        {
            originalColor = targetGraphic.color;
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        ExitEditMode();
    }

    private void OnDisable()
    {
        ExitEditMode();
    }
}
