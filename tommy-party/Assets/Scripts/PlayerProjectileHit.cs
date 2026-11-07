using UnityEngine;

// Compatibility handler for any projectile prefab tagged EnemyProjectile.
// RiotProjectile has its own handler; do not add both handlers to the same projectile.
public class PlayerProjectileHit : MonoBehaviour
{
    [SerializeField] private string projectileTag = "EnemyProjectile";
    [SerializeField] private bool destroyProjectile = true;
    private void OnTriggerEnter2D(Collider2D other) => TryHit(other.gameObject);
    private void OnCollisionEnter2D(Collision2D collision) => TryHit(collision.gameObject);
    private void TryHit(GameObject obj)
    {
        if (!obj.CompareTag(projectileTag)) return;
        if (destroyProjectile) Destroy(obj);
        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health == null) health = GetComponentInParent<PlayerHealth>();
        if (health != null) health.TakeProjectileDamage(1);
    }
}
