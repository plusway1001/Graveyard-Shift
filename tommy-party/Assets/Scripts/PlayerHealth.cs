using System.Collections;
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
}
