using UnityEngine;

// A simple progress bar that floats in the world (used for cooking and eating).
// Builds itself from code, so no prefab or world-space canvas is needed.
public class WorldBar : MonoBehaviour
{
    private Transform fill;
    private float width;
    private static Sprite pixel;

    public static WorldBar Create(string name, Vector2 position, Color fillColor, float width = 1.5f, float height = 0.18f)
    {
        if (pixel == null)
        {
            var tex = new Texture2D(1, 1) { filterMode = FilterMode.Point };
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            // pivot on the left edge so scaling X fills left -> right
            pixel = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0f, 0.5f), 1f);
        }

        var root = new GameObject(name);
        root.transform.position = position;
        var bar = root.AddComponent<WorldBar>();
        bar.width = width;

        MakePart(root.transform, "Border", new Color(0.08f, 0.05f, 0.1f), -width / 2f - 0.04f, width + 0.08f, height + 0.08f, 20);
        MakePart(root.transform, "Back", new Color(0.25f, 0.2f, 0.3f), -width / 2f, width, height, 21);
        bar.fill = MakePart(root.transform, "Fill", fillColor, -width / 2f, 0f, height, 22);
        return bar;
    }

    private static Transform MakePart(Transform parent, string n, Color c, float x, float w, float h, int order)
    {
        var go = new GameObject(n);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(x, 0f, 0f);
        go.transform.localScale = new Vector3(w, h, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = pixel;
        sr.color = c;
        sr.sortingOrder = order;
        return go.transform;
    }

    public void SetProgress(float t)
    {
        Vector3 s = fill.localScale;
        s.x = width * Mathf.Clamp01(t);
        fill.localScale = s;
    }

    public void Show() { SetProgress(0f); gameObject.SetActive(true); }
    public void Hide() => gameObject.SetActive(false);
}
