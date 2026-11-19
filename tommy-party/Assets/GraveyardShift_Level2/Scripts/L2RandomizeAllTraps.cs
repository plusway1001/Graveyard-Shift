using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Randomises EVERY trap in the level (spikes, banana peels, pop-up clowns...).
/// Each trap jumps to a random spot near where it was designed, so it stays in the
/// same room/corridor and never ends up inside a wall. The layout changes at the start
/// and again every time a customer finishes eating. Traps only move while the player
/// is far away, so nothing teleports on screen.
/// Also swaps the banana peel sprite.
///
/// No setup needed: it adds itself to any scene whose name contains "Level 2".
/// (Or add it to an empty GameObject yourself in any scene.)
/// </summary>
public class L2RandomizeAllTraps : MonoBehaviour
{
    [Header("Which objects count as traps (name contains...)")]
    public string[] trapWords = { "spike", "banana", "clown" };

    [Header("Randomising")]
    [Tooltip("How far a trap can move from its designed spot.")]
    public float moveRadius = 4f;
    public float spacing = 2f;
    [Tooltip("Traps only move when the player is at least this far away.")]
    public float playerSafeDistance = 10f;
    public float clearRadius = 0.5f;
    public bool randomizeOnStart = true;
    public bool randomizeAfterServe = true;

    [Header("Banana peel look")]
    public Sprite bananaSprite;

    private class Trap { public Transform t; public Vector2 home; public bool pending; }
    private readonly List<Trap> traps = new List<Trap>();
    private Transform player;

    // ------------------------------------------------------------ auto setup
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        SceneManager.sceneLoaded += (scene, mode) => TryCreate(scene);
        TryCreate(SceneManager.GetActiveScene());
    }

    private static void TryCreate(Scene scene)
    {
        string n = scene.name.Replace(" ", "").Replace("_", "").ToLower();
        if (!n.Contains("level2")) return;
        foreach (var existing in FindObjectsByType<L2RandomizeAllTraps>(FindObjectsSortMode.None))
            if (existing.gameObject.scene == scene) return;

        var go = new GameObject("L2_RandomizeAllTraps");
        SceneManager.MoveGameObjectToScene(go, scene);
        go.AddComponent<L2RandomizeAllTraps>();
    }

    // ------------------------------------------------------------ setup
    private IEnumerator Start()
    {
        yield return null; // let everything else spawn first

        if (bananaSprite == null) bananaSprite = Resources.Load<Sprite>("L2_BananaPeel");
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        // Traps already handled by the Kitchen / Slaughter Room shufflers are left to them.
        var managed = new HashSet<Transform>();
        foreach (L2TrapShuffler s in FindObjectsByType<L2TrapShuffler>(FindObjectsSortMode.None))
            foreach (Transform t in s.traps) if (t != null) managed.Add(t);

        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            if (!IsTrapName(t.name) || HasTrapAncestor(t) || t is RectTransform) continue;

            if (t.name.ToLower().Contains("banana")) ApplyBananaSprite(t);

            if (managed.Contains(t) || IsMover(t)) continue;
            traps.Add(new Trap { t = t, home = t.position, pending = randomizeOnStart });
        }

        if (randomizeAfterServe)
            foreach (TableService service in FindObjectsByType<TableService>(FindObjectsSortMode.None))
                service.onTableCleared.AddListener(RandomizeAll);

        Debug.Log($"[Traps] Randomising {traps.Count} traps around the level.");
    }

    /// <summary>Give every trap a new random spot (applied once the player is far enough away).</summary>
    public void RandomizeAll()
    {
        foreach (Trap trap in traps) trap.pending = true;
    }

    private void Update()
    {
        foreach (Trap trap in traps)
        {
            if (!trap.pending || trap.t == null) continue;
            if (player != null && Vector2.Distance(player.position, trap.t.position) < playerSafeDistance) continue;
            trap.pending = false;
            MoveTrap(trap);
        }
    }

    private void MoveTrap(Trap trap)
    {
        for (int attempt = 0; attempt < 40; attempt++)
        {
            Vector2 p = trap.home + UnityEngine.Random.insideUnitCircle * moveRadius;
            if (!L2RoomBounds.IsFree(p, clearRadius)) continue;
            if (!L2RoomBounds.ClearLine(trap.home, p)) continue;                 // stay in the same room/corridor
            if (player != null && Vector2.Distance(player.position, p) < playerSafeDistance) continue;
            if (TooCloseToOthers(trap, p)) continue;

            trap.t.position = new Vector3(p.x, p.y, trap.t.position.z);
            return;
        }
    }

    private bool TooCloseToOthers(Trap me, Vector2 p)
    {
        foreach (Trap o in traps)
            if (o != me && o.t != null && Vector2.Distance(o.t.position, p) < spacing) return true;
        return false;
    }

    // ------------------------------------------------------------ helpers
    private bool IsTrapName(string name)
    {
        string n = name.ToLower();
        if (n.Contains("waypoint")) return false;
        foreach (string w in trapWords) if (n.Contains(w)) return true;
        return false;
    }

    private bool HasTrapAncestor(Transform t)
    {
        for (Transform p = t.parent; p != null; p = p.parent)
            if (IsTrapName(p.name)) return true;
        return false;
    }

    /// <summary>Traps that follow waypoints or move between points are left alone.</summary>
    private static bool IsMover(Transform t)
    {
        for (Transform x = t; x != null; x = x.parent)
        {
            foreach (MonoBehaviour mb in x.GetComponents<MonoBehaviour>())
            {
                if (mb == null) continue;
                Type type = mb.GetType();
                const BindingFlags F = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                if (type.GetField("waypoints", F) != null) return true;
                FieldInfo mode = type.GetField("movementMode", F);
                if (mode != null && mode.FieldType.IsEnum && Convert.ToInt32(mode.GetValue(mb)) != 0) return true;
            }
        }
        return false;
    }

    private void ApplyBananaSprite(Transform t)
    {
        if (bananaSprite == null) return;
        foreach (Animator a in t.GetComponentsInChildren<Animator>()) a.enabled = false;
        foreach (SpriteRenderer sr in t.GetComponentsInChildren<SpriteRenderer>())
        {
            if (sr.sprite == bananaSprite) continue;
            float oldSize = Mathf.Max(sr.bounds.size.x, sr.bounds.size.y);
            sr.sprite = bananaSprite;
            sr.color = Color.white;
            float newSize = Mathf.Max(sr.bounds.size.x, sr.bounds.size.y);
            if (oldSize > 0.01f && newSize > 0.01f) sr.transform.localScale *= oldSize / newSize * 1.15f;
        }
    }
}
