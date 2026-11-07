using System.Collections;
using UnityEngine;

// Attach to Player. PC: Space. Mobile: wire Dash() to a UI Button.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDash : MonoBehaviour
{
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerHealth health;
    [SerializeField, Min(0.1f)] private float dashSpeed = 13f;
    [SerializeField, Min(0.05f)] private float dashDuration = 0.18f;
    [SerializeField, Min(0f)] private float dashCooldown = 1.5f;
    [SerializeField] private KeyCode dashKey = KeyCode.Space;

    public bool IsDashing { get; private set; }
    public float CooldownRemaining { get; private set; }
    private Rigidbody2D rb;
    private Coroutine dashRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (health == null) health = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (CooldownRemaining > 0f) CooldownRemaining -= Time.deltaTime;
        if (Input.GetKeyDown(dashKey)) Dash();
    }

    public void Dash()
    {
        if (IsDashing || CooldownRemaining > 0f || (health != null && health.IsGameOver)) return;
        Vector2 direction = movement != null ? movement.CurrentMovement : Vector2.zero;
        if (direction.sqrMagnitude < 0.01f) direction = Vector2.right;
        dashRoutine = StartCoroutine(DashRoutine(direction.normalized));
    }

    private IEnumerator DashRoutine(Vector2 direction)
    {
        IsDashing = true;
        CooldownRemaining = dashCooldown;
        if (health != null) health.GrantInvulnerability(dashDuration + 0.08f);
        float timer = 0f;
        while (timer < dashDuration)
        {
            rb.linearVelocity = direction * dashSpeed;
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        rb.linearVelocity = Vector2.zero;
        IsDashing = false;
        dashRoutine = null;
    }
}
