/*******************************************************
 * Author: [Ignacio Lopez]
 * Last Modified: [29/01/2026]
 * Description:
 *    This script optimizes terrain rendering by dynamically adjusting LOD settings based on player performance and camera visibility. It includes features such as performance-based LOD adjustments, aggressive frustum culling, and camera angle-based culling to improve frame rates while maintaining visual quality. The script allows for customization of various parameters to fine-tune the optimization process according to the specific needs of the game.
 *******************************************************/


using UnityEngine;

public class TerrainLODOptimizer : MonoBehaviour
{
    [Header("Terrain Reference")]
    [SerializeField] private Terrain terrain;
    
    [Header("Performance Settings")]
    [SerializeField] private int targetFrameRate = 60;
    [SerializeField] private float performanceCheckInterval = 1f;
    
    [Header("Detail Mesh Settings")]
    [SerializeField] private float minDetailDistance = 50f;
    [SerializeField] private float maxDetailDistance = 100f;
    [SerializeField] private float minDetailDensity = 0.2f;
    [SerializeField] private float maxDetailDensity = 0.5f;
    
    [Header("Tree Settings")]
    [SerializeField] private float minTreeDistance = 400f;
    [SerializeField] private float maxTreeDistance = 800f;
    [SerializeField] private float minBillboardDistance = 30f;
    [SerializeField] private float maxBillboardDistance = 60f;
    [SerializeField] private int minMaxFullLODTrees = 5;
    [SerializeField] private int maxMaxFullLODTrees = 15;
    
    [Header("Frustum Culling")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private bool enableAggressiveFrustumCulling = true;
    [SerializeField] private float frustumCullCheckInterval = 0.1f;
    [SerializeField] private float terrainVisibilityMargin = 50f;
    [SerializeField] private bool disableTreesWhenNotVisible = true;
    [SerializeField] private bool disableDetailsWhenNotVisible = true;
    
    [Header("Camera Angle Culling")]
    [SerializeField] private bool enableCameraAngleCulling = true;
    [SerializeField] private float cameraAngleFadeStart = 45f;
    [SerializeField] private float cameraAngleFadeEnd = 75f;
    
    private float currentPerformanceLevel = 1f;
    private float performanceCheckTimer;
    private float frustumCheckTimer;
    private int frameCount;
    private float timeAccumulator;
    private float averageFPS;
    private bool isTerrainVisible;
    private Plane[] frustumPlanes;
    private Bounds terrainBounds;
    
    private void Awake()
    {
        if (terrain == null)
        {
            terrain = GetComponent<Terrain>();
        }
        
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        
        if (terrain != null)
        {
            CalculateTerrainBounds();
        }
        
        frustumPlanes = new Plane[6];
    }
    
    private void Start()
    {
        ApplyTerrainSettings();
    }
    
    private void Update()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null) return;
        }
        
        UpdateFrameRateTracking();
        
        performanceCheckTimer += Time.deltaTime;
        if (performanceCheckTimer >= performanceCheckInterval)
        {
            performanceCheckTimer = 0f;
            AdjustPerformanceLevel();
        }
        
        frustumCheckTimer += Time.deltaTime;
        if (frustumCheckTimer >= frustumCullCheckInterval)
        {
            frustumCheckTimer = 0f;
            
            if (enableAggressiveFrustumCulling)
            {
                UpdateFrustumCulling();
            }
            
            if (enableCameraAngleCulling)
            {
                AdjustForCameraAngle();
            }
            else
            {
                ApplyTerrainSettings();
            }
        }
    }
    
    private void CalculateTerrainBounds()
    {
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPosition = terrain.GetPosition();
        Vector3 terrainSize = terrainData.size;
        
        terrainBounds = new Bounds(
            terrainPosition + terrainSize * 0.5f,
            terrainSize + Vector3.one * terrainVisibilityMargin * 2f
        );
    }
    
    private void UpdateFrustumCulling()
    {
        GeometryUtility.CalculateFrustumPlanes(playerCamera, frustumPlanes);
        
        isTerrainVisible = GeometryUtility.TestPlanesAABB(frustumPlanes, terrainBounds);
        
        if (!isTerrainVisible)
        {
            if (disableTreesWhenNotVisible || disableDetailsWhenNotVisible)
            {
                ApplyVisibilitySettings(false);
            }
        }
        else
        {
            ApplyVisibilitySettings(true);
        }
    }
    
    private void ApplyVisibilitySettings(bool visible)
    {
        if (!visible)
        {
            if (disableTreesWhenNotVisible)
            {
                terrain.treeDistance = 0f;
            }
            
            if (disableDetailsWhenNotVisible)
            {
                terrain.detailObjectDistance = 0f;
            }
        }
    }
    
    private void UpdateFrameRateTracking()
    {
        frameCount++;
        timeAccumulator += Time.unscaledDeltaTime;
        
        if (timeAccumulator >= 0.5f)
        {
            averageFPS = frameCount / timeAccumulator;
            frameCount = 0;
            timeAccumulator = 0f;
        }
    }
    
    private void AdjustPerformanceLevel()
    {
        if (averageFPS < targetFrameRate * 0.8f)
        {
            currentPerformanceLevel = Mathf.Max(0f, currentPerformanceLevel - 0.1f);
        }
        else if (averageFPS > targetFrameRate * 0.95f)
        {
            currentPerformanceLevel = Mathf.Min(1f, currentPerformanceLevel + 0.05f);
        }
        
        currentPerformanceLevel = Mathf.Clamp01(currentPerformanceLevel);
    }
    
    private void AdjustForCameraAngle()
    {
        if (!isTerrainVisible && enableAggressiveFrustumCulling)
        {
            return;
        }
        
        Vector3 cameraForward = playerCamera.transform.forward;
        float angleToHorizon = Vector3.Angle(cameraForward, Vector3.down) - 90f;
        
        float angleFactor = 1f;
        if (angleToHorizon > cameraAngleFadeStart)
        {
            angleFactor = 1f - Mathf.InverseLerp(cameraAngleFadeStart, cameraAngleFadeEnd, angleToHorizon);
        }
        
        float combinedFactor = currentPerformanceLevel * angleFactor;
        ApplyTerrainSettings(combinedFactor);
    }
    
    private void ApplyTerrainSettings(float performanceFactor = -1f)
    {
        if (terrain == null) return;
        
        if (performanceFactor < 0f)
        {
            performanceFactor = currentPerformanceLevel;
        }
        
        if (!isTerrainVisible && enableAggressiveFrustumCulling)
        {
            return;
        }
        
        terrain.detailObjectDistance = Mathf.Lerp(minDetailDistance, maxDetailDistance, performanceFactor);
        terrain.detailObjectDensity = Mathf.Lerp(minDetailDensity, maxDetailDensity, performanceFactor);
        
        terrain.treeDistance = Mathf.Lerp(minTreeDistance, maxTreeDistance, performanceFactor);
        terrain.treeBillboardDistance = Mathf.Lerp(minBillboardDistance, maxBillboardDistance, performanceFactor);
        terrain.treeMaximumFullLODCount = Mathf.RoundToInt(Mathf.Lerp(minMaxFullLODTrees, maxMaxFullLODTrees, performanceFactor));
    }
    
    public void SetQualityPreset(QualityPreset preset)
    {
        switch (preset)
        {
            case QualityPreset.Low:
                currentPerformanceLevel = 0.3f;
                break;
            case QualityPreset.Medium:
                currentPerformanceLevel = 0.6f;
                break;
            case QualityPreset.High:
                currentPerformanceLevel = 1f;
                break;
        }
        
        ApplyTerrainSettings();
    }
    
    public enum QualityPreset
    {
        Low,
        Medium,
        High
    }
}
