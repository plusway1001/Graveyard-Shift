using UnityEngine;

/// <summary>
/// Riot enemy with two states:
///   Idle     - waits at (or returns to) its start position.
///   Chasing  - follows the player, stops at stopDistance and keeps
///              throwing projectiles at a fixed interval.
/// Started/stopped by SlaughterRoomZone when the player enters/leaves the room.
/// </summary>
public class RiotEnemy : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Chasing
    }

    [Header("Movement")]
    [SerializeField, Min(0f)] private float chaseSpeed = 2.5f;
    [SerializeField, Min(0f)] private float returnSpeed = 4f;

    [Tooltip("The enemy stops this far away from the player while shooting.")]
    [SerializeField, Min(0f)] private float stopDistance = 4f;

    [Tooltip("Tick to snap back to the start position instantly instead of walking back.")]
    [SerializeField] private bool teleportBackToStart = false;

    [Header("Shooting")]
    [SerializeField] private RiotProjectile projectilePrefab;
    [Tooltip("Where projectiles spawn. Uses this object's position if empty.")]
    [SerializeField] private Transform firePoint;
    [SerializeField, Min(0.1f)] private float shootInterval = 2f;
    [Tooltip("Delay before the first shot after the player enters the room.")]
    [SerializeField, Min(0f)] private float firstShotDelay = 0.5f;
    [Tooltip("Tick to only shoot once the enemy has reached its stop distance.")]
    [SerializeField] private bool onlyShootWhenInRange = false;

    [Header("Audio (optional)")]
    [SerializeField] private AudioClip shootClip;

    public EnemyState State { get; private set; } = EnemyState.Idle;

    private Rigidbody2D rb;
    private Transform target;
    private Vector2 startPosition;
    private float shootTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // The enemy always starts (and returns to) this fixed position.
        startPosition = transform.position;
    }

    private void Update()
    {
        if (State != EnemyState.Chasing || target == null)
            return;

        if (onlyShootWhenInRange && DistanceToTarget() > stopDistance + 0.1f)
            return;

        shootTimer -= Time.deltaTime;

        if (shootTimer <= 0f)
        {
            Shoot();
            shootTimer = shootInterval;
        }
    }

    private void FixedUpdate()
    {
        switch (State)
        {
            case EnemyState.Chasing:
                ChaseTarget();
                break;

            case EnemyState.Idle:
                ReturnToStart();
                break;
        }
    }

    // ---------------- State control ----------------

    public void StartChasing(Transform player)
    {
        if (player == null)
            return;

        target = player;
        State = EnemyState.Chasing;
        shootTimer = firstShotDelay;
    }

    public void StopChasing()
    {
        target = null;
        State = EnemyState.Idle;

        if (teleportBackToStart)
            SetPosition(startPosition);
    }

    // ---------------- Behaviour ----------------

    private void ChaseTarget()
    {
        if (target == null)
            return;

        Vector2 current = GetPosition();
        Vector2 toPlayer = (Vector2)target.position - current;

        // Stand still once within stop distance.
        if (toPlayer.magnitude <= stopDistance)
            return;

        // Walk to the point that is stopDistance away from the player.
        Vector2 stopPoint =
            (Vector2)target.position - toPlayer.normalized * stopDistance;

        MoveTowards(stopPoint, chaseSpeed);
    }

    private void ReturnToStart()
    {
        if (Vector2.Distance(GetPosition(), startPosition) < 0.01f)
            return;

        MoveTowards(startPosition, returnSpeed);
    }

    private void Shoot()
    {
        if (projectilePrefab == null || target == null)
            return;

        Vector3 spawnPos =
            firePoint != null ? firePoint.position : transform.position;

        RiotProjectile projectile =
            Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        // RiotProjectile.Launch aims at the player and sets its velocity.
        projectile.Launch(target);

        if (shootClip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(shootClip);
    }

    // ---------------- Helpers ----------------

    private float DistanceToTarget()
    {
        return Vector2.Distance(GetPosition(), target.position);
    }

    private Vector2 GetPosition()
    {
        return rb != null ? rb.position : (Vector2)transform.position;
    }

    private void SetPosition(Vector2 position)
    {
        if (rb != null)
            rb.position = position;
        else
            transform.position = position;
    }

    private void MoveTowards(Vector2 destination, float speed)
    {
        Vector2 next = Vector2.MoveTowards(
            GetPosition(),
            destination,
            speed * Time.fixedDeltaTime
        );

        if (rb != null)
            rb.MovePosition(next);
        else
            transform.position = next;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}
