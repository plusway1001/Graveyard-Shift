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

    [Header("Hit Effects")]
    [SerializeField]
    private GameObject hitEffects;

    private Rigidbody2D rb;
    private bool hasHit;
    public AudioClip clip;

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
        if (hasHit)
            return;

        // Only the player should lose an ingredient.
        if (other.CompareTag("Player"))
        {
            hasHit = true;

            Debug.Log("Riot projectile hit the player!");

            // Find the inventory safely.
            PlayerInventory inventory =
                PlayerInventory.Instance;

            if (inventory == null)
            {
                inventory = other.GetComponent<PlayerInventory>();
            }

            if (inventory == null)
            {
                inventory = other.GetComponentInParent<PlayerInventory>();
            }

            if (inventory != null)
            {
                inventory.LoseIngredient();
            }
            else
            {
                Debug.LogWarning(
                    "Player hit, but PlayerInventory was not found."
                );
            }

            SpawnHitEffect(other.transform.position);
            Destroy(gameObject);
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
}

/*using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class RiotProjectile : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField, Min(0.1f)]
    private float speed = 7f;

    [SerializeField, Min(0.1f)]
    private float lifetime = 5f;

    private Rigidbody2D rb;
    private bool hasHit;
    public static PlayerInventory playerinv;
    public GameObject hiteffects;

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
        {
            direction = Vector2.right;
        }

        direction.Normalize();

        rb.gravityScale = 0f;
        rb.collisionDetectionMode =
            CollisionDetectionMode2D.Continuous;

        rb.linearVelocity = direction * speed;

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player Hit!");
            playerinv.LoseIngredient();
            // You can access components on the entering object
            // PlayerHealth health = other.GetComponent<PlayerHealth>();
        }
        if (hasHit)
            return;

        hasHit = true;

        Instantiate(hiteffects, other.transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}*/
