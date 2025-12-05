/*******************************************************
 * Author: [Bianca Marinica]
 * Last Modified: [21/11/2025]
 * Description:
 *    Represents collectible items (like notes or objects) that players can pick up. Implements the IInteractable interface..
 *******************************************************/

using UnityEngine;
using UnityEngine.InputSystem;

public class Collectible : MonoBehaviour, IInteractable
{
    [SerializeField] private string description = "Pick up";
    [SerializeField] private int value = 1;
    [SerializeField] private GameObject noteUI; // Reference to NoteUI

    [Header("Note Settings")]
    [SerializeField] private string noteTitle = "Note";
    [SerializeField][TextArea(3, 6)] private string noteText = "This is a note..."; // Individual note text

    void Start()
    {

    }

    public string GetDescription()
    {
        return description;
    }

    public void Interact()
    {
        // Show the NoteUI
        if (noteUI != null)
        {
            noteUI.SetActive(true);

            // Tell the NoteUIManager that a note is now active and set the text
            var noteUIManager = noteUI.GetComponent<NoteUIManager>();
            if (noteUIManager != null)
            {
                noteUIManager.SetNoteActive(noteText); // Pass the individual note text
            }

            // Find and use PauseMenuManager to pause the game
            PauseMenuManager pauseManager = FindObjectOfType<PauseMenuManager>();
            if (pauseManager != null)
            {
                pauseManager.PauseGame();

                // Disable the pause menu to prevent conflicts with note UI
                pauseManager.enabled = false;
            }

            // Unlock cursor for UI interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Optionally disable player movement
            var playerController = FindObjectOfType<FirstPersonController>();
            if (playerController != null)
            {
                var playerInput = playerController.GetComponent<PlayerInput>();
                if (playerInput != null)
                    playerInput.enabled = false;
            }
        }

        // Add to inventory if it exists
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.AddItem(value);
            PlayerInventory.Instance.AddNote(noteTitle, noteText);
        }

        // Destroy the collectible object
        Destroy(gameObject);
    }
}
