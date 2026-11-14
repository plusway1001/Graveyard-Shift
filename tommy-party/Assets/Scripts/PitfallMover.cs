using UnityEngine;

/// <summary>
/// Add to a PitfallTrap object to make it a MOVING pitfall.
/// It travels between the waypoints you set.
/// Leave this off for a static pitfall.
/// Tip: give the pit a Rigidbody2D set to Kinematic so its trigger is detected reliably.
/// Do NOT make the waypoints children of the pit.
/// </summary>
public class PitfallMover : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    [SerializeField, Min(0.1f)] private float speed = 2f;
    [SerializeField, Min(0f)] private float waitAtPoint = 0.5f;

    [Tooltip("Ticked: A -> B -> C -> B -> A. Unticked: A -> B -> C -> A.")]
    [SerializeField] private bool pingPong = true;

    private Rigidbody2D rb;
    private Vector2[] points;
    private int index;
    private int direction = 1;
    private float waitTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (waypoints == null || waypoints.Length < 2)
        {
            Debug.LogWarning(name + ": Needs at least 2 waypoints to move.", this);
            enabled = false;
            return;
        }

        // Copy positions so the route stays fixed even if the waypoints move.
        points = new Vector2[waypoints.Length];

        for (int i = 0; i < waypoints.Length; i++)
            points[i] = waypoints[i].position;

        // Start at the first waypoint and head for the second.
        transform.position = points[0];
        index = 1;
    }

    private void FixedUpdate()
    {
        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector2 current = rb != null ? rb.position : (Vector2)transform.position;

        Vector2 next = Vector2.MoveTowards(
            current,
            points[index],
            speed * Time.fixedDeltaTime
        );

        if (rb != null)
            rb.MovePosition(next);
        else
            transform.position = next;

        if (Vector2.Distance(next, points[index]) < 0.01f)
        {
            waitTimer = waitAtPoint;
            AdvanceIndex();
        }
    }

    private void AdvanceIndex()
    {
        if (pingPong)
        {
            if (index + direction < 0 || index + direction >= points.Length)
                direction = -direction;

            index += direction;
        }
        else
        {
            index = (index + 1) % points.Length;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (waypoints == null)
            return;

        Gizmos.color = Color.yellow;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null)
                continue;

            Gizmos.DrawWireSphere(waypoints[i].position, 0.2f);

            if (i + 1 < waypoints.Length && waypoints[i + 1] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }
    }
}
