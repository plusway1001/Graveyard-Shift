using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource movementSource;
    [SerializeField] private AudioSource ambienceSource;

    [Header("BGM Clips")]
    [SerializeField] private AudioClip[] BGM;

    [Header("Scene Names")]
    [SerializeField] private int[] BGMName;

    // Volumes the sources were set to in the Inspector. The settings sliders
    // scale these, so your own mix (e.g. quiet ambience) is kept.
    private float bgmBaseVolume = 1f;
    private float sfxBaseVolume = 1f;
    private float movementBaseVolume = 1f;
    private float ambienceBaseVolume = 1f;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Auto-find AudioSource if not assigned
        if (bgmSource == null)
        {
            bgmSource = GetComponent<AudioSource>();
        }

        CacheBaseVolumes();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // BGM
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource == null)
            return;

        if (bgmSource.clip == clip)
            return;

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    // SFX
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
            return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlaySFXTraps(AudioClip clip, float volume)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    public void SetVolume(float Vol)
    {
        bgmSource.volume = Vol;
    }

    // ============================================
    // SETTINGS PAGE API (used by AudioSettingsMenu)
    // "SFX" = sfxSource + movementSource + ambienceSource
    // ============================================

    private void CacheBaseVolumes()
    {
        if (bgmSource != null) bgmBaseVolume = bgmSource.volume;
        if (sfxSource != null) sfxBaseVolume = sfxSource.volume;
        if (movementSource != null) movementBaseVolume = movementSource.volume;
        if (ambienceSource != null) ambienceBaseVolume = ambienceSource.volume;
    }

    // value: 0 (silent) to 1 (the Inspector volume)
    public void SetBGMVolume(float value)
    {
        value = Mathf.Clamp01(value);

        if (bgmSource != null)
            bgmSource.volume = bgmBaseVolume * value;
    }

    public void SetSFXVolume(float value)
    {
        value = Mathf.Clamp01(value);

        if (sfxSource != null) sfxSource.volume = sfxBaseVolume * value;
        if (movementSource != null) movementSource.volume = movementBaseVolume * value;
        if (ambienceSource != null) ambienceSource.volume = ambienceBaseVolume * value;
    }

    public void SetBGMMuted(bool muted)
    {
        if (bgmSource != null)
            bgmSource.mute = muted;
    }

    public void SetSFXMuted(bool muted)
    {
        if (sfxSource != null) sfxSource.mute = muted;
        if (movementSource != null) movementSource.mute = muted;
        if (ambienceSource != null) ambienceSource.mute = muted;
    }

    public void PlayMovementSFX(bool status)
    {
        if (status)
        {
            movementSource.Play();
        }
        else
        {
            movementSource.Stop();
        }
    }
    public void PlayAmbienceSFX(bool status)
    {
        if (status)
        {
            ambienceSource.Play();
        }
        else
        {
            ambienceSource.Stop();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        int count = Mathf.Min(BGMName.Length, BGM.Length);

        for (int i = 0; i < count; i++)
        {
            if (scene.buildIndex == BGMName[i])
            {
                PlayBGM(BGM[i]);
                break;
            }
        }
    }
}

/*using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource movementSource;
    [SerializeField] private AudioSource ambienceSource;

    [Header("BGM Clips")]
    [SerializeField] private AudioClip[] BGM;

    [Header("Scene Names")]
    [SerializeField] private int[] BGMName;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Auto-find AudioSource if not assigned
        if (bgmSource == null)
        {
            bgmSource = GetComponent<AudioSource>();
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // BGM
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource == null)
            return;

        if (bgmSource.clip == clip)
            return;

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    // SFX
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
            return;
        sfxSource.PlayOneShot(clip);
    }

    public void SetVolume(float Vol)
    {
        bgmSource.volume = Vol;
    }

    public void PlayMovementSFX(bool status)
    {
        if (status)
        {
            movementSource.Play();
        }
        else
        {
            movementSource.Stop();
        }
    }
    public void PlayAmbienceSFX(bool status)
    {
        if (status)
        {
            ambienceSource.Play();
        }
        else
        {
            ambienceSource.Stop();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        int count = Mathf.Min(BGMName.Length, BGM.Length);

        for (int i = 0; i < count; i++)
        {
            if (scene.buildIndex == BGMName[i])
            {
                PlayBGM(BGM[i]);
                break;
            }
        }
    }
}*/
