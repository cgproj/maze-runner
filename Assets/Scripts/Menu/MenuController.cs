using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using TMPro;
using Unity.Jobs;
using Unity.Mathematics;
using static GenerateMazeJob;
using static UnityEngine.InputManagerEntry;

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

    // Background Music
    [Header("Background Music")]
    [SerializeField]
    private AudioSource backgroundMusicSource; // Reference to the AudioSource component

    [SerializeField]
    private AudioClip backgroundMusicClip; // Assign your background music clip in the Unity Editor
    
    [Header("Button Click Sound")]
    [SerializeField]
    private AudioSource buttonClickSource; // Reference to the AudioSource component for button clicks

    [SerializeField]
    private AudioClip buttonClickClip; // Assign your button click sound clip in the Unity Editor

    // Buttons
    public Button[] menuButtons;

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

        // Ensure the background music source is assigned
        if (backgroundMusicSource == null)
        {
            Debug.LogError("Background Music Source is not assigned in the MenuController.");
        }
        else
        {
            // Assign the background music clip (you can set this in the Unity Editor as well)
            backgroundMusicSource.clip = backgroundMusicClip;
            backgroundMusicSource.loop = true; // Loop the background music
        }

        if (buttonClickSource == null)
        {
            Debug.LogError("Button Click Source is not assigned in the MenuController.");
        }
        // Find and assign button click handlers
        // Add null check for menuButtons array
        if (menuButtons != null && menuButtons.Length > 0)
        {
            // Find and assign button click handlers
            foreach (Button button in menuButtons)
            {
                if (button != null)
                {
                    button.onClick.AddListener(PlayButtonClickSound);
                }
                else
                {
                    Debug.LogWarning("A button in menuButtons array is not assigned.");
                }
            }
        }
        else
        {
            Debug.LogWarning("Menu buttons array is not assigned or empty.");
        }

}

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void NewGameDialog_YES()
    {
        // Save the selected difficulty to PlayerPrefs
        PlayerPrefs.SetInt("masterDifficulty", _difficultyLevel);
        PlayerPrefs.Save();
        
        // Assuming you have a way to create a new maze and pass it to the scene:
        int2 mazeSize = new int2(10, 10); // Example size, you might have this configurable
        Maze maze = new Maze(mazeSize);

        // Create and schedule the job
        GenerateMazeJob generateMazeJob = new GenerateMazeJob
        {
            maze = maze,
            seed = UnityEngine.Random.Range(0, int.MaxValue),
           
        };

        // Schedule and complete the job
        JobHandle handle = generateMazeJob.Schedule();
        handle.Complete();

        // Pass the generated maze to your game scene here
        SceneManager.LoadScene(_newGameLevel);

        // Play background music when starting a new game
        PlayBackgroundMusic();
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

    private void PlayButtonClickSound()
    {
        if (buttonClickSource != null && buttonClickClip != null)
        {
            buttonClickSource.PlayOneShot(buttonClickClip);
        }
        else
        {
            Debug.LogWarning("Button Click Source or Clip is not assigned in the MenuController.");
        }
    }

    private void PlayBackgroundMusic()
    {
        if (backgroundMusicSource != null && backgroundMusicClip != null)
        {
            if (!backgroundMusicSource.isPlaying)
            {
                backgroundMusicSource.clip = backgroundMusicClip; // Assign the audio clip
                backgroundMusicSource.loop = true; // Ensure the music loops
                backgroundMusicSource.volume = PlayerPrefs.GetFloat("masterVolume", defaultVolume); // Set volume based on saved settings
                backgroundMusicSource.Play(); // Start playing the music
            }
        }
        else
        {
            Debug.LogWarning("Background Music Source or Clip is not assigned in the MenuController.");
        }
    }

    public IEnumerator ConfirmationBox()
    {
        confirmationPrompt.SetActive(true);
        yield return new WaitForSeconds(2);
        confirmationPrompt.SetActive(false);
    }

}
