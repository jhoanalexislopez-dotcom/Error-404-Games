/*******************************************************
 * Author: [Ignacio Lopez]
 * Last Modified: [18/01/2026]
 * Description:
 *    This script allows UI elements to be dragged around the screen. It implements the IBeginDragHandler and IDragHandler interfaces to handle the dragging behavior. When the user starts dragging, it calculates the offset between the mouse position and the UI element's position, and during the drag, it updates the UI element's position based on the mouse movement while maintaining that offset.
 *******************************************************/

using UnityEngine;
using UnityEngine.EventSystems;

public class UIDrag : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    private Vector2 offset;

    public void OnBeginDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)transform.parent,
            eventData.position,
            eventData.pressEventCamera,
            out var localPoint
        );

        offset = (Vector2)transform.localPosition - localPoint;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)transform.parent,
            eventData.position,
            eventData.pressEventCamera,
            out var localPoint
        );

        transform.localPosition = localPoint + offset;
    }
}
