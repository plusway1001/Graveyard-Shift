using UnityEngine;

// Put on the Player. When an enemy projectile hits the player,
// they lose 1 ingredient, which drops on the floor and can be picked back up.
// Tag your enemy projectile prefab "EnemyProjectile".
public class PlayerProjectileHit : MonoBehaviour
{
    [SerializeField] private string projectileTag = "EnemyProjectile";
    [SerializeField] private float invulnerableTime = 0.75f;
    [Tooltip("Leave empty if the lost ingredient should just vanish")]
    [SerializeField] private IngredientPickup droppedIngredientPrefab;
    [SerializeField] private float dropDistance = 1.2f;
    [SerializeField] private bool destroyProjectile = true;

    private float lastHitTime = -99f;

    private void OnTriggerEnter2D(Collider2D other) => TryHit(other.gameObject);
    private void OnCollisionEnter2D(Collision2D col) => TryHit(col.gameObject);

    private void TryHit(GameObject obj)
    {
        if (obj.tag != projectileTag) return;
        if (destroyProjectile) Destroy(obj);
        if (Time.time - lastHitTime < invulnerableTime) return;
        lastHitTime = Time.time;

        if (!PlayerInventory.Instance.LoseIngredient()) return;

        if (droppedIngredientPrefab != null)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            var drop = Instantiate(droppedIngredientPrefab, transform.position, Quaternion.identity);
            drop.SetBasePosition(transform.position + (Vector3)(dir * dropDistance));
        }
    }
}

