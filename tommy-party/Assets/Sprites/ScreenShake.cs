using System.Collections;
using UnityEngine;

/// <summary>
/// Attach this to your Main Camera.
/// Call from anywhere:  ScreenShake.Instance.Shake(0.3f, 0.2f);
/// </summary>
public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance { get; private set; }

    [Header("Defaults")]
    [SerializeField] private float defaultDuration = 0.25f;
    [SerializeField] private float defaultMagnitude = 0.15f;

    [Tooltip("Keeps shaking even when Time.timeScale = 0 (e.g. hit-stop or pause).")]
    [SerializeField] private bool useUnscaledTime = true;

    private Vector3 originalLocalPos;
    private Coroutine shakeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        originalLocalPos = transform.localPosition;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>Shake using the default values set in the Inspector.</summary>
    public void Shake()
    {
        Shake(defaultDuration, defaultMagnitude);
    }

    /// <summary>
    /// Shake the camera.
    /// duration  = how long (seconds)
    /// magnitude = how strong (world units, 0.1 - 0.5 is a good range)
    /// </summary>
    public void Shake(float duration, float magnitude)
    {
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            transform.localPosition = originalLocalPos;
        }

        shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Fade out the shake toward the end so it feels smooth
            float falloff = 1f - (elapsed / duration);

            Vector2 offset = Random.insideUnitCircle * magnitude * falloff;
            transform.localPosition = originalLocalPos + new Vector3(offset.x, offset.y, 0f);

            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalLocalPos;
        shakeRoutine = null;
    }
}
