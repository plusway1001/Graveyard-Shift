using System.Collections;
using UnityEngine;

/// <summary>
/// The visible ghost at a seat: sits (ghost_A), eats (ghost_B) while the table is busy,
/// then floats away (ghost_C) with sound when the table is cleared.
/// </summary>
public class L2GhostVisual : MonoBehaviour
{
    public Sprite[] idleFrames, eatingFrames, leavingFrames;
    public float fps = 5f;
    public float height = 2.4f;
    public AudioClip eatClip, leaveClip;
    [Range(0f, 1f)] public float volume = 0.9f;

    private Transform seat;
    private TableService service;
    private SpriteRenderer sr;
    private Sprite[] current;
    private bool eating, leaving;
    private float timeOffset;

    public void Init(Transform seatTransform, TableService tableService, bool faceLeft, string sortingLayer, int sortingOrder)
    {
        seat = seatTransform;
        service = tableService;
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = sortingLayer;
        sr.sortingOrder = sortingOrder;
        sr.flipX = faceLeft;
        timeOffset = Random.value * 5f;
        SetFrames(idleFrames);

        if (sr.sprite != null)
        {
            float h = sr.sprite.bounds.size.y;
            if (h > 0.001f) transform.localScale = Vector3.one * (height / h);
        }
        if (seat != null) transform.position = seat.position;
    }

    private void SetFrames(Sprite[] frames)
    {
        if (frames != null && frames.Length > 0) current = frames;
        if (sr != null && current != null && current.Length > 0) sr.sprite = current[0];
    }

    private void Update()
    {
        if (sr == null) return;

        if (!leaving)
        {
            if (seat != null) transform.position = seat.position;
            bool busy = service != null && service.IsBusy;
            if (busy && !eating)
            {
                eating = true;
                SetFrames(eatingFrames);
                PlayClip(eatClip);
            }
        }

        if (current != null && current.Length > 0)
        {
            int i = Mathf.FloorToInt((Time.time + timeOffset) * fps) % current.Length;
            sr.sprite = current[i];
        }
    }

    public void Leave()
    {
        if (leaving) return;
        leaving = true;
        SetFrames(leavingFrames);
        PlayClip(leaveClip);
        StartCoroutine(FloatAway());
    }

    private IEnumerator FloatAway()
    {
        Vector3 start = transform.position;
        Color c = sr.color;
        const float duration = 1.4f;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float k = t / duration;
            transform.position = start + Vector3.up * (k * 2f) + Vector3.right * Mathf.Sin(k * 12f) * 0.15f;
            sr.color = new Color(c.r, c.g, c.b, 1f - k);
            yield return null;
        }
        Destroy(gameObject);
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip == null) return;
        Vector3 at = Camera.main != null ? Camera.main.transform.position : transform.position;
        AudioSource.PlayClipAtPoint(clip, at, volume);
    }
}
