using UnityEngine;

/// <summary>
/// Small helper used by the pitfall and the store room exit
/// to move the player instantly and safely.
/// </summary>
public static class PlayerTeleporter
{
    public static float LastTeleportTime { get; private set; } = -100f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        LastTeleportTime = -100f;
    }

    public static void Teleport(Transform player, Vector2 position)
    {
        if (player == null)
            return;

        player.position = new Vector3(position.x, position.y, player.position.z);

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.position = position;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        Physics2D.SyncTransforms();
        LastTeleportTime = Time.time;
    }
}
