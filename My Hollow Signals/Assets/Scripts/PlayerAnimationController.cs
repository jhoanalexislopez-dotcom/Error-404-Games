/*******************************************************
 * Author: [Ignacio Lopez]
 * Last Modified: [02/02/2026]
 * Description:
 *    This script manages the player's animation states based on their movement and running status. It references the Animator component to control the animations and checks the player's movement and running state from the FirstPersonController. The script updates the Animator parameters accordingly to transition between walking and running animations, providing visual feedback that matches the player's actions in the game.
 *******************************************************/


using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private FirstPersonController playerController;
    [SerializeField] private CharacterController characterController;
    
    [Header("Animation Parameters")]
    [SerializeField] private string isWalkingParam = "IsWalking";
    [SerializeField] private string isRunningParam = "IsRunning";
    [SerializeField] private float movementThreshold = 0.1f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    
    private void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("PlayerAnimationController: No Animator found on this GameObject!");
            }
        }
        
        if (playerController == null)
        {
            playerController = transform.parent != null ? transform.parent.GetComponent<FirstPersonController>() : null;
            if (playerController == null)
            {
                Debug.LogError("PlayerAnimationController: No FirstPersonController found on parent GameObject!");
            }
        }
        
        if (characterController == null)
        {
            characterController = transform.parent != null ? transform.parent.GetComponent<CharacterController>() : null;
            if (characterController == null)
            {
                Debug.LogError("PlayerAnimationController: No CharacterController found on parent GameObject!");
            }
        }
        
        if (animator != null && animator.runtimeAnimatorController == null)
        {
            Debug.LogError("PlayerAnimationController: Animator has no controller assigned!");
        }
        
        Debug.Log("PlayerAnimationController initialized successfully!");
    }
    
    private void Update()
    {
        if (animator == null || playerController == null)
            return;
        
        bool isMoving = playerController.IsMoving;
        bool isRunning = playerController.IsRunning;
        
        animator.SetBool(isWalkingParam, isMoving);
        animator.SetBool(isRunningParam, isRunning);
        
        if (showDebugInfo)
        {
            Debug.Log($"IsMoving: {isMoving} | IsRunning: {isRunning}");
        }
    }
}
