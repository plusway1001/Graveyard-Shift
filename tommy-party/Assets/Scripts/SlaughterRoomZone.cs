using UnityEngine;

/// <summary>
/// Put on an empty GameObject with a trigger Collider2D covering the slaughter room.
/// When the player enters, the enemies start chasing and shooting.
/// When the player leaves, they stop and go back to their start positions.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SlaughterRoomZone : MonoBehaviour
{
    [SerializeField] private RiotEnemy[] enemies;

    private int playerColliderCount;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        // The player may have several colliders; only react to the first one.
        playerColliderCount++;
        if (playerColliderCount != 1)
            return;

        Transform player = other.attachedRigidbody != null
            ? other.attachedRigidbody.transform
            : other.transform;

        foreach (RiotEnemy enemy in enemies)
        {
            if (enemy != null)
                enemy.StartChasing(player);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerColliderCount = Mathf.Max(0, playerColliderCount - 1);
        if (playerColliderCount > 0)
            return;

        foreach (RiotEnemy enemy in enemies)
        {
            if (enemy != null)
                enemy.StopChasing();
        }
    }
}
