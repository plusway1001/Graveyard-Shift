using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public Transform target;

    [Header("Follow")]
    public float smoothTime = 0.2f;
    public Vector3 offset;

    [HideInInspector]
    public Vector3 shakeOffset;

    private Vector3 velocity = Vector3.zero;

    private void LateUpdate()
    {
        if (target == null)
            return;

        // Smooth follow position
        Vector3 targetPosition = target.position + offset;

        Vector3 smoothPosition = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );

        // Add shake offset
        transform.position = smoothPosition + shakeOffset;
    }
}
