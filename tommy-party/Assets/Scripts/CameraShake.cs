using UnityEngine;

public class CameraShake : MonoBehaviour
{

    public static CameraShake Instance;

    private CameraFollow cameraFollow;

    private float shakeDuration;
    private float shakeMagnitude;

    private void Awake()
    {
        Instance = this;

        cameraFollow = GetComponent<CameraFollow>();
    }

    private void Update()
    {
        if (shakeDuration > 0)
        {
            cameraFollow.shakeOffset =
                (Vector3)Random.insideUnitCircle * shakeMagnitude;

            shakeDuration -= Time.deltaTime;
        }
        else
        {
            shakeDuration = 0f;
            cameraFollow.shakeOffset = Vector3.zero;
        }
    }

    public void Shake(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
    }
}
