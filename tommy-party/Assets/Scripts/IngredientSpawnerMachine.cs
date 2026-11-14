using UnityEngine;

/// <summary>
/// Machine the player activates with E (or the mobile button).
/// Each activation spawns ONE ingredient at a random point inside the
/// spawn area. It cannot be used again until that ingredient is collected.
/// Needs a trigger Collider2D to detect when the player is close enough.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class IngredientSpawnerMachine : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Ingredient")]
    [SerializeField] private IngredientPickup ingredientPrefab;

    [Header("Spawn Area (the room)")]
    [Tooltip("Centre of the area, relative to this object.")]
    [SerializeField] private Vector2 areaOffset = Vector2.zero;
    [SerializeField] private Vector2 areaSize = new Vector2(10f, 6f);

    [Header("Avoid Obstacles (optional)")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField, Min(0.05f)] private float clearRadius = 0.3f;
    [SerializeField, Min(1)] private int maxAttempts = 20;

    [Header("Audio (optional)")]
    [SerializeField] private AudioClip activateClip;

    private IngredientPickup activeIngredient;
    private int playerColliderCount;

    // Unity treats a destroyed object as null, so collecting the
    // ingredient automatically makes the machine usable again.
    public bool CanActivate => activeIngredient == null;

    private bool PlayerInRange => playerColliderCount > 0;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Update()
    {
        if (PlayerInRange && Input.GetKeyDown(interactKey))
            Interact();
    }

    // Also callable from a mobile UI button.
    public void Interact()
    {
        if (!PlayerInRange || !CanActivate)
            return;

        SpawnIngredient();
    }

    private void SpawnIngredient()
    {
        if (ingredientPrefab == null)
        {
            Debug.LogWarning(name + ": Ingredient prefab is missing.", this);
            return;
        }

        Vector2 position = GetRandomSpawnPoint();
        activeIngredient = Instantiate(ingredientPrefab, position, Quaternion.identity);

        if (activateClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(activateClip);
    }

    private Vector2 GetRandomSpawnPoint()
    {
        Vector2 center = (Vector2)transform.position + areaOffset;
        Vector2 point = center;

        for (int i = 0; i < maxAttempts; i++)
        {
            point = center + new Vector2(
                Random.Range(-areaSize.x / 2f, areaSize.x / 2f),
                Random.Range(-areaSize.y / 2f, areaSize.y / 2f)
            );

            // Accept the first point that isn't inside a wall/obstacle.
            if (obstacleMask.value == 0 ||
                Physics2D.OverlapCircle(point, clearRadius, obstacleMask) == null)
            {
                return point;
            }
        }

        // Every attempt hit an obstacle; use the last point anyway.
        return point;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerColliderCount++;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerColliderCount = Mathf.Max(0, playerColliderCount - 1);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            (Vector2)transform.position + areaOffset,
            new Vector3(areaSize.x, areaSize.y, 0f)
        );
    }
}
