
using System.Collections;
using UnityEngine;

public enum SpikeMovementMode
{
    Static,
    PointAToB,
    RandomPatrol
}

public class SpikeTrap : MonoBehaviour
{
    [Header("Spike Cycle")]
    [SerializeField] private bool cycleSpikes = true;
    [SerializeField] private float spikesUpDuration = 1.5f;
    [SerializeField] private float spikesDownDuration = 1.5f;
    [SerializeField] private bool startUp = false;

    [SerializeField] private Collider2D damageCollider;
    [SerializeField] private Transform spikeVisual;
    [SerializeField] private Vector3 upLocalPosition;
    [SerializeField] private Vector3 downLocalPosition;

    [Header("Movement Mode")]
    [SerializeField]
    private SpikeMovementMode movementMode =
        SpikeMovementMode.Static;

    [Header("Point A to B Movement")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float pointMoveSpeed = 2f;

    [Header("Random Patrol Movement")]
    [SerializeField] private Transform patrolAreaCenter;
    [SerializeField] private Vector2 patrolAreaSize = new Vector2(5f, 5f);
    [SerializeField] private float patrolMoveSpeed = 2f;
    [SerializeField] private float waitAtDestination = 0.5f;
    [SerializeField] private float arrivalDistance = 0.05f;

    [Header("Movement Physics")]
    [SerializeField] private Rigidbody2D rb;

    private bool isUp;
    private bool movingToPointB = true;

    private Vector2 targetPosition;
    private Vector2 patrolCenterPosition;

    private float waitTimer;

    public bool IsUp => isUp;

    private void Start()
    {
        if (damageCollider == null)
            damageCollider = GetComponent<Collider2D>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        // Initialize spike state.
        isUp = cycleSpikes ? startUp : true;

        ApplyState();

        // Start spike up/down cycle.
        if (cycleSpikes)
            StartCoroutine(SpikeCycle());

        // Initialize movement.
        switch (movementMode)
        {
            case SpikeMovementMode.Static:
                break;

            case SpikeMovementMode.PointAToB:

                if (pointA != null)
                {
                    if (rb != null)
                        rb.position = pointA.position;
                    else
                        transform.position = pointA.position;
                }

                movingToPointB = true;
                break;

            case SpikeMovementMode.RandomPatrol:

                patrolCenterPosition = patrolAreaCenter != null
                    ? (Vector2)patrolAreaCenter.position
                    : (Vector2)transform.position;

                ChooseRandomDestination();
                break;
        }
    }

    private void FixedUpdate()
    {
        switch (movementMode)
        {
            case SpikeMovementMode.Static:
                break;

            case SpikeMovementMode.PointAToB:
                MoveBetweenPoints();
                break;

            case SpikeMovementMode.RandomPatrol:
                PatrolMovement();
                break;
        }
    }

    // --------------------------------------------------
    // MOVEMENT
    // --------------------------------------------------

    private void MoveTo(Vector2 destination, float speed)
    {
        Vector2 currentPosition = rb != null
            ? rb.position
            : (Vector2)transform.position;

        Vector2 newPosition = Vector2.MoveTowards(
            currentPosition,
            destination,
            speed * Time.fixedDeltaTime
        );

        if (rb != null)
        {
            rb.MovePosition(newPosition);
        }
        else
        {
            transform.position = new Vector3(
                newPosition.x,
                newPosition.y,
                transform.position.z
            );
        }
    }

    // Point A to B movement.
    private void MoveBetweenPoints()
    {
        if (pointA == null || pointB == null)
            return;

        Vector2 destination = movingToPointB
            ? (Vector2)pointB.position
            : (Vector2)pointA.position;

        MoveTo(destination, pointMoveSpeed);

        Vector2 currentPosition = rb != null
            ? rb.position
            : (Vector2)transform.position;

        if (Vector2.Distance(currentPosition, destination)
            <= arrivalDistance)
        {
            movingToPointB = !movingToPointB;
        }
    }

    // Random patrol movement.
    private void PatrolMovement()
    {
        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            return;
        }

        MoveTo(targetPosition, patrolMoveSpeed);

        Vector2 currentPosition = rb != null
            ? rb.position
            : (Vector2)transform.position;

        if (Vector2.Distance(currentPosition, targetPosition)
            <= arrivalDistance)
        {
            waitTimer = waitAtDestination;
            ChooseRandomDestination();
        }
    }

    // Select a random location inside the patrol area.
    private void ChooseRandomDestination()
    {
        float randomX = Random.Range(
            -patrolAreaSize.x / 2f,
            patrolAreaSize.x / 2f
        );

        float randomY = Random.Range(
            -patrolAreaSize.y / 2f,
            patrolAreaSize.y / 2f
        );

        targetPosition = patrolCenterPosition +
                         new Vector2(randomX, randomY);
    }

    // --------------------------------------------------
    // SPIKE UP / DOWN CYCLE
    // --------------------------------------------------

    private IEnumerator SpikeCycle()
    {
        while (true)
        {
            float duration = isUp
                ? spikesUpDuration
                : spikesDownDuration;

            yield return new WaitForSeconds(duration);

            isUp = !isUp;

            ApplyState();
        }
    }

    private void ApplyState()
    {
        if (damageCollider != null)
            damageCollider.enabled = isUp;

        if (spikeVisual != null)
        {
            spikeVisual.localPosition = isUp
                ? upLocalPosition
                : downLocalPosition;
        }
    }

    // --------------------------------------------------
    // PLAYER DAMAGE
    // --------------------------------------------------


    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isUp)
            return;

        // Find the PlayerDash script using the collider's
        // attached Rigidbody2D first.
        PlayerDash dash = null;

        if (other.attachedRigidbody != null)
        {
            dash = other.attachedRigidbody.GetComponent<PlayerDash>();
        }

        // Fallback: search the collider's parent hierarchy.
        if (dash == null)
        {
            dash = other.GetComponentInParent<PlayerDash>();
        }

        // Never damage the player during an active dash.
        if (dash != null && dash.IsDashing)
            return;

        // Find PlayerHealth.
        PlayerHealth health = null;

        if (other.attachedRigidbody != null)
        {
            health = other.attachedRigidbody.GetComponent<PlayerHealth>();
        }

        if (health == null)
        {
            health = other.GetComponentInParent<PlayerHealth>();
        }

        if (health == null)
            return;

        // Apply damage.
        health.TakeDamage(1);
    }


    // --------------------------------------------------
    // SCENE VIEW PATROL AREA
    // --------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        if (movementMode != SpikeMovementMode.RandomPatrol)
            return;

        Vector3 centre = patrolAreaCenter != null
            ? patrolAreaCenter.position
            : transform.position;

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireCube(
            centre,
            new Vector3(
                patrolAreaSize.x,
                patrolAreaSize.y,
                0.1f
            )
        );
    }
}

/*using System.Collections;
using UnityEngine;

// Static, timed, or moving spike hazard. Damage collider should be a trigger.
public class SpikeTrap : MonoBehaviour
{
    [Header("Spike Cycle")]
    [SerializeField] private bool cycleSpikes = true;
    [SerializeField, Min(0f)] private float spikesUpDuration = 1.5f;
    [SerializeField, Min(0f)] private float spikesDownDuration = 1.5f;
    [SerializeField] private bool startUp = false;
    [SerializeField] private Collider2D damageCollider;
    [SerializeField] private Transform spikeVisual;
    [SerializeField] private Vector3 upLocalPosition;
    [SerializeField] private Vector3 downLocalPosition;

    [Header("Optional Movement")]
    [SerializeField] private bool moveTrap;
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField, Min(0.1f)] private float moveSpeed = 2f;

    private bool isUp;
    private bool movingToB = true;

    private void Start()
    {
        if (damageCollider == null) damageCollider = GetComponent<Collider2D>();
        isUp = cycleSpikes ? startUp : true;
        ApplyState();
        if (cycleSpikes) StartCoroutine(SpikeCycle());
    }

    private void Update()
    {
        if (!moveTrap || pointA == null || pointB == null) return;
        Vector3 target = movingToB ? pointB.position : pointA.position;
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, target) < 0.03f) movingToB = !movingToB;
    }

    private IEnumerator SpikeCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(isUp ? spikesUpDuration : spikesDownDuration);
            isUp = !isUp;
            ApplyState();
        }
    }

    private void ApplyState()
    {
        if (damageCollider != null) damageCollider.enabled = isUp;
        if (spikeVisual != null) spikeVisual.localPosition = isUp ? upLocalPosition : downLocalPosition;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isUp) return;
        PlayerDash dash = other.GetComponentInParent<PlayerDash>();
        if (dash != null && dash.IsDashing) return; // A correctly timed dash dodges the spikes.

        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health != null) health.TakeDamage(1);
    }
}*/
