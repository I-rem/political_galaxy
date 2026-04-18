using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Clips")]
    [Tooltip("Drag the background ambiance music here.")]
    public AudioClip ambianceClip;
    [Tooltip("Drag the UI button click sound here.")]
    public AudioClip uiClickClip;
    [Tooltip("Drag the sound for entering an orbit here.")]
    public AudioClip orbitEntryClip;
    [Tooltip("Drag the base hum/drone sound for each planet here.")]
    public AudioClip planetHumClip;

    private AudioSource bgmSource;
    private AudioSource sfxSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
            // Ensure there is an AudioListener in the scene to actually hear sounds
            if (FindObjectOfType<AudioListener>() == null)
            {
                if (Camera.main != null) 
                    Camera.main.gameObject.AddComponent<AudioListener>();
                else 
                    gameObject.AddComponent<AudioListener>();
            }

            SetupAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetupAudioSources()
    {
        // Setup BGM Source
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = 0.5f; // reasonable default volume

        // Setup SFX Source
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = 1f;

        if (ambianceClip != null)
        {
            bgmSource.clip = ambianceClip;
            bgmSource.Play();
        }
    }

    public void PlayUIClick()
    {
        if (sfxSource != null && uiClickClip != null)
        {
            sfxSource.PlayOneShot(uiClickClip);
        }
    }

    public void PlayOrbitEntry()
    {
        if (sfxSource != null && orbitEntryClip != null)
        {
            sfxSource.PlayOneShot(orbitEntryClip);
        }
    }
}
