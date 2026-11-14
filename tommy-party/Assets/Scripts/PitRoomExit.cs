using UnityEngine;

/// <summary>
/// Put on an exit/ladder object inside the store room (needs a trigger Collider2D).
/// The player has to reach it to climb out and get teleported back.
/// Place it far from where the player lands to waste their time.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PitRoomExit : MonoBehaviour
{
    [Header("Return")]
    [Tooltip("Where the player appears after climbing out.")]
    [SerializeField] private Transform returnPoint;

    [Tooltip("Ignore the return point and send the player back to where they fell.")]
    [SerializeField] private bool returnToFallPosition = false;

    [Header("Time Waster")]
    [Tooltip("The exit stays locked for this many seconds after the player lands.")]
    [SerializeField, Min(0f)] private float minimumStayTime = 0f;

    [Header("Interaction")]
    [Tooltip("Ticked: press the key at the exit. Unticked: leaves automatically on touch.")]
    [SerializeField] private bool requireInteract = true;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Audio (optional)")]
    [SerializeField] private AudioClip exitClip;

    private Transform player;
    private int playerColliderCount;

    private bool IsUnlocked =>
        Time.time - PlayerTeleporter.LastTeleportTime >= minimumStayTime;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Update()
    {
        if (player == null || !IsUnlocked)
            return;

        if (!requireInteract || Input.GetKeyDown(interactKey))
            Exit();
    }

    // Also callable from a mobile UI button.
    public void Interact()
    {
        if (player != null && IsUnlocked)
            Exit();
    }

    private void Exit()
    {
        Vector2 destination;

        if (!returnToFallPosition && returnPoint != null)
            destination = returnPoint.position;
        else
            destination = PitfallTrap.LastFallPosition;

        PlayerTeleporter.Teleport(player, destination);

        if (exitClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(exitClip);

        player = null;
        playerColliderCount = 0;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerColliderCount++;

        player = other.attachedRigidbody != null
            ? other.attachedRigidbody.transform
            : other.transform;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerColliderCount = Mathf.Max(0, playerColliderCount - 1);

        if (playerColliderCount == 0)
            player = null;
    }
}
