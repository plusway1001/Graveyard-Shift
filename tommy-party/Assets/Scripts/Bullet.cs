using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float shakecamDuration = 0.08f, shakecamMagnitude = 0.12f;
    public float speed = 15f;
    public float lifeTime = 3f;
    public int damage = 1;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        rb.linearVelocity = transform.up * speed;

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignore player collision
        if (collision.CompareTag("Player"))
            return;
        // Damage enemy
        EnemyHealth enemy =
            collision.GetComponent<EnemyHealth>();

        if (enemy != null)
        {
            Vector2 hitDirection =
            (collision.transform.position - transform.position).normalized;

            enemy.TakeDamage(damage, hitDirection);

            //CameraShake.Instance.Shake(0.12f, 0.18f);
        }
        else if (collision.CompareTag("Wall"))
        {
            //CameraShake.Instance.Shake(0.05f, 0.08f);
        }

        // Shake camera on hit
        CameraShake.Instance.Shake(shakecamDuration, shakecamMagnitude);

        Destroy(gameObject);
    }
}

