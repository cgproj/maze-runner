using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using TMPro;

public class MenuController : MonoBehaviour
{
    [Header("Volume Settings")]
    [SerializeField]
    private TMP_Text volumeTextValue = null;
    
    [SerializeField]
    private Slider volumeSlider = null;
    
    [SerializeField]
    private float defaultVolume = 1.0f;

    [Header("Gameplay Settings")]
    [SerializeField]
    private TMP_Text controllerSensitivityTextValue = null;

    [SerializeField]
    private Slider controllerSensitivitySliderValue = null;

    [SerializeField]
    private int defaultSensitivity = 4;

    public int mainControllerSensitivity = 4;

    //Controls

    //Difficulty
    [Header("Difficulty Dropdowns")]
    [SerializeField]
    private TMP_Dropdown difficultyDropdown;

    [SerializeField]
    private int defaultDifficulty = 1; // Default to Normal
    
    private int _difficultyLevel;
    private string[] difficultyOptions = { "Easy", "Normal", "Hard" };

    [Header("Toggle Settings")]
    [SerializeField]
    private Toggle invertYToggle = null;

    [Header("Graphics Settings")]
    [SerializeField]
    private Slider brightnessSlider = null;

    [SerializeField]
    private TMP_Text brightnessTextValue = null;

    [SerializeField]
    private float defaultBrightness = 1;

    [Space(10)]
    [SerializeField]
    private TMP_Dropdown qualityDropdown;

    [SerializeField]
    private Toggle fullScreenMode;

    private int _qualityLevel;
    private bool _isFullScreen;
    private float _brightnessLevel;

    [Header("Confirmation")]
    [SerializeField]
    private GameObject confirmationPrompt = null;

    [Header("Levels To Load")]
    public string _newGameLevel;

    private string levelToLoad;

    [SerializeField]
    private GameObject noSavedGameDialog = null;

    [Header("Resolution Dropdowns")]
    [SerializeField]
    public TMP_Dropdown resolutionDropdown;

    private Resolution[] resolutions;

    public void Start()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string>options = new List<string>();

        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        // Initialize difficulty settings
        difficultyDropdown.ClearOptions();
        difficultyDropdown.AddOptions(new List<string>(difficultyOptions));

        // Load saved difficulty setting or use default
        _difficultyLevel = PlayerPrefs.GetInt("masterDifficulty", defaultDifficulty);
        difficultyDropdown.value = _difficultyLevel;
        difficultyDropdown.RefreshShownValue();

    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void NewGameDialog_YES() 
    {
        SceneManager.LoadScene(_newGameLevel);    
    }
   
    public void LoadGameDialog_YES()
    {
        if (PlayerPrefs.HasKey("SavedLevel"))
        {
            levelToLoad = PlayerPrefs.GetString("SavedLevel");
            SceneManager.LoadScene(levelToLoad);
        }
        else
        {
            noSavedGameDialog.SetActive(true);
        }
    }

    public void ExitButton()
    {
        Application.Quit();
    }

    public void SetVolume(float volume)
    {
        //AudioMixer
        AudioListener.volume = volume;
        volumeTextValue.text = volume.ToString("0.0");
    }

    public void VolumeApply()
    {
        PlayerPrefs.SetFloat("masterVolume",AudioListener.volume);
        // Show prompt
        StartCoroutine(ConfirmationBox());
    }

    public void SetControllerSensitivity(float sensitivity)
    {
        mainControllerSensitivity = Mathf.RoundToInt(sensitivity);
        controllerSensitivityTextValue.text = sensitivity.ToString("0");
    }

    public void SetDifficulty(int difficultyIndex)
    {
        _difficultyLevel = difficultyIndex;
        PlayerPrefs.SetInt("masterDifficulty", _difficultyLevel);
        PlayerPrefs.Save();
    }
    
    public void GameplayApply()
    {
        if (invertYToggle.isOn)
        {
            PlayerPrefs.SetInt("masterInvertY", 1);
        }
        else
        {
            PlayerPrefs.SetInt("masterInvertY", 0);
        }

        PlayerPrefs.SetInt("masterSensitivity", mainControllerSensitivity);
        StartCoroutine (ConfirmationBox());

        PlayerPrefs.SetInt("masterDifficulty", _difficultyLevel);
        StartCoroutine(ConfirmationBox());
    }

    public void SetBrightness(float brightness)
    {
        _brightnessLevel = brightness;
        brightnessTextValue.text = brightness.ToString("0.0");
    }

    public void SetFullScreen(bool isFullScreen)
    {
        _isFullScreen = isFullScreen;
    }

    public void SetQuality(int qualityIndex)
    {
        _qualityLevel = qualityIndex;
    }

    public void GraphicsApply()
    {
        PlayerPrefs.SetFloat("masterBrightness", _brightnessLevel);

        PlayerPrefs.SetFloat("masterQuality", _qualityLevel);
        QualitySettings.SetQualityLevel(_qualityLevel); // make quality and resolution one

        PlayerPrefs.SetInt("masterFullScreen", (_isFullScreen ? 1: 0));
        Screen.fullScreen = _isFullScreen;
        StartCoroutine(ConfirmationBox());
    }

    public void ResetButton(string MenuType)
    {
        if(MenuType == "Graphics")
        {
            brightnessSlider.value = defaultBrightness;
            brightnessTextValue.text = defaultBrightness.ToString("0.0");

            qualityDropdown.value = 1;
            QualitySettings.SetQualityLevel(1);

            fullScreenMode.isOn = false;
            Screen.fullScreen = false;

            Resolution currentResolution = Screen.currentResolution;
            Screen.SetResolution(currentResolution.width, currentResolution.height, Screen.fullScreen);
            resolutionDropdown.value = resolutions.Length;

            GraphicsApply();
        }
        if(MenuType == "Audio")
        {
            AudioListener.volume = defaultVolume;
            volumeSlider.value = defaultVolume;
            volumeTextValue.text = defaultVolume.ToString("0.0");
            
            VolumeApply();
        }
        if (MenuType == "Gameplay")
        {
            controllerSensitivitySliderValue.value = defaultSensitivity;
            mainControllerSensitivity = defaultSensitivity;
            controllerSensitivityTextValue.text = defaultSensitivity.ToString("0");
            invertYToggle.isOn = false;
            difficultyDropdown.value = defaultDifficulty;
            _difficultyLevel = defaultDifficulty;
            GameplayApply();
        }
    }

    public IEnumerator ConfirmationBox()
    {
        confirmationPrompt.SetActive(true);
        yield return new WaitForSeconds(2);
        confirmationPrompt.SetActive(false);
    }
}
