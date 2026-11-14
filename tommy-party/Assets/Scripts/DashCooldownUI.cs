using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows the dash cooldown on a UI Slider, read straight from PlayerDash.
/// Empty right after a dash, fills as the cooldown recovers, full = ready to dash.
/// Put this on the Slider (or any UI object) and assign the slider.
/// </summary>
public class DashCooldownUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider slider;
    [Tooltip("Found automatically from the object tagged Player if left empty.")]
    [SerializeField] private PlayerDash dash;

    [Header("Optional")]
    [Tooltip("Fades the bar out while the dash is ready.")]
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
        }
    }

    private void Update()
    {
        if (slider == null || !FindDash())
            return;

        float progress = 1f;

        if (dash.DashCooldown > 0f)
            progress = 1f - Mathf.Clamp01(dash.CooldownRemaining / dash.DashCooldown);

        slider.value = progress;

        if (canvasGroup != null)
            canvasGroup.alpha = progress >= 1f ? 0f : 1f;
    }

    private bool FindDash()
    {
        if (dash != null)
            return true;

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            dash = player.GetComponent<PlayerDash>();

            if (dash == null)
                dash = player.GetComponentInChildren<PlayerDash>();
        }

        return dash != null;
    }
}
