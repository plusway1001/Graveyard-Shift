using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Seats ghost customers at random tables when the level starts (no queue).
/// Uses the project's ghost Customer prefab and RestaurantTable.TrySeatCustomer,
/// so serving, eating and cash work exactly like Level 1.
/// </summary>
public class L2SeatedGhosts : MonoBehaviour
{
    [Tooltip("Ghost customer prefab. Leave empty to find it automatically from the customer queue.")]
    public GameObject ghostPrefab;
    public int ghostCount = 4;
    [Tooltip("Seat one ghost per table first (spread around the dining hall).")]
    public bool spreadAcrossTables = false;

    [Header("Ghost look and sound")]
    [Tooltip("ghost_A: sitting and waiting")] public Sprite[] seatedFrames;
    [Tooltip("ghost_B: eating")] public Sprite[] eatingFrames;
    [Tooltip("ghost_C: standing, used when floating away")] public Sprite[] leavingFrames;
    public float seatedFps = 5f;
    public float ghostHeight = 2.4f;
    public AudioClip eatClip;
    public AudioClip leaveClip;

    public L2ServeGoal goal;

    private IEnumerator Start()
    {
        yield return null; // tables run Awake/Start first

        if (ghostPrefab == null) ghostPrefab = FindGhostPrefabInQueue();
        if (ghostPrefab == null)
        {
            Debug.LogError("[Level 2] No ghost customer prefab found. Drag it into L2SeatedGhosts > Ghost Prefab.");
            yield break;
        }

        var tables = new List<RestaurantTable>(FindObjectsByType<RestaurantTable>(FindObjectsSortMode.None));
        Shuffle(tables);

        int placed = 0;
        if (spreadAcrossTables)
        {
            foreach (RestaurantTable t in tables)
            {
                if (placed >= ghostCount) break;
                if (t.OccupiedSeats == 0 && Seat(t)) placed++;
            }
        }

        int guard = 0;
        while (placed < ghostCount && guard++ < 100)
        {
            bool any = false;
            Shuffle(tables);
            foreach (RestaurantTable t in tables)
            {
                if (placed >= ghostCount) break;
                if (!t.IsFull && Seat(t)) { placed++; any = true; }
            }
            if (!any) break;
        }

        if (placed < ghostCount)
            Debug.LogWarning($"[Level 2] Only {placed} seats were free, so the goal is {placed}.");
        if (goal != null) goal.SetTarget(Mathf.Min(goal.target, Mathf.Max(1, placed)));
    }

    private bool Seat(RestaurantTable table)
    {
        GameObject go = Instantiate(ghostPrefab);
        Customer customer = go.GetComponentInChildren<Customer>();
        if (customer == null || !table.TrySeatCustomer(customer))
        {
            Destroy(go);
            return false;
        }

        TableService service = table.GetComponent<TableService>();
        MakeVisual(go, customer, table, service);
        if (goal != null) goal.RegisterSeated(service);
        return true;
    }

    private readonly Dictionary<TableService, List<L2GhostVisual>> visuals = new Dictionary<TableService, List<L2GhostVisual>>();

    private void MakeVisual(GameObject customerGO, Customer customer, RestaurantTable table, TableService service)
    {
        // Hide the prefab's own renderers; the L2GhostVisual is what the player sees.
        foreach (SpriteRenderer r in customerGO.GetComponentsInChildren<SpriteRenderer>(true)) r.enabled = false;

        Transform seat = customer.transform.parent != null ? customer.transform.parent : table.transform;
        string layer = "Default"; int order = 20;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        SpriteRenderer pr = player != null ? player.GetComponentInChildren<SpriteRenderer>() : null;
        if (pr != null) { layer = pr.sortingLayerName; order = pr.sortingOrder; }

        var go = new GameObject("Ghost (seated)");
        var v = go.AddComponent<L2GhostVisual>();
        v.idleFrames = seatedFrames; v.eatingFrames = eatingFrames; v.leavingFrames = leavingFrames;
        v.fps = seatedFps; v.height = ghostHeight; v.eatClip = eatClip; v.leaveClip = leaveClip;
        bool faceLeft = seat.position.x > table.transform.position.x; // ghost_A faces right
        v.Init(seat, service, faceLeft, layer, order);

        if (service == null) return;
        if (!visuals.TryGetValue(service, out var list))
        {
            list = new List<L2GhostVisual>();
            visuals[service] = list;
            service.onTableCleared.AddListener(() =>
            {
                foreach (L2GhostVisual g in list) if (g != null) g.Leave();
                list.Clear();
            });
        }
        list.Add(v);
    }


    /// <summary>Looks through the customer queue's fields for a prefab whose Customer type is Ghost.</summary>
    private static GameObject FindGhostPrefabInQueue()
    {
        foreach (CustomerQueue q in FindObjectsByType<CustomerQueue>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            foreach (FieldInfo f in q.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                object value = f.GetValue(q);
                if (value is IEnumerable<GameObject> list)
                {
                    foreach (GameObject g in list) if (IsGhost(g)) return g;
                }
                else if (value is IEnumerable<Customer> clist)
                {
                    foreach (Customer c in clist) if (c != null && IsGhost(c.gameObject)) return c.gameObject;
                }
                else if (value is GameObject g2 && IsGhost(g2)) return g2;
                else if (value is Customer c2 && c2 != null && IsGhost(c2.gameObject)) return c2.gameObject;
            }
        }
        return null;
    }

    public static bool IsGhost(GameObject g)
    {
        if (g == null) return false;
        Customer c = g.GetComponentInChildren<Customer>(true);
        return c != null && c.Type.ToString().ToLower().Contains("ghost");
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
