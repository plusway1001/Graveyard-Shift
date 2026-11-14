using System.Collections;
using UnityEngine;

/// <summary>
/// Put on a pitfall object with a trigger Collider2D.
/// When the player touches it, they fall and get teleported
/// to the store room (pitRoomSpawn).
/// Works for both static pits and moving pits (add PitfallMover to move it).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PitfallTrap : MonoBehaviour
{
    // Where the player was standing when they fell.
    public static Vector2 LastFallPosition { get; private set; }

    [Header("Destination")]
    [Tooltip("Empty object placed inside the store room where the player lands.")]
    [SerializeField] private Transform pitRoomSpawn;

    [Header("Fall Settings")]
    [Tooltip("Seconds the player is frozen before being teleported.")]
    [SerializeField, Min(0f)] private float fallDelay = 0.4f;

    [Tooltip("Ignore the pit for this long after any teleport.")]
    [SerializeField, Min(0f)] private float retriggerCooldown = 0.5f;

    [Tooltip("Tick if the player can dash over the pit safely.")]
    [SerializeField] private bool ignoreWhileDashing = false;

    [Header("Audio (optional)")]
    [SerializeField] private AudioClip fallClip;

    private bool isFalling;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    // Stay (not Enter) so a player who ends a dash on top of the pit still falls.
    private void OnTriggerStay2D(Collider2D other)
    {
        if (isFalling || !other.CompareTag("Player"))
            return;

        if (Time.time - PlayerTeleporter.LastTeleportTime < retriggerCooldown)
            return;

        Transform player = other.attachedRigidbody != null
            ? other.attachedRigidbody.transform
            : other.transform;

        if (ignoreWhileDashing)
        {
            PlayerDash dash = player.GetComponent<PlayerDash>();

            if (dash != null && dash.IsDashing)
                return;
        }

        StartCoroutine(FallRoutine(player));
    }

    private IEnumerator FallRoutine(Transform player)
    {
        isFalling = true;

        if (pitRoomSpawn == null)
        {
            Debug.LogWarning(name + ": Pit room spawn is not assigned.", this);
            isFalling = false;
            yield break;
        }

        LastFallPosition = player.position;

        // Freeze the player while falling.
        PlayerMovement movement = player.GetComponent<PlayerMovement>();

        if (movement != null)
            movement.enabled = false;

        if (fallClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(fallClip);

        yield return new WaitForSeconds(fallDelay);

        if (player != null)
            PlayerTeleporter.Teleport(player, pitRoomSpawn.position);

        if (movement != null)
            movement.enabled = true;

        isFalling = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (pitRoomSpawn == null)
            return;

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, pitRoomSpawn.position);
        Gizmos.DrawWireSphere(pitRoomSpawn.position, 0.3f);
    }
}
