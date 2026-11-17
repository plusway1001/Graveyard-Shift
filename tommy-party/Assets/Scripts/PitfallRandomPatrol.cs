using UnityEngine;

/// <summary>
/// Add to a PitfallTrap (or a spike trap) to make it a RANDOM patrolling trap.
/// It picks a random point inside a rectangular area, travels there, waits a
/// moment, then picks another point, and so on.
/// Use this INSTEAD of PitfallMover (not together with it).
/// Tip: give the object a Rigidbody2D set to Kinematic so its trigger is detected reliably.
/// </summary>
public class PitfallRandomPatrol : MonoBehaviour
{
    [Header("Patrol Area")]
    [Tooltip("Centre of the area, relative to where this object starts.")]
    [SerializeField] private Vector2 areaOffset = Vector2.zero;
    [SerializeField] private Vector2 areaSize = new Vector2(8f, 5f);

    [Header("Movement")]
    [SerializeField, Min(0.1f)] private float speed = 2f;

    [Tooltip("Random pause at each point (seconds).")]
    [SerializeField, Min(0f)] private float minWait = 0.3f;
    [SerializeField, Min(0f)] private float maxWait = 1.2f;

    [Tooltip("New targets are at least this far away, so it never twitches in place.")]
    [SerializeField, Min(0f)] private float minTravelDistance = 2f;

    [Header("Avoid Walls (optional)")]
    [Tooltip("Targets are never picked inside these layers (e.g. Walls).")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField, Min(0.05f)] private float clearRadius = 0.8f;

    private Rigidbody2D rb;
    private Vector2 areaCenter;
    private Vector2 target;
    private float waitTimer;
    private bool ready;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Fix the area in the world so it does not slide along with the trap.
        areaCenter = (Vector2)transform.position + areaOffset;

        target = transform.position;
        PickNewTarget();
        ready = true;
    }

    private void OnValidate()
    {
        if (maxWait < minWait)
            maxWait = minWait;
    }

    private void FixedUpdate()
    {
        if (!ready)
            return;

        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector2 current = rb != null ? rb.position : (Vector2)transform.position;

        Vector2 next = Vector2.MoveTowards(
            current,
            target,
            speed * Time.fixedDeltaTime
        );

        if (rb != null)
            rb.MovePosition(next);
        else
            transform.position = next;

        if (Vector2.Distance(next, target) < 0.05f)
        {
            waitTimer = Random.Range(minWait, maxWait);
            PickNewTarget();
        }
    }

    private void PickNewTarget()
    {
        Vector2 current = rb != null ? rb.position : (Vector2)transform.position;
        Vector2 candidate = current;

        for (int i = 0; i < 25; i++)
        {
            candidate = areaCenter + new Vector2(
                Random.Range(-areaSize.x / 2f, areaSize.x / 2f),
                Random.Range(-areaSize.y / 2f, areaSize.y / 2f)
            );

            bool farEnough = Vector2.Distance(candidate, current) >= minTravelDistance;

            bool clear = obstacleMask.value == 0 ||
                         Physics2D.OverlapCircle(candidate, clearRadius, obstacleMask) == null;

            if (farEnough && clear)
            {
                target = candidate;
                return;
            }
        }

        // No ideal point found: just use the last one so it keeps moving.
        target = candidate;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 center = Application.isPlaying
            ? areaCenter
            : (Vector2)transform.position + areaOffset;

        Gizmos.color = new Color(1f, 0.6f, 0f);
        Gizmos.DrawWireCube(center, new Vector3(areaSize.x, areaSize.y, 0f));

        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, target);
            Gizmos.DrawWireSphere(target, 0.2f);
        }
    }
}
