using UnityEngine;

public class InventoryItem3D : MonoBehaviour
{
    public NoteData noteData;
    public bool isSelected = false;

    [Header("Visual Settings")]
    public Material normalMaterial;
    public Material selectedMaterial;

    [Header("Rotation Settings")]
    public float autoRotateSpeed = 20f;
    public bool enableAutoRotate = true;

    private Renderer itemRenderer;
    private Vector3 targetScale;
    private Vector3 normalScale = Vector3.one;
    private Vector3 selectedScale = Vector3.one * 1.2f;
    private Quaternion manualRotation = Quaternion.identity;
    private bool isDragging = false;

    private void Awake()
    {
        itemRenderer = GetComponentInChildren<Renderer>();
        targetScale = normalScale;
    }

    private void Update()
    {
        if (enableAutoRotate && !isSelected && !isDragging)
        {
            transform.Rotate(Vector3.up, autoRotateSpeed * Time.unscaledDeltaTime);
        }

        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * 5f);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        targetScale = selected ? selectedScale : normalScale;

        if (itemRenderer != null)
        {
            itemRenderer.material = selected ? selectedMaterial : normalMaterial;
        }
    }

    public void OnMouseEnter()
    {
        if (!isSelected)
        {
            targetScale = normalScale * 1.1f;
        }
    }

    public void OnMouseExit()
    {
        if (!isSelected)
        {
            targetScale = normalScale;
        }
    }

    public void StartDragging()
    {
        isDragging = true;
    }

    public void StopDragging()
    {
        isDragging = false;
    }

    public void ApplyRotation(Vector2 mouseDelta, float sensitivity)
    {
        float rotationX = mouseDelta.y * sensitivity;
        float rotationY = -mouseDelta.x * sensitivity;

        transform.Rotate(Camera.main.transform.up, rotationY, Space.World);
        transform.Rotate(Camera.main.transform.right, rotationX, Space.World);
    }
}
