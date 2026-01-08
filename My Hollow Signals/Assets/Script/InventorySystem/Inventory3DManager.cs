using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Inventory3DManager : MonoBehaviour
{
    [Header("3D Display Setup")]
    [SerializeField] private Transform inventory3DContainer;
    [SerializeField] private Camera inventoryCamera;
    [SerializeField] private Light inventoryLight;
    [SerializeField] private GameObject item3DPrefab;

    [Header("Materials")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material selectedMaterial;

    [Header("Layout Settings")]
    [SerializeField] private float itemSpacing = 2f;
    [SerializeField] private float arcRadius = 4f;
    [SerializeField] private float arcAngle = 60f;

    [Header("Interaction Settings")]
    [SerializeField] private float rotationSensitivity = 0.5f;

    private List<InventoryItem3D> spawnedItems = new List<InventoryItem3D>();
    private InventoryItem3D selectedItem;
    private InventoryItem3D hoveredItem;
    private bool isDraggingItem = false;
    private Vector2 lastMousePosition;

    public void SpawnInventoryItems(List<NoteData> notes)
    {
        ClearInventoryItems();

        if (notes == null || notes.Count == 0)
            return;

        int itemCount = notes.Count;
        float angleStep = itemCount > 1 ? arcAngle / (itemCount - 1) : 0;
        float startAngle = -arcAngle / 2f;

        for (int i = 0; i < itemCount; i++)
        {
            float angle = startAngle + (angleStep * i);
            Vector3 position = CalculateArcPosition(angle, arcRadius);

            GameObject itemObj = Instantiate(item3DPrefab, inventory3DContainer);
            itemObj.transform.localPosition = position;
            itemObj.transform.localRotation = Quaternion.Euler(0, -angle, 0);
            itemObj.layer = LayerMask.NameToLayer("InventoryItems");

            InventoryItem3D item = itemObj.GetComponent<InventoryItem3D>();
            if (item == null)
                item = itemObj.AddComponent<InventoryItem3D>();

            item.noteData = notes[i];
            item.normalMaterial = normalMaterial;
            item.selectedMaterial = selectedMaterial;

            spawnedItems.Add(item);
        }

        if (spawnedItems.Count > 0)
        {
            SelectItem(spawnedItems[spawnedItems.Count - 1]);
        }
    }

    public void ClearInventoryItems()
    {
        foreach (var item in spawnedItems)
        {
            if (item != null)
                Destroy(item.gameObject);
        }
        spawnedItems.Clear();
        selectedItem = null;
        hoveredItem = null;
    }

    public void EnableInventoryView(bool enable)
    {
        if (inventoryCamera != null)
            inventoryCamera.enabled = enable;

        if (inventoryLight != null)
            inventoryLight.enabled = enable;

        inventory3DContainer.gameObject.SetActive(enable);
    }

    private void Update()
    {
        HandleMouseInteraction();
    }

    private void HandleMouseInteraction()
    {
        if (!inventoryCamera.enabled)
            return;

        Ray ray = inventoryCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("InventoryItems")))
            {
                InventoryItem3D clickedItem = hit.collider.GetComponent<InventoryItem3D>();
                if (clickedItem != null)
                {
                    SelectItem(clickedItem);
                    isDraggingItem = true;
                    clickedItem.StartDragging();
                    lastMousePosition = Mouse.current.position.ReadValue();
                }
            }
        }

        if (Mouse.current.leftButton.isPressed && isDraggingItem && selectedItem != null)
        {
            Vector2 currentMousePosition = Mouse.current.position.ReadValue();
            Vector2 mouseDelta = currentMousePosition - lastMousePosition;

            selectedItem.ApplyRotation(mouseDelta, rotationSensitivity);
            lastMousePosition = currentMousePosition;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (isDraggingItem && selectedItem != null)
            {
                selectedItem.StopDragging();
            }
            isDraggingItem = false;
        }
    }

    private void SelectItem(InventoryItem3D item)
    {
        if (selectedItem != null)
        {
            selectedItem.SetSelected(false);
        }

        selectedItem = item;
        selectedItem.SetSelected(true);
    }

    private Vector3 CalculateArcPosition(float angle, float radius)
    {
        float radian = angle * Mathf.Deg2Rad;
        float x = Mathf.Sin(radian) * radius;
        float z = Mathf.Cos(radian) * radius;
        return new Vector3(x, 0, z);
    }

    public NoteData GetSelectedNoteData()
    {
        return selectedItem?.noteData;
    }
}
