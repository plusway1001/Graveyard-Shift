using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// Put on each chalk menu item (a Button with a TMP label).
// Hover/select: label turns ghost-green, chalk circle + skull pointer appear, slight wobble.
public class SpookyMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public TMP_Text label;
    public GameObject chalkHighlight;   // Chalk_Highlight image (child, disabled by default)
    public GameObject skullPointer;     // Skull_Pointer image (child, disabled by default)
    public Color normalColor = new Color32(208, 214, 200, 255);
    public Color hoverColor  = new Color32(150, 245, 190, 255);
    public AudioClip hoverSound;
    public float hoverScale = 1.08f;

    bool hovered; Vector3 baseScale;

    void Awake() { baseScale = transform.localScale; SetHover(false); }

    public void OnPointerEnter(PointerEventData e) => SetHover(true);
    public void OnPointerExit(PointerEventData e)  => SetHover(false);
    public void OnSelect(BaseEventData e)          => SetHover(true);
    public void OnDeselect(BaseEventData e)        => SetHover(false);

    void SetHover(bool on)
    {
        if (on && !hovered && hoverSound) AudioSource.PlayClipAtPoint(hoverSound, Camera.main ? Camera.main.transform.position : Vector3.zero, 0.6f);
        hovered = on;
        if (label) label.color = on ? hoverColor : normalColor;
        if (chalkHighlight) chalkHighlight.SetActive(on);
        if (skullPointer) skullPointer.SetActive(on);
        if (!on) transform.localScale = baseScale;
    }

    void Update()
    {
        if (!hovered) return;
        float t = Time.unscaledTime;
        transform.localScale = baseScale * (hoverScale + Mathf.Sin(t * 6f) * 0.015f);
        if (skullPointer) skullPointer.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 4f) * 8f);
    }
}
