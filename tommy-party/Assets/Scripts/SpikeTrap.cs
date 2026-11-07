using System.Collections;
using UnityEngine;

// Static or moving spike hazard. Collider should be a trigger.
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
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isUp || !other.CompareTag("Player")) return;
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health != null) health.TakeDamage(1);
    }
}
