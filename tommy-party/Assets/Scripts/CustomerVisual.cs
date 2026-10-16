/*using UnityEngine;

public class CustomerVisual : MonoBehaviour
{
    [System.Serializable]
    private struct TypeSprite
    {
        public CustomerType type;
        public Sprite sprite;
    }

    [SerializeField] private TypeSprite[] sprites;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
    }

    public void SetVisual(CustomerType type)
    {
        foreach (var entry in sprites)
        {
            if (entry.type == type && entry.sprite != null)
            {
                spriteRenderer.sprite = entry.sprite;
                return;
            }
        }

        Debug.LogError($"{name}: No sprite for {type}", this);
    }
}*/

/*using UnityEngine;

public class CustomerVisual : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite ghostSprite;
    [SerializeField] private Sprite zombieSprite;
    [SerializeField] private Sprite ghoulSprite;

    [SerializeField] private SpriteRenderer spriteRenderer;

    public void SetVisual(CustomerType type)
    {
        switch (type)
        {
            case CustomerType.Ghost:
                spriteRenderer.sprite = ghostSprite;
                break;

            case CustomerType.Zombie:
                spriteRenderer.sprite = zombieSprite;
                break;

            case CustomerType.Ghoul:
                spriteRenderer.sprite = ghoulSprite;
                break;
        }
    }
}*/
