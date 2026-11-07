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

        if (CurrentLives <= 0) GameOver();
        else BeginHitProtection(hitInvulnerability);
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


/*using System.Collections;
using UnityEngine;
using TMPro;

// Attach to Player. All damage sources should call TakeDamage(1).
public class PlayerHealth : MonoBehaviour
{
    [Header("Lives")]
    [SerializeField, Min(1)] private int maxLives = 3;
    [SerializeField] private int startingLives = 3;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Hit Protection")]
    [SerializeField, Min(0f)] private float hitInvulnerability = 1f;
    [SerializeField] private bool disablePlayerCollidersDuringHit = true;

    public int CurrentLives { get; private set; }
    public int MaxLives => maxLives;
    public bool IsInvulnerable { get; private set; }
    public bool IsHiding { get; private set; }
    public bool IsGameOver { get; private set; }

    private Collider2D[] playerColliders;
    private Coroutine protectionRoutine;

    private void Awake()
    {
        playerColliders = GetComponentsInChildren<Collider2D>(true);
        CurrentLives = Mathf.Clamp(startingLives, 1, maxLives);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        RefreshUI();
    }

    public bool TakeDamage(int amount = 1)
    {
        if (amount <= 0 || IsGameOver || IsInvulnerable) return false;
        CurrentLives = Mathf.Max(0, CurrentLives - amount);
        RefreshUI();
        if (CurrentLives <= 0) GameOver();
        else BeginHitProtection(hitInvulnerability);
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

    // Dash can request brief protection without deducting a life.
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
        if (disablePlayerCollidersDuringHit) SetPlayerColliders(false);
        yield return new WaitForSeconds(duration);
        if (!IsGameOver && disablePlayerCollidersDuringHit) SetPlayerColliders(true);
        IsInvulnerable = false;
        protectionRoutine = null;
    }

    private void SetPlayerColliders(bool enabledState)
    {
        foreach (Collider2D col in playerColliders)
            if (col != null) col.enabled = enabledState;
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
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
        Debug.Log("GAME OVER - Player has no lives remaining.");
    }

    // Hook this to a Restart button if desired.
    public void RestartCurrentScene()
    {
        Time.timeScale = 1f;
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene.buildIndex);
    }
}*/
