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

    private AudioSource bgmSource;
    private AudioSource sfxSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Useful if we have multiple scenes, but okay if left out for single scene
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
