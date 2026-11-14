using UnityEngine;

/// <summary>
/// Mud ball thrown by the Ghoul. Needs a Rigidbody2D and a trigger Collider2D.
/// Freezes the player (no moving, no dashing) for a short time when it hits.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class StickyMudProjectile : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField, Min(0.1f)] private float speed = 6f;
    [SerializeField, Min(0.1f)] private float lifetime = 4f;

    [Header("Freeze Effect")]
    [SerializeField, Min(0.1f)] private float freezeDuration = 1.5f;

    [Header("Obstacles (optional)")]
    [Tooltip("The mud disappears when it hits anything on these layers (walls, etc.).")]
    [SerializeField] private LayerMask obstacleMask;

    [Header("Hit Effects (optional)")]
    [SerializeField] private GameObject hitEffects;
    [SerializeField] private AudioClip hitClip;

    private Rigidbody2D rb;
    private bool hasHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Transform player)
    {
        if (player == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 direction = (Vector2)(player.position - transform.position);

        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.right;

        direction.Normalize();

        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.linearVelocity = direction * speed;

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit)
            return;

        if (other.CompareTag("Player"))
        {
            hasHit = true;

            PlayerFreeze freeze = other.GetComponentInParent<PlayerFreeze>();

            if (freeze != null)
                freeze.Freeze(freezeDuration);

            Splat(other.transform.position);
            return;
        }

        // Hit a wall or other obstacle.
        if (obstacleMask.value != 0 &&
            (obstacleMask.value & (1 << other.gameObject.layer)) != 0)
        {
            hasHit = true;
            Splat(transform.position);
        }
    }

    private void Splat(Vector3 position)
    {
        if (hitEffects != null)
            Instantiate(hitEffects, position, Quaternion.identity);

        if (hitClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(hitClip);

        Destroy(gameObject);
    }
}
