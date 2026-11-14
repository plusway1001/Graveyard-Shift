using UnityEngine;

/// <summary>
/// Put on the Player. Call Freeze(seconds) to stop the player from
/// moving and dashing for a short time (PlayerDash checks PlayerMovement.IsFrozen).
/// </summary>
public class PlayerFreeze : MonoBehaviour
{
    [Header("Feedback")]
    [Tooltip("Sprite colour while frozen (muddy brown by default).")]
    [SerializeField] private Color frozenTint = new Color(0.6f, 0.45f, 0.25f, 1f);

    public bool IsFrozen { get; private set; }

    private PlayerMovement movement;
    private SpriteRenderer spriteRenderer;

    private Color originalColor = Color.white;
    private float frozenUntil;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    private void Update()
    {
        if (IsFrozen && Time.time >= frozenUntil)
            Unfreeze();
    }

    public void Freeze(float duration)
    {
        // A second hit while frozen just extends the time.
        frozenUntil = Mathf.Max(frozenUntil, Time.time + duration);

        if (IsFrozen)
            return;

        IsFrozen = true;

        if (movement != null)
            movement.SetFrozen(true);

        if (spriteRenderer != null)
            spriteRenderer.color = frozenTint;
    }

    private void Unfreeze()
    {
        IsFrozen = false;

        if (movement != null)
            movement.SetFrozen(false);

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    private void OnDisable()
    {
        if (IsFrozen)
            Unfreeze();
    }
}

/*using UnityEngine;

/// <summary>
/// Put on the Player. Call Freeze(seconds) to stop the player from
/// moving and dashing for a short time.
/// </summary>
public class PlayerFreeze : MonoBehaviour
{
    [Header("Feedback")]
    [Tooltip("Sprite colour while frozen (muddy brown by default).")]
    [SerializeField] private Color frozenTint = new Color(0.6f, 0.45f, 0.25f, 1f);

    public bool IsFrozen { get; private set; }

    private PlayerMovement movement;
    private PlayerDash dash;
    private SpriteRenderer spriteRenderer;

    private Color originalColor = Color.white;
    private bool dashWasEnabled;
    private float frozenUntil;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        dash = GetComponent<PlayerDash>();

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    private void Update()
    {
        if (IsFrozen && Time.time >= frozenUntil)
            Unfreeze();
    }

    public void Freeze(float duration)
    {
        // A second hit while frozen just extends the time.
        frozenUntil = Mathf.Max(frozenUntil, Time.time + duration);

        if (IsFrozen)
            return;

        IsFrozen = true;

        if (movement != null)
            movement.SetFrozen(true);

        if (dash != null)
        {
            dashWasEnabled = dash.enabled;
            dash.enabled = false;
        }

        if (spriteRenderer != null)
            spriteRenderer.color = frozenTint;
    }

    private void Unfreeze()
    {
        IsFrozen = false;

        if (movement != null)
            movement.SetFrozen(false);

        if (dash != null && dashWasEnabled)
            dash.enabled = true;

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    private void OnDisable()
    {
        if (IsFrozen)
            Unfreeze();
    }
}*/
