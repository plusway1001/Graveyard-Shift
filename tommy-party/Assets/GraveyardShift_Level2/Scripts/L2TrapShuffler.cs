using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moves a room's traps to new random spots. Every time a customer is served the
/// layout changes; if the player is inside the room, it waits until they leave.
/// </summary>
public class L2TrapShuffler : MonoBehaviour
{
    public string roomName = "Room";
    public Rect area;
    public List<Transform> traps = new List<Transform>();
    [Tooltip("Spots that must stay clear (doors, dish counter...).")]
    public List<Transform> keepClear = new List<Transform>();
    public float keepClearRadius = 2.5f;
    public float spacing = 2.2f;
    public float wallPadding = 1.4f;
    public float playerClearRadius = 3f;
    public bool shuffleOnStart = true;

    private static readonly List<L2TrapShuffler> rooms = new List<L2TrapShuffler>();
    private bool pending;
    private Transform player;

    private void OnEnable() => rooms.Add(this);
    private void OnDisable() => rooms.Remove(this);

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        if (shuffleOnStart) pending = true;
    }

    /// <summary>Ask every room to change its layout (called when a customer is served).</summary>
    public static void ShuffleAllRooms()
    {
        foreach (L2TrapShuffler r in rooms) r.pending = true;
    }

    private void Update()
    {
        if (!pending) return;
        if (player != null && area.Contains(player.position)) return; // wait until the player leaves
        pending = false;
        Shuffle();
    }

    public void Shuffle()
    {
        Rect r = Rect.MinMaxRect(area.xMin + wallPadding, area.yMin + wallPadding, area.xMax - wallPadding, area.yMax - wallPadding);
        if (r.width <= 0 || r.height <= 0) return;
        Vector2 centre = area.center;
        var placed = new List<Vector2>();

        foreach (Transform t in traps)
        {
            if (t == null) continue;
            for (int attempt = 0; attempt < 80; attempt++)
            {
                Vector2 p = new Vector2(Random.Range(r.xMin, r.xMax), Random.Range(r.yMin, r.yMax));
                if (!L2RoomBounds.IsFree(p, 0.7f)) continue;
                if (!L2RoomBounds.ClearLine(centre, p)) continue;
                if (player != null && Vector2.Distance(player.position, p) < playerClearRadius) continue;
                if (TooClose(p, placed, spacing)) continue;
                bool blocked = false;
                foreach (Transform k in keepClear)
                    if (k != null && Vector2.Distance(k.position, p) < keepClearRadius) { blocked = true; break; }
                if (blocked) continue;

                t.position = new Vector3(p.x, p.y, t.position.z);
                placed.Add(p);
                break;
            }
        }
    }

    private static bool TooClose(Vector2 p, List<Vector2> others, float d)
    {
        foreach (Vector2 o in others) if (Vector2.Distance(o, p) < d) return true;
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.7f);
        Gizmos.DrawWireCube(area.center, area.size);
    }
}
