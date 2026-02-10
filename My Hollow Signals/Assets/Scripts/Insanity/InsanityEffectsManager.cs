/*******************************************************
 * Author: [Ignacio Lopez]
 * Last Modified: [26/01/2026]
 * Description:
 *    This script manages the visual and audio effects related to the player's insanity level. It allows you to activate and deactivate an "insane screen" overlay, as well as enable or disable post-processing effects like VHS and CRT distortion. The script also handles hiding the flashlight mesh when the insane screen is active, if the player has a flashlight in their inventory.
 *******************************************************/

using UnityEngine;
using UnityEngine.Video;
using RetroTVFX;

public class InsanityEffectsManager : MonoBehaviour
{
    [Header("Camera Effects")]
    [SerializeField] private VHSPostProcessEffect vhsEffect;
    [SerializeField] private CRTEffect crtEffect;
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("UI")]
    [SerializeField] private GameObject insaneScreen;
    
    [Header("Flashlight Settings")]
    [Tooltip("Reference to the flashlight mesh GameObject to hide when game over screen shows")]
    [SerializeField] private GameObject flashlightMesh;

    private void Awake()
    {
        if (insaneScreen != null)
        {
            insaneScreen.SetActive(false);
        }

        DisableEffects();
    }

    public void ActivateInsaneScreen()
    {
        if (insaneScreen != null)
        {
            insaneScreen.SetActive(true);
        }

        if (flashlightMesh != null && PlayerInventory.Instance != null && PlayerInventory.Instance.HasFlashlight)
        {
            flashlightMesh.SetActive(false);
        }

        EnableEffects();
    }

    public void DeactivateInsaneScreen()
    {
        if (insaneScreen != null)
        {
            insaneScreen.SetActive(false);
        }
        
        if (flashlightMesh != null && PlayerInventory.Instance != null && PlayerInventory.Instance.HasFlashlight)
        {
            flashlightMesh.SetActive(true);
        }

        DisableEffects();
    }

    private void EnableEffects()
    {
        if (vhsEffect != null)
        {
            vhsEffect.enabled = true;
        }

        if (crtEffect != null)
        {
            crtEffect.enabled = true;
        }

        if (videoPlayer != null)
        {
            videoPlayer.enabled = true;
            videoPlayer.Play();
        }
    }

    private void DisableEffects()
    {
        if (vhsEffect != null)
        {
            vhsEffect.enabled = false;
        }

        if (crtEffect != null)
        {
            crtEffect.enabled = false;
        }

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.enabled = false;
        }
    }
}
