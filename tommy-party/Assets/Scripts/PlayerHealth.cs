using System.Collections;
using UnityEngine;
using TMPro;

// Attach to Player. Damage sources should call TakeDamage(1).
// IMPORTANT: Player colliders remain enabled during invulnerability so walls still block movement.
public class PlayerHealth : MonoBehaviour
{
    [Header("Lives")]
    [SerializeField, Min(1)] private int maxLives = 3;
    [SerializeField] private int startingLives = 3;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Hit Protection")]
    [SerializeField, Min(0f)] private float hitInvulnerability = 1f;
    [SerializeField, Range(0.05f, 1f)] private float invisibleAlpha = 0.4f;

    public int CurrentLives { get; private set; }
    public int MaxLives => maxLives;
    public bool IsInvulnerable { get; private set; }
    public bool IsHiding { get; private set; }
    public bool IsGameOver { get; private set; }

    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;
    private Coroutine protectionRoutine;

    [Header("Audio (optional)")]
    [SerializeField] private AudioClip[] hurtClip, deathClip;

    private void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalColors = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
            originalColors[i] = spriteRenderers[i].color;

        CurrentLives = Mathf.Clamp(startingLives, 1, maxLives);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        RefreshUI();
    }

    public bool TakeDamage(int amount = 1)
    {
        if (amount <= 0 || IsGameOver || IsInvulnerable) return false;

        CurrentLives = Mathf.Max(0, CurrentLives - amount);
        RefreshUI();

        if (CurrentLives <= 0)
        {
            if (deathClip != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(deathClip[Random.Range(0, deathClip.Length)]);
            GameOver();
        }
        else
        {
            if (hurtClip != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(hurtClip[Random.Range(0, hurtClip.Length)]);
            BeginHitProtection(hitInvulnerability);
        }
        return true;
    }

    public bool TakeProjectileDamage(int amount = 1)
    {
        if (IsHiding) return false;
        return TakeDamage(amount);
    }

    public void AddLife(int amount = 1)
    {
        if (amount <= 0 || IsGameOver) return;
        CurrentLives = Mathf.Min(maxLives, CurrentLives + amount);
        RefreshUI();
    }

    // Dash requests temporary invulnerability and uses the same translucent visual.
    public void GrantInvulnerability(float duration)
    {
        if (IsGameOver || duration <= 0f) return;
        BeginHitProtection(duration);
    }

    private void BeginHitProtection(float duration)
    {
        if (protectionRoutine != null) StopCoroutine(protectionRoutine);
        protectionRoutine = StartCoroutine(ProtectionRoutine(duration));
    }

    private IEnumerator ProtectionRoutine(float duration)
    {
        IsInvulnerable = true;
        SetSpriteAlpha(invisibleAlpha);
        yield return new WaitForSeconds(duration);

        IsInvulnerable = false;
        RestoreSpriteAlpha();
        protectionRoutine = null;
    }

    private void SetSpriteAlpha(float alpha)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] == null) continue;
            Color color = originalColors[i];
            color.a = alpha;
            spriteRenderers[i].color = color;
        }
    }

    private void RestoreSpriteAlpha()
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
            if (spriteRenderers[i] != null)
                spriteRenderers[i].color = originalColors[i];
    }

    public void SetHiding(bool hiding) { IsHiding = hiding; }

    private void RefreshUI()
    {
        if (livesText != null) livesText.text = "Lives: " + CurrentLives + "/" + maxLives;
    }

    private void GameOver()
    {
        IsGameOver = true;
        IsInvulnerable = true;
        if (protectionRoutine != null) StopCoroutine(protectionRoutine);
        RestoreSpriteAlpha();
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
        Debug.Log("GAME OVER - Player has no lives remaining.");
    }

    public void RestartCurrentScene()
    {
        Time.timeScale = 1f;
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene.buildIndex);
    }
}