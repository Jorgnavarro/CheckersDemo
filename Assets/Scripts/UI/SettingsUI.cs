using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] GameObject settingsPanel;
    
    [Header("Sounds Handler")]
    [SerializeField] private Slider backgroundVolumeSlider;

    [SerializeField] private Slider captureVolumeSlider;
    
    [SerializeField] private Slider moveVolumeSlider;
    
    
    void Start()
    {
        settingsPanel.SetActive(false); 

        // initial values
        backgroundVolumeSlider.value = AudioManager.Instance.backgroundMusic.volume;
        captureVolumeSlider.value = AudioManager.Instance.captureSound.volume;
        moveVolumeSlider.value = AudioManager.Instance.moveSound.volume;

        // add listeners to the values
        backgroundVolumeSlider.onValueChanged.AddListener(SetBackgroundVolume);
        captureVolumeSlider.onValueChanged.AddListener(SetCaptureVolume);
        moveVolumeSlider.onValueChanged.AddListener(SetMoveVolume);


    }
    
    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        Time.timeScale = 0; // Game paused
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        Time.timeScale = 1; // resume game
    }

    public void ExitGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("CheckersDemo"); 
    }
    
    private void SetBackgroundVolume(float volume)
    {
        AudioManager.Instance.backgroundMusic.volume = volume;
    }


    private void SetCaptureVolume(float volume)
    {
        AudioManager.Instance.captureSound.volume = volume;
    }

    private void SetMoveVolume(float volume)
    {
        AudioManager.Instance.moveSound.volume = volume;
    }


}
