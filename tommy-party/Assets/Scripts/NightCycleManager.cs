using UnityEngine;
using TMPro;
using System;

public class NightCycleManager : MonoBehaviour
{
    public static NightCycleManager Instance { get; private set; }

    [Header("Night Time")]
    [SerializeField] private float startHour = 20f; // 20:00
    [SerializeField] private float endHour = 6f;    // 06:00

    [Header("Real Time Duration")]
    [Tooltip("How many real seconds the entire 20:00 - 06:00 night lasts.")]
    [SerializeField] private float realSecondsPerNight = 600f;

    [Header("Starting Time")]
    [SerializeField] private float currentHour = 20f;

    [Header("Patience Multipliers")]
    [SerializeField] private float youngNightMultiplier = 0.5f;
    [SerializeField] private float lateNightMultiplier = 1.0f;
    [SerializeField] private float midNightMultiplier = 2.0f;

    [Header("UI")]
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text phaseText;
    [SerializeField] private TMP_Text nightCounterText;

    [Header("Night Counter")]
    [SerializeField] private int nightCount = 1;

    private NightPhase currentPhase;

    public float CurrentHour => currentHour;
    public NightPhase CurrentPhase => currentPhase;
    public int NightCount => nightCount;

    public event Action<NightPhase> OnNightPhaseChanged;

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
        UpdateNightPhase(true);
        UpdateUI();
    }

    private void Update()
    {
        AdvanceTime();

        UpdateNightPhase(false);
        UpdateUI();
    }

    private void AdvanceTime()
    {
        // Total night duration:
        // 20:00 -> 06:00 = 10 in-game hours.
        //
        // Convert real time into in-game hours.

        float gameHoursPerSecond = 10f / realSecondsPerNight;

        currentHour += gameHoursPerSecond * Time.deltaTime;

        // When reaching 24:00, wrap to 00:00.
        if (currentHour >= 24f)
        {
            currentHour -= 24f;
        }

        // When the night reaches 06:00,
        // start a new night at 20:00.
        if (currentHour >= endHour &&
            currentHour < 20f)
        {
            currentHour = startHour;
            nightCount++;
        }
    }

    private void UpdateNightPhase(bool forceUpdate)
    {
        NightPhase newPhase;

        // 20:00 - 23:00
        if (currentHour >= 20f && currentHour < 23f)
        {
            newPhase = NightPhase.YoungNight;
        }

        // 23:00 - 03:00
        else if (currentHour >= 23f || currentHour < 3f)
        {
            newPhase = NightPhase.LateNight;
        }

        // 03:00 - 06:00
        else
        {
            newPhase = NightPhase.MidNight;
        }

        if (forceUpdate || newPhase != currentPhase)
        {
            currentPhase = newPhase;

            OnNightPhaseChanged?.Invoke(
                currentPhase
            );

            Debug.Log(
                "Night Phase: " +
                currentPhase
            );
        }
    }

    public float GetPatienceMultiplier()
    {
        switch (currentPhase)
        {
            case NightPhase.YoungNight:
                return youngNightMultiplier;

            case NightPhase.LateNight:
                return lateNightMultiplier;

            case NightPhase.MidNight:
                return midNightMultiplier;

            default:
                return 1f;
        }
    }

    private void UpdateUI()
    {
        UpdateTimeUI();
        UpdatePhaseUI();
        UpdateNightCounterUI();
    }

    private void UpdateTimeUI()
    {
        if (timeText == null)
            return;

        int hours = Mathf.FloorToInt(currentHour);
        int minutes = Mathf.FloorToInt(
            (currentHour - hours) * 60f
        );

        timeText.text =
            $"{hours:00}:{minutes:00}";
    }

    private void UpdatePhaseUI()
    {
        if (phaseText == null)
            return;

        switch (currentPhase)
        {
            case NightPhase.YoungNight:
                phaseText.text = "YOUNG NIGHT";
                break;

            case NightPhase.LateNight:
                phaseText.text = "LATE NIGHT";
                break;

            case NightPhase.MidNight:
                phaseText.text = "MID NIGHT";
                break;
        }
    }

    private void UpdateNightCounterUI()
    {
        if (nightCounterText == null)
            return;

        nightCounterText.text =
            "NIGHT " + nightCount;
    }
}