using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    
    [Header("Health")]
    public int maxHealth = 3;

    private int currentHealth;

    private Rigidbody2D rb;

    public float knockbackForce = 5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        currentHealth -= damage;

        rb.AddForce(hitDirection * knockbackForce,
            ForceMode2D.Impulse);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
