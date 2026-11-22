using UnityEngine;

/// <summary>
/// Keeps rotating the object it is on (2D, around the Z axis).
/// Direction can be clockwise, anti-clockwise, or random.
/// </summary>
public class ObjectRotator : MonoBehaviour
{
    public enum RotationDirection
    {
        Clockwise,
        AntiClockwise,
        Random
    }

    [Header("Rotation")]
    [SerializeField] private RotationDirection direction = RotationDirection.Clockwise;
    [Tooltip("Degrees per second.")]
    [SerializeField, Min(0f)] private float speed = 90f;

    [Header("Random Direction Only")]
    [Tooltip("0 = pick one random direction at the start and keep it. Above 0 = pick a new random direction every this many seconds.")]
    [SerializeField, Min(0f)] private float changeDirectionInterval = 0f;

    private float directionSign = -1f;   // -1 = clockwise, +1 = anti-clockwise
    private float changeTimer;

    private void Start()
    {
        ApplyDirection();
    }

    private void Update()
    {
        if (direction == RotationDirection.Random && changeDirectionInterval > 0f)
        {
            changeTimer -= Time.deltaTime;

            if (changeTimer <= 0f)
                PickRandomDirection();
        }

        transform.Rotate(0f, 0f, directionSign * speed * Time.deltaTime);
    }

    private void ApplyDirection()
    {
        switch (direction)
        {
            case RotationDirection.Clockwise:
                directionSign = -1f;
                break;

            case RotationDirection.AntiClockwise:
                directionSign = 1f;
                break;

            case RotationDirection.Random:
                PickRandomDirection();
                break;
        }
    }

    private void PickRandomDirection()
    {
        directionSign = Random.value < 0.5f ? -1f : 1f;
        changeTimer = changeDirectionInterval;
    }

    // Optional: change direction from other scripts.
    public void SetDirection(RotationDirection newDirection)
    {
        direction = newDirection;
        ApplyDirection();
    }

    public void SetSpeed(float degreesPerSecond)
    {
        speed = Mathf.Max(0f, degreesPerSecond);
    }
}
