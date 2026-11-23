using UnityEngine;

/// Put this on your existing pink trap object.
/// Your trap script keeps doing the open/close timing and the damage; this only draws the Grave Chomper art on top.
/// It watches the pink square: bright = open (dangerous), dark = closed (safe).
public class GraveChomperVisual : MonoBehaviour
{
    public enum Detect { ColourBrightness, ColliderEnabled }

    [Header("Art")]
    public Sprite closedSprite;   // Chomper_0_Closed
    public Sprite warningSprite;  // Chomper_1_Warning
    public Sprite openSprite;     // Chomper_2_Open

    [Header("How to tell when it's open")]
    public Detect detectBy = Detect.ColourBrightness;
    [Tooltip("Tick if the art shows open when it should be closed.")]
    public bool invert = false;
    [Tooltip("Pink renderer to watch. Leave empty to use the one on this object.")]
    public SpriteRenderer source;
    [Tooltip("Collider to watch if Detect = ColliderEnabled. Leave empty to use the one on this object.")]
    public Collider2D sourceCollider;

    [Header("Feel")]
    public float warningTime = 0.12f;      // how long the half-open frame shows between states
    public float biteScale = 1.12f;        // little pop when it opens
    public AudioClip openSound;
    public AudioClip closeSound;

    SpriteRenderer art;
    float lo = float.MaxValue, hi = float.MinValue;
    bool shownOpen, targetOpen;
    float switchTimer, popTimer;
    Vector3 baseScale;

    void Awake()
    {
        if (!source) source = GetComponent<SpriteRenderer>();
        if (!sourceCollider) sourceCollider = GetComponent<Collider2D>();

        var go = new GameObject("ChomperArt");
        go.transform.SetParent(transform, false);
        art = go.AddComponent<SpriteRenderer>();
        art.sprite = closedSprite;
        if (source)
        {
            art.sortingLayerID = source.sortingLayerID;
            art.sortingOrder = source.sortingOrder + 1;   // draws over the pink square
            if (source.sprite && closedSprite)
            {
                Vector2 want = source.sprite.bounds.size, have = closedSprite.bounds.size;
                go.transform.localScale = new Vector3(want.x / have.x, want.y / have.y, 1f) * 1.04f;
            }
        }
        baseScale = go.transform.localScale;
    }

    bool ReadOpen()
    {
        bool open;
        if (detectBy == Detect.ColliderEnabled && sourceCollider)
            open = sourceCollider.enabled;
        else if (source)
        {
            Color c = source.color;
            float b = (c.r + c.g + c.b) * c.a;
            lo = Mathf.Min(lo, b); hi = Mathf.Max(hi, b);
            open = (hi - lo) > 0.1f && b > (lo + hi) * 0.5f;
        }
        else open = false;
        return invert ? !open : open;
    }

    void LateUpdate()
    {
        bool now = ReadOpen();
        if (now != targetOpen) { targetOpen = now; switchTimer = warningTime; art.sprite = warningSprite; }

        if (switchTimer > 0f)
        {
            switchTimer -= Time.deltaTime;
            if (switchTimer <= 0f)
            {
                shownOpen = targetOpen;
                art.sprite = shownOpen ? openSprite : closedSprite;
                var clip = shownOpen ? openSound : closeSound;
                if (clip) AudioSource.PlayClipAtPoint(clip, transform.position);
                if (shownOpen) popTimer = 0.15f;
            }
        }

        if (popTimer > 0f)
        {
            popTimer -= Time.deltaTime;
            float k = Mathf.Sin((1f - popTimer / 0.15f) * Mathf.PI);
            art.transform.localScale = baseScale * Mathf.Lerp(1f, biteScale, k);
        }
        else art.transform.localScale = baseScale;
    }
}
