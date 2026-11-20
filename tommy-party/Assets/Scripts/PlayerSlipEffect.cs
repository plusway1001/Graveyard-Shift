using System.Collections;
using UnityEngine;

/// <summary>
/// Put on the Player. Play(duration) makes the sprite flip left/right rapidly
/// so it looks like the player is spinning after slipping.
/// Sprites go back to their original facing when it ends.
/// </summary>
public class PlayerSlipEffect : MonoBehaviour
{
    [Tooltip("Seconds between each flip. Smaller = faster spin.")]
    [SerializeField, Min(0.02f)] private float flipInterval = 0.08f;

    private SpriteRenderer[] spriteRenderers;
    private bool[] originalFlipX;
    private Coroutine routine;

    public bool IsSpinning => routine != null;

    private void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalFlipX = new bool[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
            originalFlipX[i] = spriteRenderers[i].flipX;
    }

    public void Play(float duration)
    {
        if (duration <= 0f)
            return;

        // Touching another peel mid-spin just restarts the spin.
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(SpinRoutine(duration));
    }

    private IEnumerator SpinRoutine(float duration)
    {
        float elapsed = 0f;
        bool flipped = false;

        while (elapsed < duration)
        {
            flipped = !flipped;
            SetFlip(flipped);

            yield return new WaitForSeconds(flipInterval);
            elapsed += flipInterval;
        }

        RestoreFlip();
        routine = null;
    }

    // Flipping is relative to each sprite's own starting direction.
    private void SetFlip(bool flipped)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
                spriteRenderers[i].flipX = flipped ? !originalFlipX[i] : originalFlipX[i];
        }
    }

    private void RestoreFlip()
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
                spriteRenderers[i].flipX = originalFlipX[i];
        }
    }

    private void OnDisable()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        if (spriteRenderers != null)
            RestoreFlip();
    }
}
