using System.Collections;
using UnityEngine;

public enum SpikeMovementMode
{
    Static,
    PointAToB,
    RandomPatrol
}

// How the spikes show "up" and "down".
public enum SpikeVisualMode
{
    TransformMove,   // original: moves spikeVisual between two local positions
    SpriteSwap,      // new: changes the sprite on a SpriteRenderer
    Both             // does both at the same time
}

public class SpikeTrap : MonoBehaviour
{
    [Header("Spike Cycle")]
    [SerializeField] private bool cycleSpikes = true;
    [SerializeField] private float spikesUpDuration = 1.5f;
    [SerializeField] private float spikesDownDuration = 1.5f;
    [SerializeField] private bool startUp = false;

    [SerializeField] private Collider2D damageCollider;

    [Header("Up / Down Visual")]
    [SerializeField] private SpikeVisualMode visualMode = SpikeVisualMode.TransformMove;

    [Header("Transform Mode (TransformMove / Both)")]
    [SerializeField] private Transform spikeVisual;
    [SerializeField] private Vector3 upLocalPosition;
    [SerializeField] private Vector3 downLocalPosition;

    [Header("Sprite Mode (SpriteSwap / Both)")]
    [Tooltip("Found automatically on this object or its children if left empty.")]
    [SerializeField] private SpriteRenderer spikeSpriteRenderer;
    [SerializeField] private Sprite upSprite;
    [SerializeField] private Sprite downSprite;

    [Tooltip("Optional. Frames played when the spikes rise (down -> up). Leave empty for an instant swap.")]
    [SerializeField] private Sprite[] risingFrames;
    [Tooltip("Optional. Frames played when the spikes retract (up -> down). Leave empty for an instant swap.")]
    [SerializeField] private Sprite[] fallingFrames;
    [SerializeField, Min(1f)] private float transitionFrameRate = 12f;

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
    private Coroutine spriteRoutine;

    public bool IsUp => isUp;

    private bool UsesTransform =>
        visualMode == SpikeVisualMode.TransformMove || visualMode == SpikeVisualMode.Both;

    private bool UsesSprite =>
        visualMode == SpikeVisualMode.SpriteSwap || visualMode == SpikeVisualMode.Both;

    [Header("Audio (only plays when Cycle Spikes is ON)")]
    [Tooltip("Found automatically on this object if left empty. One is added if none exists.")]
    //[SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip spikeUpClip;
    [SerializeField] private AudioClip spikeDownClip;
    [SerializeField, Range(0f, 1f)] private float soundVolume = 1f;
    [Tooltip("Player must be within this distance for the sound to play.")]
    [SerializeField] private float hearingRadius = 8f;
    [Tooltip("Volume fades out the further the player is from the trap.")]
    [SerializeField] private bool fadeWithDistance = true;

    private Transform playerTransform;

    private void Start()
    {
        /*if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D game: we handle distance ourselves*/

        if (damageCollider == null)
            damageCollider = GetComponent<Collider2D>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (spikeSpriteRenderer == null)
            spikeSpriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Initialize spike state.
        isUp = cycleSpikes ? startUp : true;

        // Set the starting look instantly (no transition animation).
        ApplyState(true);

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

            ApplyState(false);
        }
    }

    // --------------------------------------------------
    // AUDIO
    // --------------------------------------------------

    private void PlaySpikeSound()
    {
        AudioClip clip = isUp ? spikeUpClip : spikeDownClip;

        if (clip == null)
            return;

        if (playerTransform == null)
            FindPlayer();

        if (playerTransform == null)
            return;

        float distance = Vector2.Distance(
            transform.position,
            playerTransform.position
        );

        if (distance > hearingRadius)
            return;

        float volume = soundVolume;

        if (fadeWithDistance)
            volume *= 1f - Mathf.Clamp01(distance / hearingRadius);

        // Create a temporary AudioSource on its own GameObject.
        GameObject tempObject = new GameObject("SpikeSound_Temp");
        tempObject.transform.position = transform.position;

        AudioSource tempSource = tempObject.AddComponent<AudioSource>();
        tempSource.clip = clip;
        tempSource.volume = volume;
        tempSource.spatialBlend = 0f; // 2D: distance is handled above
        tempSource.playOnAwake = false;
        tempSource.Play();

        // Delete the temporary object once the clip has finished.
        Destroy(tempObject, clip.length);
    }

    /*private void PlaySpikeSound()
    {
        AudioClip clip = isUp ? spikeUpClip : spikeDownClip;

        if (clip == null || audioSource == null)
            return;

        if (playerTransform == null)
            FindPlayer();

        if (playerTransform == null)
            return;

        float distance = Vector2.Distance(
            transform.position,
            playerTransform.position
        );

        if (distance > hearingRadius)
            return;

        float volume = soundVolume;

        if (fadeWithDistance)
            volume *= 1f - Mathf.Clamp01(distance / hearingRadius);

        audioSource.PlayOneShot(clip, volume);
    }*/

    private void FindPlayer()
    {
        // Uses your existing PlayerHealth script to locate the player.
        // On Unity versions older than 2023, use FindObjectOfType instead.
        PlayerHealth health = FindFirstObjectByType<PlayerHealth>();

        if (health != null)
            playerTransform = health.transform;
    }

    private void ApplyState(bool instant)
    {
        // The damage collider follows the state straight away.
        if (damageCollider != null)
            damageCollider.enabled = isUp;

        // Play sound only on real transitions, not the initial setup.
        if (!instant && cycleSpikes)
            PlaySpikeSound();

        // Original transform movement (unchanged).
        if (UsesTransform && spikeVisual != null)
        {
            spikeVisual.localPosition = isUp
                ? upLocalPosition
                : downLocalPosition;
        }

        // New sprite swap / frame animation.
        if (UsesSprite && spikeSpriteRenderer != null)
        {
            if (spriteRoutine != null)
                StopCoroutine(spriteRoutine);

            Sprite[] frames = isUp ? risingFrames : fallingFrames;

            if (instant || frames == null || frames.Length == 0)
            {
                SetFinalSprite();
            }
            else
            {
                spriteRoutine = StartCoroutine(PlayTransition(frames));
            }
        }
    }

    private IEnumerator PlayTransition(Sprite[] frames)
    {
        float frameTime = 1f / Mathf.Max(1f, transitionFrameRate);

        for (int i = 0; i < frames.Length; i++)
        {
            if (frames[i] != null)
                spikeSpriteRenderer.sprite = frames[i];

            yield return new WaitForSeconds(frameTime);
        }

        SetFinalSprite();
        spriteRoutine = null;
    }

    private void SetFinalSprite()
    {
        Sprite sprite = isUp ? upSprite : downSprite;

        if (sprite != null)
            spikeSpriteRenderer.sprite = sprite;
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


