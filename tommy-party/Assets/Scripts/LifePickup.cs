using UnityEngine;

// Put on a life pickup prefab with a 2D trigger collider.
public class LifePickup : MonoBehaviour
{
    [SerializeField, Min(1)] private int livesToGive = 1;
    [SerializeField] private AudioClip pickupSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health == null || health.CurrentLives >= health.MaxLives) return;
        health.AddLife(livesToGive);
        if (AudioManager.Instance != null && pickupSound != null)
            AudioManager.Instance.PlaySFX(pickupSound);
        Destroy(gameObject);
    }
}
