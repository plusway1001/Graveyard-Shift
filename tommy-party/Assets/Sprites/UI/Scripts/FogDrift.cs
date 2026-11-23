using UnityEngine;
using UnityEngine.UI;

// Put on a full-screen RawImage using a fog texture (wrap mode Repeat). Scrolls slowly for drifting mist.
[RequireComponent(typeof(RawImage))]
public class FogDrift : MonoBehaviour
{
    public Vector2 speed = new Vector2(0.01f, 0f);
    RawImage img;
    void Awake() => img = GetComponent<RawImage>();
    void Update() { var r = img.uvRect; r.position += speed * Time.unscaledDeltaTime; img.uvRect = r; }
}
