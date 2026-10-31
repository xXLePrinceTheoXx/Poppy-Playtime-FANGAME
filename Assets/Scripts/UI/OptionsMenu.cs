using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [Header("Sliders")]
    public Slider sensitivitySlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("Toggle & Dropdown")]
    public Toggle fullscreenToggle;
    public Dropdown qualityDropdown;

    [Header("Text infos")]
    public Text sensitivityValueText;

    [Header("Références")]
    public FPSController playerController;

    void Start()
    {
        if (sensitivitySlider != null && playerController != null)
        {
            sensitivitySlider.value = playerController.mouseSensitivity;
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        }

        if (qualityDropdown != null)
        {
            qualityDropdown.value = QualitySettings.GetQualityLevel();
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        }

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    public void OnSensitivityChanged(float value)
    {
        if (playerController != null) playerController.SetSensitivity(value);
        if (sensitivityValueText != null)
            sensitivityValueText.text = value.ToString("F1");
    }

    public void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void OnQualityChanged(int index)
    {
        QualitySettings.SetQualityLevel(index);
    }

    public void OnMusicVolumeChanged(float value)
    {
        // À relier à ton AudioMixer ou AudioSource de musique
        // Exemple : AudioListener.volume = value;
    }

    public void OnSFXVolumeChanged(float value)
    {
        // À relier au volume des effets sonores
    }

    public void RefreshUI()
    {
        if (sensitivitySlider != null && playerController != null)
            sensitivitySlider.value = playerController.mouseSensitivity;
        if (fullscreenToggle != null) fullscreenToggle.isOn = Screen.fullScreen;
        if (qualityDropdown != null) qualityDropdown.value = QualitySettings.GetQualityLevel();
    }

    public void ApplyAndSave()
    {
        if (playerController != null)
            PlayerPrefs.SetFloat("MouseSensitivity", playerController.mouseSensitivity);
        PlayerPrefs.SetInt("Fullscreen", Screen.fullScreen ? 1 : 0);
        PlayerPrefs.SetInt("QualityLevel", QualitySettings.GetQualityLevel());
        PlayerPrefs.Save();
    }
}