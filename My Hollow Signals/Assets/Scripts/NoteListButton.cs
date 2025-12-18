using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NoteListButton : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && button.interactable)
        {
            button.Select();
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (button != null && button.interactable)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }
}
