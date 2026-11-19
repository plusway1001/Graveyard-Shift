using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD: shows how many brains you have toward the next dish (4 brains = 1 dish sent to the kitchen).
/// When the 4th brain is collected it pops a message and points an arrow at the Dish Counter.
/// Reads the existing "Ingredients: X/20" HUD text, so it works with the current ingredient system.
/// </summary>
public class L2BrainMeter : MonoBehaviour
{
    public TMP_Text ingredientText;
    public Image[] pips;
    public TMP_Text toast;
    public RectTransform arrow;
    public Transform dishCounter;
    public int brainsPerDish = 4;
    public float arrowSeconds = 10f;
    public float toastSeconds = 2.5f;
    public Color emptyColor = new Color(1f, 1f, 1f, 0.25f);
    public AudioClip readyClip;

    private int lastCount = -1;
    private int collected;
    private float toastTimer, arrowTimer;
    private Transform player;

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        if (toast != null) toast.gameObject.SetActive(false);
        if (arrow != null) arrow.gameObject.SetActive(false);
        RefreshPips();
    }

    private void Update()
    {
        int n = ReadCount();
        if (n >= 0)
        {
            if (lastCount >= 0 && n > lastCount)
            {
                collected += n - lastCount;
                while (collected >= brainsPerDish)
                {
                    collected -= brainsPerDish;
                    DishReady();
                }
                RefreshPips();
            }
            lastCount = n;
        }

        UpdateToast();
        UpdateArrow();
    }

    private int ReadCount()
    {
        if (ingredientText == null) return -1;
        string s = ingredientText.text;
        int i = 0;
        while (i < s.Length && !char.IsDigit(s[i])) i++;
        if (i >= s.Length) return -1;
        int value = 0;
        while (i < s.Length && char.IsDigit(s[i])) { value = value * 10 + (s[i] - '0'); i++; }
        return value;
    }

    private void RefreshPips()
    {
        if (pips == null) return;
        for (int i = 0; i < pips.Length; i++)
            if (pips[i] != null) pips[i].color = i < collected ? Color.white : emptyColor;
    }

    private void DishReady()
    {
        toastTimer = toastSeconds;
        arrowTimer = arrowSeconds;
        if (toast != null)
        {
            toast.text = "4 brains sent to the Kitchen!\n<size=70%>Grab your dish at the Dish Counter</size>";
            toast.gameObject.SetActive(true);
        }
        if (readyClip != null && Camera.main != null)
            AudioSource.PlayClipAtPoint(readyClip, Camera.main.transform.position, 0.8f);
    }

    /// <summary>Show a short message in the middle of the screen (no arrow).</summary>
    public void ShowMessage(string text)
    {
        if (toast == null) return;
        toast.text = text;
        toast.gameObject.SetActive(true);
        toastTimer = toastSeconds;
    }

    private void UpdateToast()
    {
        if (toast == null || toastTimer <= 0f) return;
        toastTimer -= Time.unscaledDeltaTime;
        float pulse = 1f + Mathf.Sin(Time.unscaledTime * 10f) * 0.04f;
        toast.transform.localScale = Vector3.one * pulse;
        toast.alpha = Mathf.Clamp01(toastTimer / 0.5f);
        if (toastTimer <= 0f) toast.gameObject.SetActive(false);
    }

    private void UpdateArrow()
    {
        if (arrow == null) return;
        bool near = player != null && dishCounter != null && Vector2.Distance(player.position, dishCounter.position) < 3f;
        if (arrowTimer > 0f) arrowTimer -= Time.unscaledDeltaTime;
        bool show = arrowTimer > 0f && !near && dishCounter != null && Camera.main != null;
        arrow.gameObject.SetActive(show);
        if (!show) return;

        Vector3 sp = Camera.main.WorldToScreenPoint(dishCounter.position);
        Vector2 screen = new Vector2(Screen.width, Screen.height);
        Vector2 centre = screen * 0.5f;
        bool onScreen = sp.z > 0 && sp.x > 0 && sp.x < screen.x && sp.y > 0 && sp.y < screen.y;

        if (onScreen)
        {
            // Bob above the counter, pointing down.
            arrow.position = new Vector3(sp.x, sp.y + 90f + Mathf.Sin(Time.unscaledTime * 6f) * 12f, 0f);
            arrow.rotation = Quaternion.Euler(0, 0, -90f);
        }
        else
        {
            // Sit at the screen edge, pointing towards the counter.
            Vector2 dir = ((Vector2)sp - centre);
            if (sp.z < 0) dir = -dir;
            dir.Normalize();
            float margin = 80f;
            float tx = dir.x != 0 ? (centre.x - margin) / Mathf.Abs(dir.x) : float.MaxValue;
            float ty = dir.y != 0 ? (centre.y - margin) / Mathf.Abs(dir.y) : float.MaxValue;
            Vector2 pos = centre + dir * Mathf.Min(tx, ty);
            arrow.position = pos;
            arrow.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        }
    }
}
