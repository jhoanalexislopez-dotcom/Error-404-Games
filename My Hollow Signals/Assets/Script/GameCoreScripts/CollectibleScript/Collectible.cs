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
    [SerializeField][TextArea(3, 6)] private string noteText = "This is a note...";

    [Header("Sanity Settings")]
    [Tooltip("Amount of sanity to lower when this item is collected")]
    [SerializeField] private float sanityLossAmount = 0f;

    void Start()
    {

    }

    public string GetDescription()
    {
        return description;
    }

    public void Interact()
    {
        if (sanityLossAmount > 0f)
        {
            SanityManager sanityManager = FindObjectOfType<SanityManager>(true);
            if (sanityManager != null)
            {
                sanityManager.LowerSanity(sanityLossAmount);
            }
        }

        if (noteUI != null)
        {
            noteUI.SetActive(true);

            var noteUIManager = noteUI.GetComponent<NoteUIManager>();
            if (noteUIManager != null)
            {
                noteUIManager.SetNoteActive(noteText);
            }

            PauseMenuManager pauseManager = FindObjectOfType<PauseMenuManager>();
            if (pauseManager != null)
            {
                pauseManager.PauseGame();

                pauseManager.enabled = false;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            var playerController = FindObjectOfType<FirstPersonController>();
            if (playerController != null)
            {
                var playerInput = playerController.GetComponent<PlayerInput>();
                if (playerInput != null)
                    playerInput.enabled = false;
            }
        }

        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.AddItem(value);
            PlayerInventory.Instance.AddNote(noteTitle, noteText);
        }

        Destroy(gameObject);
    }
}
