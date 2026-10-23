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
        sfxSource.PlayOneShot(clip);
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
