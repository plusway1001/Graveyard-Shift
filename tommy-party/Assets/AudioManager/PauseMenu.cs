
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;

    public static bool isPaused = false;

    public AudioClip clip;

    public float SetLowVol = 0.3f;

    void Start()
    {
        Time.timeScale = 1f;
        AudioManager.Instance.SetVolume(1f);

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    void Update()
    {
        // Press Escape to pause or resume
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(clip);
            }
            else
            {
                Debug.LogWarning("AudioManager instance is missing from the scene!");
            }
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        AudioManager.Instance.SetVolume(SetLowVol);

        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        AudioManager.Instance.SetVolume(1f);

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }
}
