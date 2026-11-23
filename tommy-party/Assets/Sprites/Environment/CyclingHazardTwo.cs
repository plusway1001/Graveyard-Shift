using UnityEngine;

/// <summary>
/// Timed hazard used for the Pop-up Clown and the Oven Fire.
/// Cycle: OFF (harmless) -> WARNING (harmless, optional) -> BURN (hurts the player) -> repeat.
/// Two looks:
///   - Fade mode (default): the sprite fades in/out, like before.
///   - Sprite mode: swaps sprites (off / warning / ignite / burn loop), like the FireTrap script.
/// Put it on an object with a trigger Collider2D and a SpriteRenderer.
/// The collider is only switched on while burning.
/// </summary>
public class CyclingHazardTwo : MonoBehaviour
{
    [Header("Timing (seconds)")]
    [Tooltip("OFF time before the warning.")]
    [SerializeField, Min(0.1f)] private float inactiveDuration = 2.5f;
    [Tooltip("Warning (e.g. smoke) before the burn. 0 = no warning phase.")]
    [SerializeField, Min(0f)] private float warningDuration = 0f;
    [Tooltip("How long it burns (hurts the player).")]
    [SerializeField, Min(0.1f)] private float activeDuration = 1.5f;
    [Tooltip("Shifts the cycle so hazards do not all fire at the same time.")]
    [SerializeField, Min(0f)] private float startOffset = 0f;

    [Header("Damage")]
    [SerializeField, Min(1)] private int damage = 1;
    [SerializeField, Min(0.1f)] private float damageInterval = 1f;

    [Header("Look: Fade Mode (used when Use Sprites is off)")]
    [SerializeField, Range(0f, 1f)] private float inactiveAlpha = 0.25f;

    [Header("Look: Sprite Mode")]
    [SerializeField] private bool useSprites = false;
    [Tooltip("The sprite that is swapped. Found automatically if left empty.")]
    [SerializeField] private SpriteRenderer mainRenderer;
    [SerializeField] private Sprite offSprite;
    [SerializeField] private Sprite warningSprite;
    [Tooltip("Optional. Shown for the first frame of the burn.")]
    [SerializeField] private Sprite igniteSprite;
    [Tooltip("Burn frames, played in a loop while burning.")]
    [SerializeField] private Sprite[] burnLoop;
    [SerializeField, Min(1f)] private float burnFps = 10f;

    [Header("Sound (optional)")]
    [SerializeField] private AudioClip whooshSound;
    [Tooltip("Beyond this distance from the player the whoosh is silent.")]
    [SerializeField, Min(0.1f)] private float hearDistance = 10f;

    private enum Phase { Off, Warning, Burn }

    private SpriteRenderer[] renderers;
    private Collider2D hitbox;
    private Phase phase;
    private bool initialised;
    private float nextDamageTime;
    private Transform playerTransform;

    public bool IsBurning => phase == Phase.Burn;

    private void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();
        hitbox = GetComponent<Collider2D>();

        if (mainRenderer == null)
            mainRenderer = GetComponent<SpriteRenderer>();

        if (mainRenderer == null && renderers.Length > 0)
            mainRenderer = renderers[0];
    }

    private void Update()
    {
        float cycle = inactiveDuration + warningDuration + activeDuration;
        float t = (Time.time + startOffset) % cycle;

        Phase newPhase;
        float burnStart = inactiveDuration + warningDuration;

        if (t < inactiveDuration)
            newPhase = Phase.Off;
        else if (t < burnStart)
            newPhase = Phase.Warning;
        else
            newPhase = Phase.Burn;

        if (!initialised || newPhase != phase)
        {
            bool startedBurning = initialised && newPhase == Phase.Burn;

            initialised = true;
            EnterPhase(newPhase);

            if (startedBurning)
                PlayWhoosh();
        }

        // Burn animation (sprite mode).
        if (useSprites && phase == Phase.Burn)
            AnimateBurn(t - burnStart);
    }

    private void EnterPhase(Phase newPhase)
    {
        phase = newPhase;

        // Only dangerous while burning.
        if (hitbox != null)
            hitbox.enabled = (phase == Phase.Burn);

        if (useSprites)
        {
            if (mainRenderer == null)
                return;

            // Make sure a previous fade does not leave the sprite see-through.
            Color c = mainRenderer.color;
            c.a = 1f;
            mainRenderer.color = c;

            if (phase == Phase.Off && offSprite != null)
                mainRenderer.sprite = offSprite;
            else if (phase == Phase.Warning && warningSprite != null)
                mainRenderer.sprite = warningSprite;
            // Burn sprite is set by AnimateBurn.
        }
        else
        {
            float alpha = 1f;

            if (phase == Phase.Off)
                alpha = inactiveAlpha;
            else if (phase == Phase.Warning)
                alpha = (inactiveAlpha + 1f) / 2f;   // half-way between faded and solid

            foreach (SpriteRenderer sr in renderers)
            {
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
        }
    }

    private void AnimateBurn(float burnElapsed)
    {
        if (mainRenderer == null)
            return;

        int frameIndex = Mathf.FloorToInt(Mathf.Max(0f, burnElapsed) * burnFps);
        bool hasLoop = burnLoop != null && burnLoop.Length > 0;

        Sprite sprite = null;

        if (igniteSprite != null)
        {
            // First frame = ignite, then the loop.
            if (frameIndex == 0 || !hasLoop)
                sprite = igniteSprite;
            else
                sprite = burnLoop[(frameIndex - 1) % burnLoop.Length];
        }
        else if (hasLoop)
        {
            sprite = burnLoop[frameIndex % burnLoop.Length];
        }

        if (sprite != null && mainRenderer.sprite != sprite)
            mainRenderer.sprite = sprite;
    }

    // Plays the whoosh through AudioManager, getting quieter the further the player is.
    private void PlayWhoosh()
    {
        if (whooshSound == null)
            return;

        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");

            if (p != null)
                playerTransform = p.transform;
        }

        float volume = 1f;

        if (playerTransform != null)
        {
            volume = 1f - Vector2.Distance(transform.position, playerTransform.position) / hearDistance;
        }

        if (volume <= 0.02f)
            return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFXTraps(whooshSound, volume);
        else
            AudioSource.PlayClipAtPoint(whooshSound, transform.position);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (phase != Phase.Burn || !other.CompareTag("Player"))
            return;

        if (Time.time < nextDamageTime)
            return;

        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();

        if (health == null)
            return;

        nextDamageTime = Time.time + damageInterval;

        health.TakeProjectileDamage(damage);
    }
}

/*using UnityEngine;

/// <summary>
/// Timed hazard used for the Pop-up Clown and the Oven Fire.
/// Cycle: OFF (harmless) -> WARNING (harmless, optional) -> BURN (hurts the player) -> repeat.
/// Two looks:
///   - Fade mode (default): the sprite fades in/out, like before.
///   - Sprite mode: swaps sprites (off / warning / ignite / burn loop), like the FireTrap script.
/// Put it on an object with a trigger Collider2D and a SpriteRenderer.
/// The collider is only switched on while burning.
/// </summary>
public class CyclingHazardTwo : MonoBehaviour
{
    [Header("Timing (seconds)")]
    [Tooltip("OFF time before the warning.")]
    [SerializeField, Min(0.1f)] private float inactiveDuration = 2.5f;
    [Tooltip("Warning (e.g. smoke) before the burn. 0 = no warning phase.")]
    [SerializeField, Min(0f)] private float warningDuration = 0f;
    [Tooltip("How long it burns (hurts the player).")]
    [SerializeField, Min(0.1f)] private float activeDuration = 1.5f;
    [Tooltip("Shifts the cycle so hazards do not all fire at the same time.")]
    [SerializeField, Min(0f)] private float startOffset = 0f;

    [Header("Damage")]
    [SerializeField, Min(1)] private int damage = 1;
    [SerializeField, Min(0.1f)] private float damageInterval = 1f;

    [Header("Look: Fade Mode (used when Use Sprites is off)")]
    [SerializeField, Range(0f, 1f)] private float inactiveAlpha = 0.25f;

    [Header("Look: Sprite Mode")]
    [SerializeField] private bool useSprites = false;
    [Tooltip("The sprite that is swapped. Found automatically if left empty.")]
    [SerializeField] private SpriteRenderer mainRenderer;
    [SerializeField] private Sprite offSprite;
    [SerializeField] private Sprite warningSprite;
    [Tooltip("Optional. Shown for the first frame of the burn.")]
    [SerializeField] private Sprite igniteSprite;
    [Tooltip("Burn frames, played in a loop while burning.")]
    [SerializeField] private Sprite[] burnLoop;
    [SerializeField, Min(1f)] private float burnFps = 10f;

    [Header("Audio (optional)")]
    [SerializeField] private AudioClip whooshSound;

    private enum Phase { Off, Warning, Burn }

    private SpriteRenderer[] renderers;
    private Collider2D hitbox;
    private Phase phase;
    private bool initialised;
    private float nextDamageTime;

    public bool IsBurning => phase == Phase.Burn;

    [Header("Sound (optional)")]
    public AudioClip whooshSound;
    [Tooltip("Beyond this distance from the player the whoosh is silent.")]
    public float hearDistance = 10f;

    Transform playerTransform;

    private void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();
        hitbox = GetComponent<Collider2D>();

        if (mainRenderer == null)
            mainRenderer = GetComponent<SpriteRenderer>();

        if (mainRenderer == null && renderers.Length > 0)
            mainRenderer = renderers[0];
    }

    private void Update()
    {
        float cycle = inactiveDuration + warningDuration + activeDuration;
        float t = (Time.time + startOffset) % cycle;

        Phase newPhase;
        float burnStart = inactiveDuration + warningDuration;

        if (t < inactiveDuration)
            newPhase = Phase.Off;
        else if (t < burnStart)
            newPhase = Phase.Warning;
        else
            newPhase = Phase.Burn;

        if (!initialised || newPhase != phase)
        {
            bool startedBurning = initialised && newPhase == Phase.Burn;

            initialised = true;
            EnterPhase(newPhase);

            if (startedBurning)
                PlayWhoosh();
        }

        // Burn animation (sprite mode).
        if (useSprites && phase == Phase.Burn)
            AnimateBurn(t - burnStart);
    }

    private void EnterPhase(Phase newPhase)
    {
        phase = newPhase;

        // Only dangerous while burning.
        if (hitbox != null)
            hitbox.enabled = (phase == Phase.Burn);

        if (useSprites)
        {
            if (mainRenderer == null)
                return;

            // Make sure a previous fade does not leave the sprite see-through.
            Color c = mainRenderer.color;
            c.a = 1f;
            mainRenderer.color = c;

            if (phase == Phase.Off && offSprite != null)
                mainRenderer.sprite = offSprite;
            else if (phase == Phase.Warning && warningSprite != null)
                mainRenderer.sprite = warningSprite;
            // Burn sprite is set by AnimateBurn.
        }
        else
        {
            float alpha = 1f;

            if (phase == Phase.Off)
                alpha = inactiveAlpha;
            else if (phase == Phase.Warning)
                alpha = (inactiveAlpha + 1f) / 2f;   // half-way between faded and solid

            foreach (SpriteRenderer sr in renderers)
            {
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
        }
    }

    private void AnimateBurn(float burnElapsed)
    {
        if (mainRenderer == null)
            return;

        int frameIndex = Mathf.FloorToInt(Mathf.Max(0f, burnElapsed) * burnFps);
        bool hasLoop = burnLoop != null && burnLoop.Length > 0;

        Sprite sprite = null;

        if (igniteSprite != null)
        {
            // First frame = ignite, then the loop.
            if (frameIndex == 0 || !hasLoop)
                sprite = igniteSprite;
            else
                sprite = burnLoop[(frameIndex - 1) % burnLoop.Length];
        }
        else if (hasLoop)
        {
            sprite = burnLoop[frameIndex % burnLoop.Length];
        }

        if (sprite != null && mainRenderer.sprite != sprite)
            mainRenderer.sprite = sprite;
    }

    private void PlayWhoosh()
    {
        //if (whooshSound != null && AudioManager.Instance != null)
        //AudioManager.Instance.PlaySFX(whooshSound);
        PlayWhoosh();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (phase != Phase.Burn || !other.CompareTag("Player"))
            return;

        if (Time.time < nextDamageTime)
            return;

        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();

        if (health == null)
            return;

        nextDamageTime = Time.time + damageInterval;

        health.TakeProjectileDamage(damage);
    }
}*/