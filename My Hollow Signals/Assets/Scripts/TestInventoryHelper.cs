using UnityEngine;

public class TestInventoryHelper : MonoBehaviour
{
    void Start()
    {
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.AddNote("Chapter 1: The Beginning", 
                "I've seen things you wouldn't understand. They watch me, they guide me, they show me the truth.");
            
            PlayerInventory.Instance.AddNote("Chapter 2: Revelations", 
                "The whispers grow louder each night. They're not just in my head anymore - they're everywhere.");
            
            PlayerInventory.Instance.AddNote("Chapter 3: The Truth", 
                "I understand now. Everything makes sense. The entity that dwells in the shadows is ready.");
                
            Debug.Log("✓ Test notes added! Press Tab to open 3D inventory.");
        }
        else
        {
            Debug.LogError("PlayerInventory.Instance is null!");
        }
    }
    
    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Tab))
    //    {
    //        Debug.Log("Tab key pressed detected!");
    //    }
    //}
}
