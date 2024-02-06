using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessController : MonoBehaviour
{

    [SerializeField] private PostProcessVolume postProcessVolume;
    private ColorGrading colorGrading;
    private bool colorGradingOff;

    public int score = 0;
    public int threshold = 1;

    // PlayerPrefs key for saving the colorGrading bool value
    private const string ColorGradingKey = "ColorGradingKey";

    void Start()
    {
        postProcessVolume = GetComponent<PostProcessVolume>();
        postProcessVolume.profile.TryGetSettings(out colorGrading);

        // Load the saved colorGrading value from PlayerPrefs
        colorGradingOff = PlayerPrefs.GetInt(ColorGradingKey, 0) == 1;
        colorGrading.active = colorGradingOff;
    }

    public void MonochromeButton()
    {
         if (score < threshold)
         {
            score++;
            colorGradingOff = !colorGradingOff;
            colorGrading.active = colorGradingOff;

            // Save the updated colorGrading value to PlayerPrefs
            PlayerPrefs.SetInt(ColorGradingKey, colorGradingOff ? 1 : 0);
            PlayerPrefs.Save();
         }
         else
         {
            score--;
         }
    }
}