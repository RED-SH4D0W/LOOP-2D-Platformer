using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class UI_VolumeSliders : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string audioParameter;
    [SerializeField] private float multiplier;

    public void SetupSlider()
    {
        slider.onValueChanged.AddListener(SliderValue);
        slider.minValue = .001f;
        slider.value = PlayerPrefs.GetFloat(audioParameter, slider.value);
    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat(audioParameter, slider.value);
    }

    // Update is called once per frame
    private void SliderValue(float value)
    {
        audioMixer.SetFloat(audioParameter, Mathf.Log10(value) * multiplier);
    }
}
