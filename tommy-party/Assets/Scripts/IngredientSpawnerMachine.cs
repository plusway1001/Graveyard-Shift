using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Machine the player activates with E (or the mobile button).
/// Each activation spawns ONE ingredient at a random point inside the
/// spawn area. It cannot be used again until that ingredient is collected.
/// Needs a trigger Collider2D to detect when the player is close enough.
/// Shows READY / NOT READY through a colour or sprite change and optional text.
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

    [Header("Ready Indicator")]
    [Tooltip("Sprite that changes to show the state (the machine itself or a small light on it).")]
    [SerializeField] private SpriteRenderer indicatorRenderer;
    [SerializeField] private Color readyColor = new Color(0.3f, 1f, 0.4f);
    [SerializeField] private Color notReadyColor = new Color(1f, 0.3f, 0.3f);

    [Tooltip("Optional. Swap to these sprites as well as the colour.")]
    [SerializeField] private Sprite readySprite;
    [SerializeField] private Sprite notReadySprite;

    [Tooltip("Optional world text above the machine.")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private string readyMessage = "READY";
    [SerializeField] private string notReadyMessage = "COLLECT INGREDIENT";

    [Tooltip("Gently pulses the indicator while the machine is ready.")]
    [SerializeField] private bool pulseWhenReady = true;
    [SerializeField, Min(0f)] private float pulseAmount = 0.1f;
    [SerializeField, Min(0.1f)] private float pulseSpeed = 4f;

    [Header("Audio (optional)")]
    [SerializeField] private AudioClip activateClip;
    [Tooltip("Plays when the ingredient is collected and the machine is ready again.")]
    [SerializeField] private AudioClip readyAgainClip;

    private IngredientPickup activeIngredient;
    private int playerColliderCount;

    private bool indicatorInitialised;
    private bool lastReady;
    private Vector3 indicatorBaseScale = Vector3.one;

    // Unity treats a destroyed object as null, so collecting the
    // ingredient automatically makes the machine usable again.
    public bool CanActivate => activeIngredient == null;

    private bool PlayerInRange => playerColliderCount > 0;

    public GameObject hitEffects;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Awake()
    {
        if (indicatorRenderer != null)
            indicatorBaseScale = indicatorRenderer.transform.localScale;
    }

    private void Update()
    {
        if (PlayerInRange && Input.GetKeyDown(interactKey))
            Interact();

        UpdateIndicator();
    }

    // Also callable from a mobile UI button.
    public void Interact()
    {
        if (!PlayerInRange || !CanActivate)
            return;

        if (hitEffects != null)
            Instantiate(hitEffects, transform.position, Quaternion.identity);

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
        if (hitEffects != null)
            Instantiate(hitEffects, position, Quaternion.identity);
        activeIngredient = Instantiate(ingredientPrefab, position, Quaternion.identity);

        if (activateClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(activateClip);
    }

    // ---------------- Ready / not ready indicator ----------------

    private void UpdateIndicator()
    {
        bool ready = CanActivate;

        if (!indicatorInitialised || ready != lastReady)
        {
            // Sound when the machine becomes ready again (not at game start).
            if (indicatorInitialised && ready &&
                readyAgainClip != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(readyAgainClip);
            }

            ApplyIndicator(ready);

            indicatorInitialised = true;
            lastReady = ready;
        }

        // Gentle pulse while ready.
        if (indicatorRenderer != null)
        {
            float scale = (pulseWhenReady && ready)
                ? 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount
                : 1f;

            indicatorRenderer.transform.localScale = indicatorBaseScale * scale;
        }
    }

    private void ApplyIndicator(bool ready)
    {
        if (indicatorRenderer != null)
        {
            indicatorRenderer.color = ready ? readyColor : notReadyColor;

            Sprite sprite = ready ? readySprite : notReadySprite;

            if (sprite != null)
                indicatorRenderer.sprite = sprite;
        }

        if (statusText != null)
        {
            statusText.text = ready ? readyMessage : notReadyMessage;
            statusText.color = ready ? readyColor : notReadyColor;
        }
    }

    // ---------------- Spawn point ----------------

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

/*using UnityEngine;

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
}*/
