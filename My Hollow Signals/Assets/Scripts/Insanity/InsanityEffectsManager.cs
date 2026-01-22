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

        EnableEffects();
    }

    public void DeactivateInsaneScreen()
    {
        if (insaneScreen != null)
        {
            insaneScreen.SetActive(false);
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
