using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Level goal: serve a number of seated customers. A customer counts as served
/// when their table finishes eating (TableService.onTableCleared).
/// </summary>
public class L2ServeGoal : MonoBehaviour
{
    public int target = 4;
    public GameObject winScreen;
    public TMP_Text progressText;
    public string label = "Ghosts served";
    [TextArea] public string hint = "Grab brains, get dishes, serve the ghosts!";
    public bool pauseOnWin = true;

    private int served;
    private bool won;
    private readonly Dictionary<TableService, int> waiting = new Dictionary<TableService, int>();

    private void Start() => Refresh();

    [SerializeField] private AudioClip WinSound;

    /// <summary>Called by L2SeatedGhosts for every ghost it seats.</summary>
    public void RegisterSeated(TableService service)
    {
        if (service == null) return;
        if (!waiting.ContainsKey(service))
        {
            waiting[service] = 0;
            service.onTableCleared.AddListener(() => OnTableCleared(service));
        }
        waiting[service]++;
        Refresh();
    }

    public void SetTarget(int newTarget)
    {
        target = Mathf.Max(1, newTarget);
        Refresh();
    }

    private void OnTableCleared(TableService service)
    {
        if (!waiting.TryGetValue(service, out int count) || count <= 0) return;
        waiting[service] = 0;
        served += count;
        Refresh();
        if (served >= target) { Win(); return; }

        // New trap layout in the Kitchen and Slaughter Room for the next trip.
        L2TrapShuffler.ShuffleAllRooms();
        L2BrainMeter meter = FindFirstObjectByType<L2BrainMeter>();
        if (meter != null) meter.ShowMessage("Ghost served!\n<size=70%>The traps in the Kitchen and Slaughter Room have moved...</size>");
    }

    private void Win()
    {
        if (won) return;
        won = true;
        CameraShake.Instance.StopShake();
        if (AudioManager.Instance != null && WinSound != null) AudioManager.Instance.PlaySFX(WinSound);

        if (winScreen != null) winScreen.SetActive(true);
        if (pauseOnWin) Time.timeScale = 0f;
    }

    private void Refresh()
    {
        if (progressText == null) return;
        progressText.text = $"{label}: {Mathf.Min(served, target)}/{target}" +
                            (string.IsNullOrEmpty(hint) ? "" : $"\n<size=60%>{hint}</size>");
    }
}
