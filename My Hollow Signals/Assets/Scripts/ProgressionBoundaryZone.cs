using UnityEngine;

public class ProgressionBoundaryZone : MonoBehaviour
{
    [Header("Safe Zone Settings")]
    [Tooltip("If true, player should stay INSIDE this zone. If false, player should stay OUTSIDE")]
    public bool isInsideSafeZone = true;
    
    [Header("Progression Requirements")]
    [Tooltip("Event flags that must be true to unlock this boundary")]
    public string[] requiredEventFlags;
    
    [Tooltip("Require ALL flags to be true, or just ANY one of them")]
    public bool requireAllFlags = false;
    
    [Header("Sanity Drain Settings")]
    [Tooltip("Sanity drain per second when outside safe zone without required flags")]
    public float sanityDrainRate = 10f;
    
    [Tooltip("Delay before sanity starts draining after entering forbidden zone")]
    public float drainDelay = 1f;
    
    [Tooltip("Optional warning message when player enters forbidden zone")]
    public string warningMessage = "You shouldn't be here yet...";
    
    [Header("Debug")]
    [Tooltip("Show debug messages in console")]
    public bool debugLog = false;
    
    private bool playerInZone = false;
    private bool isDraining = false;
    private float drainTimer = 0f;
    private SanityManager sanityManager;
    
    private void Start()
    {
        FindSanityManager();
        
        BoxCollider collider = GetComponent<BoxCollider>();
        if (collider != null && !collider.isTrigger)
        {
            Debug.LogWarning($"ProgressionBoundaryZone on {gameObject.name}: BoxCollider should be set as Trigger!");
        }
    }
    
    private void FindSanityManager()
    {
        if (sanityManager != null)
            return;
        
        sanityManager = FindObjectOfType<SanityManager>(true);
        
        if (sanityManager == null && debugLog)
        {
            Debug.LogWarning($"ProgressionBoundaryZone on {gameObject.name}: SanityManager not found!");
        }
    }
    
    private void Update()
    {
        if (sanityManager == null)
        {
            FindSanityManager();
            
            if (sanityManager == null)
                return;
        }
        
        bool shouldDrain = ShouldDrainSanity();
        
        if (shouldDrain)
        {
            drainTimer += Time.deltaTime;
            
            if (drainTimer >= drainDelay)
            {
                if (!isDraining)
                {
                    isDraining = true;
                    
                    if (debugLog)
                    {
                        Debug.Log($"Started draining sanity - Player in forbidden zone");
                    }
                }
                
                float drainAmount = sanityDrainRate * Time.deltaTime;
                sanityManager.LowerSanity(drainAmount);
            }
        }
        else
        {
            if (isDraining)
            {
                isDraining = false;
                drainTimer = 0f;
                
                if (debugLog)
                {
                    Debug.Log($"Stopped draining sanity");
                }
            }
            else if (drainTimer > 0f)
            {
                drainTimer = 0f;
            }
        }
    }
    
    private bool ShouldDrainSanity()
    {
        if (HasRequiredFlags())
            return false;
        
        if (isInsideSafeZone)
        {
            return !playerInZone;
        }
        else
        {
            return playerInZone;
        }
    }
    
    private bool HasRequiredFlags()
    {
        if (requiredEventFlags == null || requiredEventFlags.Length == 0)
            return false;
        
        if (GameEventManager.Instance == null)
            return false;
        
        if (requireAllFlags)
        {
            foreach (string flagName in requiredEventFlags)
            {
                if (string.IsNullOrEmpty(flagName))
                    continue;
                
                if (!GameEventManager.Instance.GetEventFlag(flagName))
                {
                    return false;
                }
            }
            return true;
        }
        else
        {
            foreach (string flagName in requiredEventFlags)
            {
                if (string.IsNullOrEmpty(flagName))
                    continue;
                
                if (GameEventManager.Instance.GetEventFlag(flagName))
                {
                    return true;
                }
            }
            return false;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        
        playerInZone = true;
        
        if (debugLog)
        {
            Debug.Log($"Player entered zone: {gameObject.name}");
        }
        
        if (!isInsideSafeZone && !HasRequiredFlags() && !string.IsNullOrEmpty(warningMessage))
        {
            Debug.Log(warningMessage);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        
        playerInZone = false;
        
        if (debugLog)
        {
            Debug.Log($"Player exited zone: {gameObject.name}");
        }
        
        if (isInsideSafeZone && !HasRequiredFlags() && !string.IsNullOrEmpty(warningMessage))
        {
            Debug.Log(warningMessage);
        }
    }
    
    private void OnDrawGizmos()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
            return;
        
        bool hasFlags = Application.isPlaying ? HasRequiredFlags() : false;
        
        if (hasFlags)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        }
        else
        {
            Gizmos.color = isInsideSafeZone ? 
                new Color(0f, 0.5f, 1f, 0.2f) : 
                new Color(1f, 0f, 0f, 0.2f);
        }
        
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.matrix = rotationMatrix;
        Gizmos.DrawCube(boxCollider.center, boxCollider.size);
        
        Gizmos.color = hasFlags ? 
            new Color(0f, 1f, 0f, 0.6f) : 
            (isInsideSafeZone ? new Color(0f, 0.5f, 1f, 0.6f) : new Color(1f, 0f, 0f, 0.6f));
        Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
    }
}
