using UnityEngine;

/// <summary>Plays a list of sprites on a loop (used for the seated ghosts).</summary>
public class L2SpriteLoop : MonoBehaviour
{
    public Sprite[] frames;
    public float fps = 5f;
    private SpriteRenderer sr;
    private float offset;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        offset = Random.value * 10f; // so ghosts don't move in sync
    }

    private void LateUpdate()
    {
        if (sr == null || frames == null || frames.Length == 0) return;
        int i = Mathf.FloorToInt((Time.time + offset) * fps) % frames.Length;
        sr.sprite = frames[i];
    }
}
