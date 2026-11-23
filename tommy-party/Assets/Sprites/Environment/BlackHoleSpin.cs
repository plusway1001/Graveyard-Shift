using UnityEngine;

/// Plays the black hole frames on a loop with a slow "breathing" pulse.
/// Put it on the black hole / pit object that has a SpriteRenderer.
[RequireComponent(typeof(SpriteRenderer))]
public class BlackHoleSpin : MonoBehaviour
{
    public Sprite[] frames;          // BlackHole_0 ... BlackHole_7 in order
    public float fps = 12f;
    public float pulseAmount = 0.05f;
    public float pulseSpeed = 3f;

    SpriteRenderer sr; Vector3 baseScale; float t; int i;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        i = Random.Range(0, Mathf.Max(1, frames.Length));
    }

    void Update()
    {
        if (frames != null && frames.Length > 0)
        {
            t += Time.deltaTime;
            if (t >= 1f / fps) { t = 0f; i = (i + 1) % frames.Length; sr.sprite = frames[i]; }
        }
        transform.localScale = baseScale * (1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount);
    }
}
