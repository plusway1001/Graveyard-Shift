using UnityEngine;

/// <summary>
/// Code-driven sprite animation for the riot enemy.
/// Idle  = loops by default (also plays while the riot walks).
/// Shoot = plays once when the riot throws, then returns to Idle.
/// Put on the riot enemy (next to RiotEnemy and its SpriteRenderer).
/// </summary>
public class RiotEnemySpriteAnimator : MonoBehaviour
{
    private enum AnimationState
    {
        Idle,
        Shoot
    }

    [System.Serializable]
    public class AnimationData
    {
        [Tooltip("Individual sprite images in animation order.")]
        public Sprite[] frames;

        [Min(1f)]
        public float frameRate = 8f;

        public bool loop = true;

        public bool HasFrames => frames != null && frames.Length > 0;
    }

    [Header("Sprite Renderer")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Animations")]
    [SerializeField] private AnimationData idleAnimation = new AnimationData { loop = true };
    [SerializeField] private AnimationData shootAnimation = new AnimationData { frameRate = 12f, loop = false };

    private AnimationState state = AnimationState.Idle;
    private AnimationData currentAnimation;

    private int currentFrame;
    private float frameTimer;

    // Total time the shoot animation takes. RiotEnemy can use this if needed.
    public float ShootDuration =>
        shootAnimation != null && shootAnimation.HasFrames
            ? shootAnimation.frames.Length / Mathf.Max(1f, shootAnimation.frameRate)
            : 0f;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
    }

    private void Start()
    {
        Play(AnimationState.Idle, true);
    }

    private void Update()
    {
        if (spriteRenderer == null || currentAnimation == null || !currentAnimation.HasFrames)
            return;

        // A single looping/idle frame never needs updating.
        if (currentAnimation.frames.Length == 1 && state != AnimationState.Shoot)
            return;

        frameTimer += Time.deltaTime;

        float frameDuration = 1f / Mathf.Max(1f, currentAnimation.frameRate);

        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            currentFrame++;

            if (currentFrame >= currentAnimation.frames.Length)
            {
                if (currentAnimation.loop)
                {
                    currentFrame = 0;
                }
                else if (state == AnimationState.Shoot)
                {
                    // Shot finished: back to idle.
                    Play(AnimationState.Idle, true);
                    return;
                }
                else
                {
                    currentFrame = currentAnimation.frames.Length - 1;
                    break;
                }
            }
        }

        ApplyFrame();
    }

    public void PlayIdle()  { Play(AnimationState.Idle, false); }

    // Restarts from the first frame each time the riot throws.
    public void PlayShoot() { Play(AnimationState.Shoot, true); }

    private void Play(AnimationState newState, bool restart)
    {
        AnimationData data = newState == AnimationState.Shoot ? shootAnimation : idleAnimation;

        // Missing frames: keep whatever sprite is showing.
        if (data == null || !data.HasFrames)
            return;

        if (!restart && data == currentAnimation)
            return;

        state = newState;
        currentAnimation = data;
        currentFrame = 0;
        frameTimer = 0f;

        ApplyFrame();
    }

    private void ApplyFrame()
    {
        Sprite sprite = currentAnimation.frames[currentFrame];

        if (sprite != null && spriteRenderer != null)
            spriteRenderer.sprite = sprite;
    }
}
