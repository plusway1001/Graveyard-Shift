using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Put on any object in a scene (e.g. a splash or loading scene).
/// After a random delay between minDelay and maxDelay seconds,
/// it loads the chosen scene.
/// </summary>
public class TimedSceneLoader : MonoBehaviour
{
    public enum LoadMode
    {
        NextInBuildOrder,
        ByIndex,
        ByName
    }

    [Header("Delay (seconds)")]
    [SerializeField, Min(0f)] private float minDelay = 4f;
    [SerializeField, Min(0f)] private float maxDelay = 6f;

    [Header("Next Scene")]
    [SerializeField] private LoadMode loadMode = LoadMode.ByIndex;

    [Tooltip("Used by ByIndex. The scene's number in File > Build Settings.")]
    [SerializeField, Min(0)] private int sceneIndex = 1;

    [Tooltip("Used by ByName.")]
    [SerializeField] private string sceneName;

    private void OnValidate()
    {
        if (maxDelay < minDelay)
            maxDelay = minDelay;
    }

    private void Start()
    {
        StartCoroutine(LoadAfterDelay());
    }

    private IEnumerator LoadAfterDelay()
    {
        float delay = Random.Range(minDelay, maxDelay);

        // Realtime so it still works if Time.timeScale is 0.
        yield return new WaitForSecondsRealtime(delay);

        LoadScene();
    }

    private void LoadScene()
    {
        switch (loadMode)
        {
            case LoadMode.NextInBuildOrder:
                LoadByIndex(SceneManager.GetActiveScene().buildIndex + 1);
                break;

            case LoadMode.ByIndex:
                LoadByIndex(sceneIndex);
                break;

            case LoadMode.ByName:
                if (string.IsNullOrWhiteSpace(sceneName))
                {
                    Debug.LogWarning(name + ": Scene name is empty.", this);
                    return;
                }

                SceneManager.LoadScene(sceneName);
                break;
        }
    }

    private void LoadByIndex(int index)
    {
        if (index < 0 || index >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning(
                name + ": Scene index " + index + " is not in Build Settings " +
                "(there are " + SceneManager.sceneCountInBuildSettings + " scenes).",
                this
            );
            return;
        }

        SceneManager.LoadScene(index);
    }
}