using UnityEngine;

/// <summary>
/// Code-driven sprite animation for the player.
/// Idle = standing still, Walking = moving in any direction, Dashing = dash.
/// </summary>
public class PlayerSpriteAnimator : MonoBehaviour
{
    public enum AnimationState
    {
        Idle,
        Walking,
        Dashing
    }

    [System.Serializable]
    public class AnimationData
    {
        [Tooltip("Individual sprite images in animation order.")]
        public Sprite[] frames;

        [Min(1f)]
        public float frameRate = 6f;

        public bool loop = true;

        public bool HasFrames => frames != null && frames.Length > 0;
    }

    [Header("Sprite Renderer")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Player Animations")]
    [SerializeField] private AnimationData idleAnimation;
    [SerializeField] private AnimationData walkingAnimation;
    [SerializeField] private AnimationData dashAnimation;

    private AnimationData currentAnimation;
    private AnimationState currentState;

    private int currentFrame;
    private float frameTimer;

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
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (spriteRenderer == null || currentAnimation == null || !currentAnimation.HasFrames)
            return;

        if (currentAnimation.frames.Length == 1)
            return;

        if (!currentAnimation.loop && currentFrame >= currentAnimation.frames.Length - 1)
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
                else
                {
                    currentFrame = currentAnimation.frames.Length - 1;
                    break;
                }
            }
        }

        ApplyFrame();
    }

    private void ApplyFrame()
    {
        Sprite sprite = currentAnimation.frames[currentFrame];

        if (sprite != null && spriteRenderer != null)
            spriteRenderer.sprite = sprite;
    }

    /// <summary>
    /// Call every frame. Dash takes priority over walking, walking over idle.
    /// Switches animation only when the state actually changes.
    /// </summary>
    public void SetMoving(bool isMoving, bool isDashing = false)
    {
        if (isDashing)
            Play(AnimationState.Dashing);
        else
            Play(isMoving ? AnimationState.Walking : AnimationState.Idle);
    }

    public void PlayIdle() { Play(AnimationState.Idle); }
    public void PlayWalking() { Play(AnimationState.Walking); }
    public void PlayDashing() { Play(AnimationState.Dashing); }

    private void Play(AnimationState state, bool force = false)
    {
        AnimationData data;

        switch (state)
        {
            case AnimationState.Walking: data = walkingAnimation; break;
            case AnimationState.Dashing: data = dashAnimation; break;
            default: data = idleAnimation; break;
        }

        // No dash frames assigned: fall back to walking so it still animates.
        if (state == AnimationState.Dashing && (data == null || !data.HasFrames))
            data = walkingAnimation;

        // Missing frames: keep whatever sprite is showing.
        if (data == null || !data.HasFrames)
            return;

        if (!force && currentAnimation == data)
            return;

        currentState = state;
        currentAnimation = data;
        currentFrame = 0;
        frameTimer = 0f;

        ApplyFrame();
    }
}