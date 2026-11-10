using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameObjectiveManager : MonoBehaviour
{
    public static GameObjectiveManager Instance { get; private set; }

    [Header("Win Condition")]
    [SerializeField] private int targetRevenue = 50;
    [SerializeField] private int targetNights = 5;

    [Header("Night Cycle")]
    [SerializeField] private NightCycleManager nightCycleManager;

    [Header("HUD")]
    [SerializeField] private TMP_Text revenueText;
    [SerializeField] private TMP_Text targetRevenueText;
    //[SerializeField] private TMP_Text nightProgressText;


    [Header("Progress Bars")]
    [SerializeField] private Slider revenueProgressBar;
    [SerializeField] private Slider nightProgressBar;

    [SerializeField] private TMP_Text revenueProgressText;
    [SerializeField] private TMP_Text nightProgressText;


    [Header("Result Screens")]
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;

    public int TotalRevenue { get; private set; }
    public bool GameEnded { get; private set; }

    private int startingNightCount;
    private int completedNights;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        Time.timeScale = 1f;

        if (nightCycleManager == null)
            nightCycleManager = FindFirstObjectByType<NightCycleManager>();

        if (nightCycleManager != null)
            startingNightCount = nightCycleManager.NightCount;

        if (winScreen != null)
            winScreen.SetActive(false);

        if (loseScreen != null)
            loseScreen.SetActive(false);

        UpdateHUD();
    }

    private void Update()
    {
        if (GameEnded || nightCycleManager == null)
            return;

        // NightCount increases when the night cycle reaches 6:00 AM.
        completedNights = Mathf.Max(
            0,
            nightCycleManager.NightCount - startingNightCount
        );

        UpdateHUD();

        // Check whether the player has run out of nights.
        if (completedNights >= targetNights)
        {
            if (TotalRevenue >= targetRevenue)
                WinGame();
            else
                LoseGame();
        }
    }

    // Call this whenever the player earns revenue.
    public void AddRevenue(int amount)
    {
        if (GameEnded || amount <= 0)
            return;

        TotalRevenue += amount;

        UpdateHUD();

        if (TotalRevenue >= targetRevenue)
            WinGame();
    }


    private void UpdateHUD()
    {
        // Revenue display
        if (revenueText != null)
            revenueText.text = "$" + TotalRevenue.ToString("N0");

        if (targetRevenueText != null)
            targetRevenueText.text = "$" + targetRevenue.ToString("N0");

        // Revenue progress bar
        float revenueProgress = targetRevenue > 0
            ? Mathf.Clamp01((float)TotalRevenue / targetRevenue)
            : 1f;

        if (revenueProgressBar != null)
            revenueProgressBar.value = revenueProgress;

        if (revenueProgressText != null)
        {
            revenueProgressText.text =
                "$" + TotalRevenue.ToString("N0") +
                " / $" + targetRevenue.ToString("N0");
        }

        // Night progress bar
        float nightProgress = targetNights > 0
            ? Mathf.Clamp01((float)completedNights / targetNights)
            : 1f;

        if (nightProgressBar != null)
            nightProgressBar.value = nightProgress;

        if (nightProgressText != null)
        {
            nightProgressText.text =
                completedNights + " / " + targetNights + " Nights";
        }

        // Current night display
        if (nightProgressText != null && completedNights < targetNights)
        {
            nightProgressText.text =
                "Night " + (completedNights + 1) +
                " / " + targetNights;
        }
    }


    private void WinGame()
    {
        if (GameEnded)
            return;

        GameEnded = true;

        if (winScreen != null)
            winScreen.SetActive(true);

        Time.timeScale = 0f;

        Debug.Log("YOU WIN! Revenue target reached.");
    }

    private void LoseGame()
    {
        if (GameEnded)
            return;

        GameEnded = true;

        if (loseScreen != null)
            loseScreen.SetActive(true);

        Time.timeScale = 0f;

        Debug.Log("GAME OVER! Revenue target not reached.");
    }
}
