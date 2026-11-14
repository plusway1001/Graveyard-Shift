using UnityEngine;

/// <summary>
/// Put on the customer prefab (next to Customer).
/// Only does anything for Ghoul customers. A seated Ghoul throws sticky mud at the
/// player at a fixed interval while the player is close to it.
/// It stops for good once the Ghoul has been served.
/// </summary>
[RequireComponent(typeof(Customer))]
public class GhoulMudThrower : MonoBehaviour
{
    [Header("Mud")]
    [SerializeField] private StickyMudProjectile mudPrefab;
    [Tooltip("Where the mud spawns. Uses this object's position if empty.")]
    [SerializeField] private Transform firePoint;

    [Header("Behaviour")]
    [Tooltip("The Ghoul only throws while the player is within this distance.")]
    [SerializeField, Min(0.5f)] private float throwRange = 4f;
    [SerializeField, Min(0.2f)] private float throwInterval = 2.5f;
    [Tooltip("Delay before the first throw after the player walks into range.")]
    [SerializeField, Min(0f)] private float firstThrowDelay = 0.6f;

    [Header("Audio (optional)")]
    [SerializeField] private AudioClip throwClip;

    private Customer customer;
    private Transform player;
    private float throwTimer;

    private void Awake()
    {
        customer = GetComponent<Customer>();
        throwTimer = firstThrowDelay;
    }

    private void Update()
    {
        // Only seated, unserved Ghouls throw mud.
        if (customer.Type != CustomerType.Ghoul ||
            !customer.IsSeated ||
            customer.HasBeenServed)
        {
            return;
        }

        if (!FindPlayer())
            return;

        float distance = Vector2.Distance(transform.position, player.position);

        // Player walked away: reset so there is a short warning when they return.
        if (distance > throwRange)
        {
            throwTimer = firstThrowDelay;
            return;
        }

        throwTimer -= Time.deltaTime;

        if (throwTimer <= 0f)
        {
            ThrowMud();
            throwTimer = throwInterval;
        }
    }

    private bool FindPlayer()
    {
        if (player != null)
            return true;

        GameObject found = GameObject.FindGameObjectWithTag("Player");

        if (found != null)
            player = found.transform;

        return player != null;
    }

    private void ThrowMud()
    {
        if (mudPrefab == null)
        {
            Debug.LogWarning(name + ": Mud prefab is not assigned.", this);
            return;
        }

        Vector3 spawnPos =
            firePoint != null ? firePoint.position : transform.position;

        StickyMudProjectile mud =
            Instantiate(mudPrefab, spawnPos, Quaternion.identity);

        mud.Launch(player);

        if (throwClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(throwClip);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.8f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, throwRange);
    }
}
