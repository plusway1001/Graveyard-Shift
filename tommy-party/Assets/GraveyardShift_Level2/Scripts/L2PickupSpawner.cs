using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Keeps a number of pickups (brains, hearts...) scattered around a room,
/// and tops them up after the player collects them.
/// </summary>
public class L2PickupSpawner : MonoBehaviour
{
    [Header("What to spawn")]
    public GameObject pickupPrefab;
    [Tooltip("Optional: replace the pickup's sprite (e.g. a brain).")]
    public Sprite overrideSprite;
    [Tooltip("World height of the override sprite.")]
    public float overrideHeight = 0.9f;

    [Header("Where")]
    [Tooltip("A point inside the room. The room size is measured from its walls.")]
    public Transform roomCenter;
    [Tooltip("Keep pickups this far from the walls.")]
    public float wallPadding = 1.2f;
    [Tooltip("If the room can't be measured, use this size around the centre.")]
    public Vector2 fallbackSize = new Vector2(12f, 8f);

    [Tooltip("Optional: spawn near these (e.g. the riot zombies) instead of anywhere in the room.")]
    public Transform[] spawnAround;
    public Vector2 aroundRadius = new Vector2(1.3f, 3f);
    [Tooltip("If Spawn Around is empty, use objects whose name contains all of these words (comma separated) within Anchor Search Radius.")]
    public string autoFindAnchorsNamed = "";
    public float anchorSearchRadius = 25f;

    [Header("How many")]
    public int maxAlive = 6;
    public float respawnDelay = 4f;
    public float minSpacing = 1.6f;
    public float clearRadius = 0.4f;

    private readonly List<GameObject> alive = new List<GameObject>();
    private Rect area;
    private Vector2 center;

    private IEnumerator Start()
    {
        yield return null; // let the scene settle
        center = roomCenter != null ? (Vector2)roomCenter.position : (Vector2)transform.position;
        if ((spawnAround == null || spawnAround.Length == 0) && !string.IsNullOrEmpty(autoFindAnchorsNamed))
            spawnAround = FindAnchors();
        area = ComputeArea();

        for (int i = 0; i < maxAlive; i++) SpawnOne();

        while (true)
        {
            yield return new WaitForSeconds(respawnDelay);
            alive.RemoveAll(g => g == null || !g.activeInHierarchy);
            if (alive.Count < maxAlive) SpawnOne();
        }
    }

    private Rect ComputeArea()
    {
        Rect r = L2RoomBounds.Measure(center);
        if (r.width < 3f || r.height < 3f || r.width > 60f || r.height > 60f)
            r = new Rect(center - fallbackSize * 0.5f, fallbackSize);

        r.xMin += wallPadding; r.xMax -= wallPadding;
        r.yMin += wallPadding; r.yMax -= wallPadding;
        return r;
    }

    private void SpawnOne()
    {
        if (pickupPrefab == null) return;

        for (int attempt = 0; attempt < 40; attempt++)
        {
            Vector2 from = center;
            Vector2 p;
            Transform anchor = RandomAnchor();
            if (anchor != null)
            {
                from = anchor.position;
                p = from + Random.insideUnitCircle.normalized * Random.Range(aroundRadius.x, aroundRadius.y);
            }
            else
            {
                p = new Vector2(Random.Range(area.xMin, area.xMax), Random.Range(area.yMin, area.yMax));
            }
            if (!L2RoomBounds.IsFree(p, clearRadius)) continue;
            if (!L2RoomBounds.ClearLine(from, p)) continue;
            if (TooClose(p)) continue;

            GameObject go = Instantiate(pickupPrefab, p, Quaternion.identity);
            go.name = pickupPrefab.name + " (spawned)";
            ApplySprite(go);
            alive.Add(go);
            return;
        }
    }

    private Transform[] FindAnchors()
    {
        string[] words = autoFindAnchorsNamed.ToLower().Split(',');
        var found = new List<Transform>();
        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            string n = t.name.ToLower();
            bool match = true;
            foreach (string w in words) if (!n.Contains(w.Trim())) { match = false; break; }
            if (match && Vector2.Distance(t.position, center) < anchorSearchRadius) found.Add(t);
        }
        return found.ToArray();
    }

    private Transform RandomAnchor()
    {
        if (spawnAround == null || spawnAround.Length == 0) return null;
        var live = new List<Transform>();
        foreach (Transform t in spawnAround) if (t != null && t.gameObject.activeInHierarchy) live.Add(t);
        return live.Count > 0 ? live[Random.Range(0, live.Count)] : null;
    }

    private bool TooClose(Vector2 p)
    {
        foreach (GameObject g in alive)
            if (g != null && Vector2.Distance(g.transform.position, p) < minSpacing) return true;
        return false;
    }

    private void ApplySprite(GameObject go)
    {
        if (overrideSprite == null) return;
        SpriteRenderer sr = go.GetComponentInChildren<SpriteRenderer>();
        if (sr == null) return;

        Animator anim = sr.GetComponent<Animator>();
        if (anim != null) anim.enabled = false;

        sr.sprite = overrideSprite;
        float h = sr.bounds.size.y;
        if (h > 0.001f) sr.transform.localScale *= overrideHeight / h;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.6f, 0.7f, 0.6f);
        if (Application.isPlaying) Gizmos.DrawWireCube(area.center, area.size);
        else if (roomCenter != null) Gizmos.DrawWireSphere(roomCenter.position, 1f);
    }
}
