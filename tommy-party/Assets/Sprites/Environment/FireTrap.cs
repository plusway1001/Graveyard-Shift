using UnityEngine;
using UnityEngine.Events;

/// Fire trap: Off -> Warning (smoke) -> Burn (flames) -> Off, on a loop.
/// Put it on an object with a SpriteRenderer and a Collider2D set to "Is Trigger".
/// The collider is ONLY switched on while the flames are up, so any damage script on this
/// object (e.g. the same one your spikes use) only hurts during the burn.
[RequireComponent(typeof(SpriteRenderer))]
public class FireTrap : MonoBehaviour
{
    [Header("Frames")]
    public Sprite offSprite;          // Fire_0_Off
    public Sprite warningSprite;      // Fire_1_Warning
    public Sprite igniteSprite;       // Fire_2_Ignite
    public Sprite[] burnLoop;         // Fire_3_Burn_A, B, C
    public float burnFps = 10f;

    [Header("Timing (seconds)")]
    public float offTime = 2.0f;
    public float warningTime = 0.8f;
    public float burnTime = 1.5f;
    [Tooltip("Offsets the loop so traps placed next to each other don't all fire at once.")]
    public float startDelay = 0f;
    public bool randomStart = true;

    [Header("Damage")]
    public Collider2D hitbox;         // leave empty to use the one on this object
    public string playerTag = "Player";
    public float hitCooldown = 1.0f;
    [Tooltip("Hook your player's damage / lose-heart method here.")]
    public UnityEvent onPlayerHit;

    [Header("Sound (optional)")]
    public AudioClip whooshSound;

    enum State { Off, Warning, Burn }
    State state = State.Off;
    float timer, frameTimer, lastHit = -99f;
    int frame;
    SpriteRenderer sr;

    public bool IsBurning => state == State.Burn;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (!hitbox) hitbox = GetComponent<Collider2D>();
        if (hitbox) hitbox.isTrigger = true;
        timer = startDelay + (randomStart ? Random.Range(0f, offTime) : 0f);
        Enter(State.Off, timer);
    }

    void Enter(State s, float time)
    {
        state = s; timer = time; frame = 0; frameTimer = 0f;
        if (hitbox) hitbox.enabled = (s == State.Burn);
        if (s == State.Off) sr.sprite = offSprite;
        if (s == State.Warning) sr.sprite = warningSprite;
        if (s == State.Burn)
        {
            sr.sprite = igniteSprite ? igniteSprite : (burnLoop.Length > 0 ? burnLoop[0] : offSprite);
            if (whooshSound) AudioSource.PlayClipAtPoint(whooshSound, transform.position);
        }
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            if (state == State.Off) Enter(State.Warning, warningTime);
            else if (state == State.Warning) Enter(State.Burn, burnTime);
            else Enter(State.Off, offTime);
        }

        if (state == State.Burn && burnLoop != null && burnLoop.Length > 0)
        {
            frameTimer += Time.deltaTime;
            if (frameTimer >= 1f / burnFps)
            {
                frameTimer = 0f;
                sr.sprite = burnLoop[frame % burnLoop.Length];
                frame++;
            }
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (state != State.Burn || !other.CompareTag(playerTag)) return;
        if (Time.time - lastHit < hitCooldown) return;
        lastHit = Time.time;
        onPlayerHit.Invoke();
    }
}
