using UnityEngine;

/// <summary>
/// Static spike. Put on a spike object with a trigger Collider2D.
/// Damages the player when touched, then again every damageInterval
/// seconds while the player keeps standing on it.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SpikeTrapper : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField, Min(1)] private int damage = 1;

    [Tooltip("Seconds between hits while the player stays on the spikes.")]
    [SerializeField, Min(0.1f)] private float damageInterval = 1f;

    [Tooltip("Tick if the player can dash over the spikes without being hurt.")]
    [SerializeField] private bool ignoreWhileDashing = false;

    [Header("Audio (optional)")]
    [SerializeField] private AudioClip hitClip;

    private float nextDamageTime;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    // Stay (not Enter) so standing on the spikes keeps hurting, at the interval above.
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (Time.time < nextDamageTime)
            return;

        if (ignoreWhileDashing)
        {
            PlayerDash dash = other.GetComponentInParent<PlayerDash>();

            if (dash != null && dash.IsDashing)
                return;
        }

        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();

        if (health == null)
            return;

        nextDamageTime = Time.time + damageInterval;

        health.TakeProjectileDamage(damage);

        if (hitClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(hitClip);
    }
}