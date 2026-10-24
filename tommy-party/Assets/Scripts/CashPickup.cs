using UnityEngine;

// Put on a cash prefab (SpriteRenderer + CircleCollider2D set to Is Trigger).
// The table spawns one per customer. The player walks over it to collect.
public class CashPickup : MonoBehaviour
{
    [SerializeField] private int value = 1;
    public AudioClip clip;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        RevenueManager.Instance?.AddCash(value);
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
