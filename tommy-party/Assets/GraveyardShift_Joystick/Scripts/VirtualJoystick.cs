using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// On-screen joystick for phone builds.
/// Put this on a transparent UI Image that covers the touch zone (e.g. the left side of the screen).
/// "Background" is the ring, "Handle" is the knob (child of Background).
/// Read <see cref="Direction"/> (-1..1 on each axis) from any script.
/// </summary>
public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public enum JoystickMode
    {
        Fixed,     // Ring stays in one place.
        Floating,  // Ring jumps to where the thumb first touches.
        Dynamic    // Like Floating, and the ring follows the thumb if it drags too far.
    }

    [Header("References")]
    public RectTransform background;
    public RectTransform handle;

    [Header("Behaviour")]
    public JoystickMode mode = JoystickMode.Floating;

    [Tooltip("Ignore tiny thumb movements below this amount (0-1).")]
    [Range(0f, 0.9f)] public float deadZone = 0.15f;

    [Tooltip("How far the knob can travel, as a fraction of the ring's radius.")]
    [Range(0.2f, 1.5f)] public float handleRange = 0.9f;

    [Tooltip("Lock movement to 8 directions (up, down, left, right and diagonals).")]
    public bool snapTo8Directions = false;

    [Header("Look")]
    [Tooltip("Ring opacity while nobody is touching it.")]
    [Range(0f, 1f)] public float idleAlpha = 0.55f;
    [Range(0f, 1f)] public float activeAlpha = 1f;

    [Tooltip("Hide the joystick on PC / in the Editor (keyboard still works).")]
    public bool onlyShowOnMobile = false;

    /// <summary>Joystick input, each axis -1..1. Zero when not touched.</summary>
    public Vector2 Direction { get; private set; }

    /// <summary>True while a finger is on the joystick.</summary>
    public bool IsHeld { get; private set; }

    private RectTransform area;
    private CanvasGroup group;
    private Vector2 restPosition;
    private int activePointerId = int.MinValue;

    private void Awake()
    {
        area = transform as RectTransform;

        if (background != null)
        {
            restPosition = background.anchoredPosition;
            group = background.GetComponent<CanvasGroup>();
            if (group == null) group = background.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
        }

        if (onlyShowOnMobile && !Application.isMobilePlatform)
            gameObject.SetActive(false);

        ResetStick();
    }

    private void OnDisable()
    {
        // Never leave the player walking if the joystick gets hidden (pause menu etc.).
        ResetStick();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (activePointerId != int.MinValue || background == null) return;

        activePointerId = eventData.pointerId;
        IsHeld = true;

        if (mode != JoystickMode.Fixed &&
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                area, eventData.position, eventData.pressEventCamera, out Vector3 worldPoint))
        {
            background.position = worldPoint;
        }

        if (group != null) group.alpha = activeAlpha;
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId || background == null) return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, eventData.position, eventData.pressEventCamera, out Vector2 local))
            return;

        // Centre of the ring in its own local space.
        local -= background.rect.center;

        float radius = Mathf.Max(1f, background.rect.width * 0.5f);
        Vector2 raw = local / radius;

        // Dynamic mode: drag the ring along when the thumb goes past the edge.
        if (mode == JoystickMode.Dynamic && raw.magnitude > 1f)
        {
            Vector2 excess = local - local.normalized * radius;
            background.position += background.TransformVector(excess);
            raw = raw.normalized;
        }

        if (raw.magnitude > 1f) raw = raw.normalized;

        // Dead zone, then rescale so movement still starts smoothly from 0.
        float mag = raw.magnitude;
        Vector2 input = Vector2.zero;
        if (mag > deadZone)
        {
            input = raw.normalized * Mathf.InverseLerp(deadZone, 1f, mag);
            if (snapTo8Directions) input = Snap8(input);
        }

        Direction = input;

        if (handle != null)
            handle.anchoredPosition = raw * radius * handleRange;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId) return;
        ResetStick();
    }

    private void ResetStick()
    {
        activePointerId = int.MinValue;
        IsHeld = false;
        Direction = Vector2.zero;

        if (handle != null) handle.anchoredPosition = Vector2.zero;
        if (background != null && mode != JoystickMode.Fixed) background.anchoredPosition = restPosition;
        if (group != null) group.alpha = idleAlpha;
    }

    private static Vector2 Snap8(Vector2 v)
    {
        if (v == Vector2.zero) return v;
        float angle = Mathf.Atan2(v.y, v.x);
        float step = Mathf.PI / 4f;
        angle = Mathf.Round(angle / step) * step;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * v.magnitude;
    }
}
