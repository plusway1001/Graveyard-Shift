using UnityEngine;

/// Makes the EXIT sign flicker now and then like a dying light.
[RequireComponent(typeof(SpriteRenderer))]
public class ExitSignFlicker : MonoBehaviour
{
    public Sprite onSprite;    // e.g. Exit_Right_On
    public Sprite dimSprite;   // e.g. Exit_Right_Dim
    public Vector2 waitBetween = new Vector2(1.5f, 5f);   // seconds between flickers
    public int maxBlinks = 3;
    public AudioClip buzzSound;

    SpriteRenderer sr; float timer; int blinksLeft; bool dim;

    void Awake() { sr = GetComponent<SpriteRenderer>(); sr.sprite = onSprite; timer = Random.Range(waitBetween.x, waitBetween.y); }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;

        if (blinksLeft == 0 && !dim)
        {
            blinksLeft = Random.Range(1, maxBlinks + 1);
            if (buzzSound) AudioSource.PlayClipAtPoint(buzzSound, transform.position, 0.5f);
        }
        dim = !dim;
        sr.sprite = dim ? dimSprite : onSprite;
        if (!dim) blinksLeft--;
        timer = (blinksLeft > 0 || dim) ? Random.Range(0.04f, 0.12f) : Random.Range(waitBetween.x, waitBetween.y);
    }
}
