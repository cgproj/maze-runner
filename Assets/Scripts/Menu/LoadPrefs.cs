using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadPrefs : MonoBehaviour
{
    [Header("General Setting")]
    [SerializeField]
    private bool canUse = false;
    [SerializeField]
    private MenuController menuController;

    [Header("Volume Setting")]
    [SerializeField]
    private TMP_Text volumeTextValue = null;
    [SerializeField]
    private Slider volumeSlider = null;

    [Header("Brightness Setting")]
    [SerializeField]
    private Slider brightnessSlider = null;
    [SerializeField]
    private TMP_Text brightnessTextValue = null;

    [Header("Quality Level Setting")]
    [SerializeField]
    private TMP_Dropdown qualityDropdown;

    [Header("Fullscreen Setting")]
    [SerializeField]
    private Toggle fullScreenMode;

    [Header("Sensitivity Setting")]
    [SerializeField]
    private TMP_Text controllerSensitivityTextValue = null;
    [SerializeField]
    private Slider controllerSensitivitySliderValue = null;

    [Header("Invert Y Setting")]
    [SerializeField]
    private Toggle invertYToggle = null;

    [Header("Difficulty Settings")]
    [SerializeField]
    private TMP_Dropdown difficultyDropdown;


    private void Awake()
    {
        if (canUse)
        {
            if (PlayerPrefs.HasKey("masterVolume"))
            {
                float localVolume = PlayerPrefs.GetFloat("masterVolume");

                volumeTextValue.text = localVolume.ToString("0.0");
                volumeSlider.value = localVolume;
                AudioListener.volume = localVolume;
            }
            else
            {
                menuController.ResetButton("Audio");
            }
            if (PlayerPrefs.HasKey("masterQuality"))
            {
                int localQuality = PlayerPrefs.GetInt("masterQuality");
                qualityDropdown.value = localQuality;
                QualitySettings.SetQualityLevel(localQuality);
            }
            if (PlayerPrefs.HasKey("masterFullScreen"))
            {
                int localFullScreen = PlayerPrefs.GetInt("masterFullScreen");
                if (localFullScreen == 1)
                {
                    Screen.fullScreen = true;
                    fullScreenMode.isOn = true;
                }
                else
                {
                    Screen.fullScreen = false;
                    fullScreenMode.isOn = false;
                }
            }
            if (PlayerPrefs.HasKey("masterBrightness"))
            {
                float localBrightness = PlayerPrefs.GetFloat("masterBrightness");
                brightnessSlider.value = localBrightness;
                brightnessTextValue.text = localBrightness.ToString("0.0");
            }
            if (PlayerPrefs.HasKey("masterSensitivity"))
            {
                float localSensitivity = PlayerPrefs.GetFloat("masterSensitivity");
                controllerSensitivitySliderValue.value = localSensitivity;
                controllerSensitivityTextValue.text = localSensitivity.ToString("0");
                menuController.mainControllerSensitivity = Mathf.RoundToInt(localSensitivity);
            }
            if (PlayerPrefs.HasKey("masterInvertY"))
            {
                if (PlayerPrefs.GetInt("masterInvertY") == 1)
                {
                    invertYToggle.isOn = true;
                }
                else
                {
                    invertYToggle.isOn = false;
                }
            }
            if (PlayerPrefs.HasKey("masterDifficulty"))
            {
                int localDifficulty = PlayerPrefs.GetInt("masterDifficulty");
                difficultyDropdown.value = localDifficulty;
                menuController.SetDifficulty(localDifficulty);
            }
            else
            {
                menuController.ResetButton("Gameplay");
            }
        }
    }
}
