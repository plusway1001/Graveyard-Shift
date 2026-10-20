
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
        if (hasHit)
            return;

        hasHit = true;
        
        Destroy(gameObject);

        /*PlayerIngredientInventory inventory =
            other.GetComponentInParent<PlayerIngredientInventory>();

        if (inventory != null)
        {
            hasHit = true;
            inventory.LoseIngredients(1);
            Destroy(gameObject);
        }*/
    }
}
