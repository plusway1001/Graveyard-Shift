using UnityEngine;

// Put on banana peel with a trigger collider. Player loses all carried dishes.
public class BananaPeel : MonoBehaviour
{
    [SerializeField] private bool destroyPeelOnContact = true;
    [SerializeField] private AudioClip slipSound;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health != null && health.IsInvulnerable) return; // Dash/hit protection dodges the peel.
        PlayerInventory inventory = other.GetComponentInParent<PlayerInventory>();
        if (inventory != null) inventory.LoseAllDishes();
        if (health != null) health.GrantInvulnerability(healthHitProtection);
        if (AudioManager.Instance != null && slipSound != null) AudioManager.Instance.PlaySFX(slipSound);
        if (destroyPeelOnContact) Destroy(gameObject);
    }
    [SerializeField, Min(0f)] private float healthHitProtection = 0.8f;
}
