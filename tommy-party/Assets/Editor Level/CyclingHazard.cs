using UnityEngine;

/// <summary>
/// Simple timed hazard used for the Pop-up Clown and the Oven Fire.
/// It cycles between INACTIVE (faded, harmless) and ACTIVE (solid, hurts the player).
/// Put it on an object with a trigger Collider2D and a SpriteRenderer.
/// </summary>
public class CyclingHazard : MonoBehaviour
{
    [Header("Timing (seconds)")]
    [SerializeField, Min(0.1f)] private float activeDuration = 1.5f;
    [SerializeField, Min(0.1f)] private float inactiveDuration = 2.5f;
    [Tooltip("Shifts the cycle so hazards do not all fire at the same time.")]
    [SerializeField, Min(0f)] private float startOffset = 0f;

    [Header("Damage")]
    [SerializeField, Min(1)] private int damage = 1;
    [SerializeField, Min(0.1f)] private float damageInterval = 1f;

    [Header("Look")]
    [SerializeField, Range(0f, 1f)] private float inactiveAlpha = 0.25f;

    private SpriteRenderer[] renderers;
    private Collider2D hitbox;
    private bool isActive;
    private bool initialised;
    private float nextDamageTime;

    private void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();
        hitbox = GetComponent<Collider2D>();
    }

    private void Update()
    {
        float cycle = activeDuration + inactiveDuration;
        float t = (Time.time + startOffset) % cycle;

        // First part of the cycle is inactive, the rest is active.
        bool shouldBeActive = t >= inactiveDuration;

        if (!initialised || shouldBeActive != isActive)
        {
            initialised = true;
            SetActive(shouldBeActive);
        }
    }

    private void SetActive(bool active)
    {
        isActive = active;

        if (hitbox != null)
            hitbox.enabled = active;

        foreach (SpriteRenderer sr in renderers)
        {
            Color c = sr.color;
            c.a = active ? 1f : inactiveAlpha;
            sr.color = c;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isActive || !other.CompareTag("Player"))
            return;

        if (Time.time < nextDamageTime)
            return;

        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();

        if (health == null)
            return;

        nextDamageTime = Time.time + damageInterval;

        health.TakeProjectileDamage(damage);
    }
}
