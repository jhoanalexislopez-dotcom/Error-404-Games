/*******************************************************
 * Author: [Ignacio Lopez]
 * Last Modified: [10/02/2026]
 * Description:
 *    Generates capsule colliders for Unity terrain trees since
 *    terrain trees don't have automatic collision support.
 *    Attach this to your Terrain GameObject.
 *******************************************************/

using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class TerrainTreeCollider : MonoBehaviour
{
    [Header("Collider Settings")]
    [Tooltip("Radius of the tree collider capsule")]
    [SerializeField] private float colliderRadius = 0.5f;
    
    [Tooltip("Height of the tree collider capsule")]
    [SerializeField] private float colliderHeight = 5f;
    
    [Tooltip("Y offset for the collider (to align with trunk)")]
    [SerializeField] private float colliderYOffset = 2.5f;
    
    [Tooltip("Layer to assign to tree colliders")]
    [SerializeField] private string colliderLayer = "Default";
    
    [Header("Performance")]
    [Tooltip("Maximum number of colliders to create (0 = unlimited)")]
    [SerializeField] private int maxColliders = 0;
    
    [Header("Debug")]
    [Tooltip("Show collider gizmos in Scene view")]
    [SerializeField] private bool showGizmos = true;
    
    private Terrain terrain;
    private TerrainData terrainData;
    private GameObject collidersParent;
    
    void Start()
    {
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;
        
        GenerateTreeColliders();
    }
    
    [ContextMenu("Generate Tree Colliders")]
    public void GenerateTreeColliders()
    {
        if (terrain == null)
            terrain = GetComponent<Terrain>();
            
        if (terrainData == null)
            terrainData = terrain.terrainData;
        
        // Clean up old colliders
        if (collidersParent != null)
            DestroyImmediate(collidersParent);
        
        // Create parent object for organization
        collidersParent = new GameObject("TreeColliders");
        collidersParent.transform.parent = transform;
        collidersParent.transform.localPosition = Vector3.zero;
        
        TreeInstance[] trees = terrainData.treeInstances;
        int colliderCount = 0;
        int maxCount = maxColliders > 0 ? maxColliders : trees.Length;
        
        Debug.Log($"[TerrainTreeCollider] Generating colliders for {Mathf.Min(trees.Length, maxCount)} trees...");
        
        for (int i = 0; i < trees.Length && colliderCount < maxCount; i++)
        {
            TreeInstance tree = trees[i];
            
            // Convert normalized position to world position
            Vector3 treePosition = Vector3.Scale(tree.position, terrainData.size) + terrain.transform.position;
            
            // Create collider GameObject
            GameObject colliderObj = new GameObject($"TreeCollider_{i}");
            colliderObj.transform.parent = collidersParent.transform;
            colliderObj.transform.position = treePosition + Vector3.up * colliderYOffset;
            
            // Add capsule collider
            CapsuleCollider capsule = colliderObj.AddComponent<CapsuleCollider>();
            capsule.radius = colliderRadius;
            capsule.height = colliderHeight;
            capsule.direction = 1; // Y-axis
            
            // Set layer
            int layer = LayerMask.NameToLayer(colliderLayer);
            if (layer != -1)
                colliderObj.layer = layer;
            
            colliderCount++;
        }
        
        Debug.Log($"[TerrainTreeCollider] Created {colliderCount} tree colliders!");
    }
    
    [ContextMenu("Clear Tree Colliders")]
    public void ClearTreeColliders()
    {
        if (collidersParent != null)
        {
            DestroyImmediate(collidersParent);
            Debug.Log("[TerrainTreeCollider] Cleared all tree colliders!");
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showGizmos || terrainData == null)
            return;
        
        Gizmos.color = Color.green;
        TreeInstance[] trees = terrainData.treeInstances;
        
        // Only draw first 100 trees to avoid performance issues
        int drawCount = Mathf.Min(trees.Length, 100);
        
        for (int i = 0; i < drawCount; i++)
        {
            TreeInstance tree = trees[i];
            Vector3 treePosition = Vector3.Scale(tree.position, terrainData.size) + terrain.transform.position;
            treePosition.y += colliderYOffset;
            
            // Draw wire capsule representation
            Gizmos.DrawWireSphere(treePosition + Vector3.up * (colliderHeight * 0.5f - colliderRadius), colliderRadius);
            Gizmos.DrawWireSphere(treePosition - Vector3.up * (colliderHeight * 0.5f - colliderRadius), colliderRadius);
        }
    }
}
