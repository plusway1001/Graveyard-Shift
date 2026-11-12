using UnityEngine;

/// <summary>
/// Code-driven sprite animation for customers.
/// Each CustomerType has its own Idle / Sitting / Eating frames.
/// Idle    = while queuing
/// Sitting = once seated at a table
/// Eating  = after the food is served
/// </summary>
public class CustomerSpriteAnimator : MonoBehaviour
{
    public enum AnimationState
    {
        Idle,
        Sitting,
        Eating
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

    [System.Serializable]
    public class CustomerAnimationSet
    {
        public CustomerType type;
        public AnimationData idle;
        public AnimationData sitting;
        public AnimationData eating;
    }

    [Header("Sprite Renderer")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Animations Per Customer Type")]
    [SerializeField] private CustomerAnimationSet[] animationSets;

    private CustomerAnimationSet activeSet;
    private AnimationState requestedState = AnimationState.Idle;
    private AnimationData currentAnimation;

    private int currentFrame;
    private float frameTimer;

    // Lets other scripts know the sprite animator is handling eating,
    // so the placeholder EatBob is not needed.
    public bool HasEatingAnimation =>
        activeSet != null && activeSet.eating != null && activeSet.eating.HasFrames;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
    }

    private void Update()
    {
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (spriteRenderer == null || currentAnimation == null || !currentAnimation.HasFrames)
            return;

        // A single frame (or finished non-looping clip) needs no updating.
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
    /// Called when the customer type is assigned. Picks that type's
    /// animation set and starts the Idle (queue) animation.
    /// </summary>
    public void SetCustomerType(CustomerType type)
    {
        activeSet = null;

        if (animationSets != null)
        {
            foreach (var set in animationSets)
            {
                if (set != null && set.type == type)
                {
                    activeSet = set;
                    break;
                }
            }
        }

        if (activeSet == null)
        {
            Debug.LogWarning(
                gameObject.name + ": No animation set for " + type, this);
            currentAnimation = null;
            return;
        }

        requestedState = AnimationState.Idle;
        Play(requestedState, true);
    }

    public void PlayIdle() { Play(AnimationState.Idle); }
    public void PlaySitting() { Play(AnimationState.Sitting); }
    public void PlayEating() { Play(AnimationState.Eating); }

    private void Play(AnimationState state, bool force = false)
    {
        requestedState = state;

        if (activeSet == null)
            return;

        AnimationData data = GetData(state);

        // Missing frames: keep whatever sprite is showing.
        if (data == null || !data.HasFrames)
            return;

        // Don't restart an animation that is already playing.
        if (!force && data == currentAnimation)
            return;

        currentAnimation = data;
        currentFrame = 0;
        frameTimer = 0f;

        ApplyFrame();
    }

    private AnimationData GetData(AnimationState state)
    {
        switch (state)
        {
            case AnimationState.Sitting: return activeSet.sitting;
            case AnimationState.Eating: return activeSet.eating;
            default: return activeSet.idle;
        }
    }
}

/*using UnityEngine;

public class CustomerSpriteAnimator : MonoBehaviour
{
    public enum AnimationState
    {
        Idle,
        Sitting,
        Eating
    }

    [System.Serializable]
    public class AnimationData
    {
        [Tooltip("Individual sprite images in animation order.")]
        public Sprite[] frames;

        [Min(1f)]
        public float frameRate = 6f;

        public bool loop = true;
    }

    [Header("Sprite Renderer")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Character Animations")]
    [SerializeField] private AnimationData idleAnimation;
    [SerializeField] private AnimationData sittingAnimation;
    [SerializeField] private AnimationData eatingAnimation;

    private AnimationState currentState;
    private AnimationData currentAnimation;

    private int currentFrame;
    private float frameTimer;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        PlayIdle();
    }

    private void Update()
    {
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (spriteRenderer == null ||
            currentAnimation == null ||
            currentAnimation.frames == null ||
            currentAnimation.frames.Length == 0)
        {
            return;
        }

        frameTimer += Time.deltaTime;

        float frameDuration =
            1f / Mathf.Max(1f, currentAnimation.frameRate);

        if (frameTimer < frameDuration)
            return;

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
            }
        }

        spriteRenderer.sprite =
            currentAnimation.frames[currentFrame];
    }

    public void PlayIdle()
    {
        PlayAnimation(AnimationState.Idle, idleAnimation);
    }

    public void PlaySitting()
    {
        PlayAnimation(AnimationState.Sitting, sittingAnimation);
    }

    public void PlayEating()
    {
        PlayAnimation(AnimationState.Eating, eatingAnimation);
    }

    public void PlayAnimation(
        AnimationState state,
        AnimationData animation)
    {
        if (animation == null ||
            animation.frames == null ||
            animation.frames.Length == 0)
        {
            Debug.LogWarning(
                gameObject.name + ": Animation frames are missing for " + state,
                this
            );
            return;
        }

        // Avoid restarting an animation every time it is requested.
        if (currentState == state && currentAnimation == animation)
            return;

        currentState = state;
        currentAnimation = animation;

        currentFrame = 0;
        frameTimer = 0f;

        spriteRenderer.sprite = currentAnimation.frames[0];
    }
}*/