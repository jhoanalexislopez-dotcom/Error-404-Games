using UnityEngine;
using UnityEngine.EventSystems;

public class Interactive3DNote : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IDragHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("Visual Settings")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material selectedMaterial;
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float selectedScale = 1.3f;
    [SerializeField] private float scaleSpeed = 5f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float autoRotateSpeed = 20f;

    private NoteData noteData;
    private Renderer noteRenderer;
    private Vector3 baseScale;
    private Vector3 targetScale;
    private bool isSelected = false;
    private bool isHovered = false;
    private bool isDragging = false;
    private Vector2 lastMousePosition;
    private Quaternion baseRotation;
    private int noteIndex;

    private Inventory3DManager inventoryManager;

    public void Initialize(NoteData data, int index, Material normal, Material selected, Inventory3DManager manager)
    {
        noteData = data;
        noteIndex = index;
        normalMaterial = normal;
        selectedMaterial = selected;
        inventoryManager = manager;

        noteRenderer = GetComponentInChildren<Renderer>();
        if (noteRenderer != null && normalMaterial != null)
        {
            noteRenderer.material = normalMaterial;
        }

        baseScale = transform.localScale;
        targetScale = baseScale;
        baseRotation = transform.rotation;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * scaleSpeed);

        if (!isDragging && !isSelected)
        {
            transform.Rotate(Vector3.up, autoRotateSpeed * Time.unscaledDeltaTime, Space.World);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSelected)
        {
            isHovered = true;
            targetScale = baseScale * hoverScale;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        if (!isSelected)
        {
            targetScale = baseScale;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        lastMousePosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        isDragging = true;
        Vector2 delta = eventData.position - lastMousePosition;
        
        float rotationX = delta.y * rotationSpeed * Time.unscaledDeltaTime;
        float rotationY = -delta.x * rotationSpeed * Time.unscaledDeltaTime;
        
        transform.Rotate(Camera.main.transform.up, rotationY, Space.World);
        transform.Rotate(Camera.main.transform.right, rotationX, Space.World);
        
        lastMousePosition = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isDragging && inventoryManager != null)
        {
            inventoryManager.SelectNote(noteIndex);
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        if (isSelected)
        {
            targetScale = baseScale * selectedScale;
            if (noteRenderer != null && selectedMaterial != null)
            {
                noteRenderer.material = selectedMaterial;
            }
        }
        else
        {
            targetScale = baseScale;
            if (noteRenderer != null && normalMaterial != null)
            {
                noteRenderer.material = normalMaterial;
            }
            transform.rotation = baseRotation;
        }
    }

    public NoteData GetNoteData()
    {
        return noteData;
    }
}
