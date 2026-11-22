using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class RiotProjectile : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField, Min(0.1f)]
    private float speed = 7f;

    [SerializeField, Min(0.1f)]
    private float lifetime = 5f;

    [Header("Damage")]
    [SerializeField, Min(1)]
    private int hidingSpotDamage = 1;

    [Header("Hit Effects")]
    [SerializeField]
    private GameObject hitEffects;

    private Rigidbody2D rb;
    private bool hasHit;
    public AudioClip clip, clip2;

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

        Vector2 direction =
            (Vector2)(player.position - transform.position);

        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.right;

        direction.Normalize();

        rb.gravityScale = 0f;
        rb.collisionDetectionMode =
            CollisionDetectionMode2D.Continuous;

        rb.linearVelocity = direction * speed;

        StartCoroutine(DestroyAfterLifetime());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        // Hit a hiding box: damage it
        if (other.CompareTag("HidingSpot"))
        {
            hasHit = true;

            HidingSpot spot = other.GetComponentInParent<HidingSpot>();
            if (spot != null) spot.TakeDamage(hidingSpotDamage);

            if (AudioManager.Instance != null && clip2 != null)
                AudioManager.Instance.PlaySFX(clip2);

            SpawnHitEffect(other.transform.position);

            Destroy(gameObject);
            return;
        }

        // Hit the player
        if (!other.CompareTag("Player")) return;

        hasHit = true;

        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health != null) health.TakeProjectileDamage(1);

        SpawnHitEffect(other.transform.position);

        if (AudioManager.Instance != null && clip != null)
            AudioManager.Instance.PlaySFX(clip);

        Destroy(gameObject);
    }

    private void SpawnHitEffect(Vector3 position)
    {
        if (hitEffects != null)
        {
            Instantiate(
                hitEffects,
                position,
                Quaternion.identity
            );
        }
    }

    private System.Collections.IEnumerator DestroyAfterLifetime()
    {
        yield return new WaitForSeconds(lifetime);

        // Spawn effect at the projectile's current position.
        SpawnHitEffect(transform.position);

        Destroy(gameObject);
    }
}

/*using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class RiotProjectile : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField, Min(0.1f)]
    private float speed = 7f;

    [SerializeField, Min(0.1f)]
    private float lifetime = 5f;

    [Header("Hit Effects")]
    [SerializeField]
    private GameObject hitEffects;

    private Rigidbody2D rb;
    private bool hasHit;
    public AudioClip clip, clip2;

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

        Vector2 direction =
            (Vector2)(player.position - transform.position);

        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.right;

        direction.Normalize();

        rb.gravityScale = 0f;
        rb.collisionDetectionMode =
            CollisionDetectionMode2D.Continuous;

        rb.linearVelocity = direction * speed;

        //Destroy(gameObject, lifetime);
        StartCoroutine(DestroyAfterLifetime());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("HidingSpot"))
        {
            if (AudioManager.Instance != null && clip != null)
                AudioManager.Instance.PlaySFX(clip2);
            Destroy(gameObject);
            return;
        }

        if (hasHit || !other.CompareTag("Player")) return;
        hasHit = true;
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health != null) health.TakeProjectileDamage(1);
        SpawnHitEffect(other.transform.position);
        if (AudioManager.Instance != null && clip != null)
            AudioManager.Instance.PlaySFX(clip);
        Destroy(gameObject);
    }

    private void SpawnHitEffect(Vector3 position)
    {
        if (hitEffects != null)
        {
            Instantiate(
                hitEffects,
                position,
                Quaternion.identity
            );
        }
    }

    private System.Collections.IEnumerator DestroyAfterLifetime()
    {
        yield return new WaitForSeconds(lifetime);

        // Spawn effect at the projectile's current position.
        if (hitEffects != null)
        {
            Instantiate(
                hitEffects,
                transform.position,
                Quaternion.identity
            );
        }

        Destroy(gameObject);
    }
}*/