using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Slowly follows the player while the player is inside an area (used for the kitchen pitfalls).
/// Stays inside the area and keeps some distance from the other chasers.
/// </summary>
public class L2Chaser : MonoBehaviour
{
    public float speed = 1.6f;
    public Rect area;
    public float keepApart = 2.4f;
    [Tooltip("Stop moving for a moment after the player teleports (e.g. falls into a pit).")]
    public float pauseAfterTeleport = 3f;

    private static readonly List<L2Chaser> chasers = new List<L2Chaser>();
    private Transform player;
    private Vector3 lastPlayerPos;
    private float pause;

    private void OnEnable() => chasers.Add(this);
    private void OnDisable() => chasers.Remove(this);

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) { player = p.transform; lastPlayerPos = player.position; }
    }

    private void Update()
    {
        if (player == null) return;

        Vector3 pp = player.position;
        if ((pp - lastPlayerPos).sqrMagnitude > 36f) pause = pauseAfterTeleport; // teleported
        lastPlayerPos = pp;
        if (pause > 0f) { pause -= Time.deltaTime; return; }
        if (!area.Contains(pp)) return;

        Vector2 pos = transform.position;
        Vector2 toPlayer = (Vector2)pp - pos;
        if (toPlayer.sqrMagnitude < 0.0025f) return;

        Vector2 next = pos + toPlayer.normalized * speed * Time.deltaTime;
        foreach (L2Chaser other in chasers)
        {
            if (other == this) continue;
            Vector2 away = next - (Vector2)other.transform.position;
            float d = away.magnitude;
            if (d > 0.001f && d < keepApart) next += away / d * (keepApart - d) * 0.5f;
        }

        next.x = Mathf.Clamp(next.x, area.xMin, area.xMax);
        next.y = Mathf.Clamp(next.y, area.yMin, area.yMax);
        transform.position = new Vector3(next.x, next.y, transform.position.z);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        Gizmos.DrawWireCube(area.center, area.size);
    }
}
