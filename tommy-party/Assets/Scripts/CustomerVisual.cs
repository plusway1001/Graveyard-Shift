using UnityEngine;

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
}
