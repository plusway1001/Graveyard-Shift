using UnityEngine;

public class EnemyAI : MonoBehaviour
{

    [Header("Chase")]
    public float moveSpeed = 3f;

    [Header("Attack")]
    public int damage = 1;
    public float damageCooldown = 1f;

    private float nextDamageTime;

    private Transform player;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void FixedUpdate()
    {
        if (player == null)
            return;

        Vector2 direction =
            (player.position - transform.position).normalized;

        rb.MovePosition(
            rb.position + direction * moveSpeed * Time.fixedDeltaTime
        );

        float angle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        rb.rotation = angle;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (Time.time >= nextDamageTime)
        {
            nextDamageTime = Time.time + damageCooldown;

            PlayerHealth playerHealth =
                collision.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }
}
