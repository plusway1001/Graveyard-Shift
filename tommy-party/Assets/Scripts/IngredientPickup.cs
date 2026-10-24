using UnityEngine;

// Put on an ingredient prefab (SpriteRenderer + CircleCollider2D set to Is Trigger).
// The player walks over it to collect it.
public class IngredientPickup : MonoBehaviour
{
    [SerializeField] private int amount = 1;
    [Tooltip("Seconds before it can be picked up (so a dropped one isn't grabbed instantly)")]
    [SerializeField] private float pickupDelay = 0.6f;
    [SerializeField] private float bobHeight = 0.08f;

    private float spawnTime;
    private Vector3 basePos;
    public AudioClip clip;

    private void Start()
    {
        spawnTime = Time.time;
        basePos = transform.position;
    }

    private void Update()
    {
        transform.position = basePos + Vector3.up * Mathf.Sin(Time.time * 4f) * bobHeight;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time - spawnTime < pickupDelay || !other.CompareTag("Player")) return;
        if (PlayerInventory.Instance.AddIngredients(amount) > 0)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(clip);
            }
            else
            {
                Debug.LogWarning("AudioManager instance is missing from the scene!");
            }
            Destroy(gameObject);
        }
    }

    // Lets other scripts move it after spawning (e.g. a little toss)
    public void SetBasePosition(Vector3 pos)
    {
        basePos = pos;
        transform.position = pos;
    }
}
