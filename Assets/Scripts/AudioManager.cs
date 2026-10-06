using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Clips")]
    [Tooltip("Drag the background ambiance music here.")]
    public AudioClip ambianceClip;
    [Tooltip("Kısık arka plan fısıltıları için buraya ses dosyası sürükleyin.")]
    public AudioClip whispersClip;
    [Tooltip("Drag the UI button click sound here.")]
    public AudioClip uiClickClip;
    [Tooltip("Drag the sound for entering an orbit here.")]
    public AudioClip orbitEntryClip;

    private AudioSource bgmSource;
    private AudioSource sfxSource;
    private AudioSource whispersSource;

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

        // Setup Whispers Source (Arka plan fısıltıları)
        whispersSource = gameObject.AddComponent<AudioSource>();
        whispersSource.loop = true;
        whispersSource.playOnAwake = false;
        whispersSource.volume = 0.35f; // Fısıltı olduğu için kısık ses
        whispersSource.pitch = 0.85f; // Fısıltıyı biraz daha derin ve tekinsiz yapmak için pitch düşürüldü

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

        if (whispersClip != null)
        {
            whispersSource.clip = whispersClip;
            whispersSource.Play();
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
