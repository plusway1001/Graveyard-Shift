using System.Collections;
using UnityEngine;

// Attach to Player. PC: Space. Mobile: connect Dash() to a UI Button.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDash : MonoBehaviour
{
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerHealth health;

    [Header("Dash Settings")]
    [SerializeField, Min(0.1f)] private float dashSpeed = 13f;
    [SerializeField, Min(0.05f)] private float dashDuration = 0.18f;
    [SerializeField, Min(0f)] private float dashCooldown = 1.5f;
    [SerializeField] private KeyCode dashKey = KeyCode.Space;

    [Header("Dash Collision")]
    [Tooltip("Only solid level obstacles should be included, such as Walls and Furniture. Do not include SpikeHazard triggers.")]
    [SerializeField] private LayerMask obstacleLayers;
    [SerializeField, Min(0f)] private float wallSkin = 0.03f;

    public bool IsDashing { get; private set; }
    public float CooldownRemaining { get; private set; }
    public float DashCooldown => dashCooldown;

    private Rigidbody2D rb;
    private Collider2D bodyCollider;
    private readonly RaycastHit2D[] castHits = new RaycastHit2D[8];

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (health == null) health = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (CooldownRemaining > 0f)
            CooldownRemaining = Mathf.Max(0f, CooldownRemaining - Time.deltaTime);

        if (Input.GetKeyDown(dashKey)) Dash();
    }

    public void Dash()
    {
        //if (IsDashing || CooldownRemaining > 0f || (health != null && health.IsGameOver)) return;

        if (IsDashing || CooldownRemaining > 0f ||
    (health != null && health.IsGameOver) ||
    (movement != null && movement.IsFrozen)) return;

        Vector2 direction = movement != null ? movement.CurrentMovement : Vector2.zero;
        if (direction.sqrMagnitude < 0.01f) direction = Vector2.right;
        StartCoroutine(DashRoutine(direction.normalized));
    }

    private IEnumerator DashRoutine(Vector2 direction)
    {
        IsDashing = true;
        CooldownRemaining = dashCooldown;
        if (health != null) health.GrantInvulnerability(dashDuration + 0.08f);

        float elapsed = 0f;
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(obstacleLayers);
        filter.useTriggers = false;

        while (elapsed < dashDuration)
        {
            float stepDistance = dashSpeed * Time.fixedDeltaTime;
            float allowedDistance = stepDistance;

            // Cast the player's actual collider shape before moving. This prevents tunnelling through walls.
            if (bodyCollider != null && obstacleLayers.value != 0)
            {
                int hitCount = bodyCollider.Cast(direction, filter, castHits, stepDistance + wallSkin);
                float nearestHit = float.PositiveInfinity;
                for (int i = 0; i < hitCount; i++)
                {
                    if (castHits[i].collider == null) continue;
                    if (castHits[i].distance < nearestHit) nearestHit = castHits[i].distance;
                }

                if (!float.IsPositiveInfinity(nearestHit))
                {
                    allowedDistance = Mathf.Clamp(nearestHit - wallSkin, 0f, stepDistance);
                    if (allowedDistance < stepDistance) elapsed = dashDuration;
                }
            }

            rb.MovePosition(rb.position + direction * allowedDistance);
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        IsDashing = false;
    }
}
