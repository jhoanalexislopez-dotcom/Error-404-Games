/*******************************************************
 * Author: [Ignacio Lopez]
 * Last Modified: [10/02/2026]
 * Description:
 *    This script loads the gamma setting saved in PlayerPrefs and applies it to all PostProcessVolume ColorGrading effects in the scene. It should be attached to a GameObject in gameplay scenes to ensure the player's gamma preference is applied when the scene loads. This script finds all PostProcessVolumes to handle multiple volumes in the same scene.
 *******************************************************/

using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class GammaLoader : MonoBehaviour
{
    private const string GAMMA_KEY = "GameGamma";
    private const float DEFAULT_GAMMA = 1f;

    void Start()
    {
        ApplyGammaToAllVolumes();
    }

    private void ApplyGammaToAllVolumes()
    {
        PostProcessVolume[] volumes = FindObjectsOfType<PostProcessVolume>();

        if (volumes.Length == 0)
        {
            Debug.LogWarning("GammaLoader: No PostProcessVolume found in the scene!");
            return;
        }

        float savedGamma = PlayerPrefs.GetFloat(GAMMA_KEY, DEFAULT_GAMMA);
        int successCount = 0;

        foreach (PostProcessVolume volume in volumes)
        {
            if (volume.profile == null)
            {
                continue;
            }

            ColorGrading colorGrading;
            if (volume.profile.TryGetSettings(out colorGrading))
            {
                if (!colorGrading.enabled.value)
                {
                    colorGrading.enabled.Override(true);
                }

                if (!colorGrading.postExposure.overrideState)
                {
                    colorGrading.postExposure.overrideState = true;
                }

                colorGrading.postExposure.value = savedGamma - 1f;
                successCount++;
            }
        }

        if (successCount == 0)
        {
            Debug.LogWarning("GammaLoader: No ColorGrading settings found in any PostProcessVolume profiles!");
        }
    }

    public void RefreshGamma()
    {
        ApplyGammaToAllVolumes();
    }
}
