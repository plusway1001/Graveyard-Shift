using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Load selected scene
    public void LoadScene(int sceneID)
    {
        SceneManager.LoadScene(sceneID);
    }

    // Quit game
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Exited");
    }

    public void PlaySound(AudioClip clip)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clip);
        }
        else
        {
            Debug.LogWarning("AudioManager instance is missing from the scene!");
        }
    }
}
