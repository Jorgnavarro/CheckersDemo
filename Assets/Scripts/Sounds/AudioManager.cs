using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")] 
    public AudioSource backgroundMusic;
    public AudioSource captureSound;
    public AudioSource buttonClickSound;
    public AudioSource moveSound;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetBackgroundVolume(0.6f); // config initial volume to 60%
        PlayBackgroundMusic();
    }
    
    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null)
        {
            backgroundMusic.loop = true;
            backgroundMusic.Play();
        }
    }

    public void PlayMoveSound()
    {
        if (moveSound != null)
        {
            moveSound.Play();
        }
    }

    public void PlayCaptureSound()
    {
        if (captureSound != null)
        {
            captureSound.Play();
        }
    }

    public void PlayButtonClickSound()
    {
        if (buttonClickSound != null)
        {
            buttonClickSound.Play();
        }
    }

    public void SetBackgroundVolume(float volume)
    {
        if (backgroundMusic != null)
        {
            backgroundMusic.volume = volume;
        }
    }
 
    

    
}
