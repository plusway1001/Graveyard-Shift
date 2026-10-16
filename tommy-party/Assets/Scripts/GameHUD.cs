using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Put on the Canvas. Drag in the texts and bar images.
// For bars: UI Image, Image Type = Filled, Fill Method = Horizontal.
public class GameHUD : MonoBehaviour
{
    [Header("Ingredients (stock left = dishes you can still make)")]
    [SerializeField] private TMP_Text ingredientText;
    [SerializeField] private Image ingredientBarFill;

    [Header("Dishes on hand")]
    [SerializeField] private TMP_Text dishText;

    [Header("Revenue")]
    [SerializeField] private TMP_Text revenueText;
    [SerializeField] private Image revenueBarFill;

    private void OnEnable()
    {
        if (PlayerInventory.Instance) PlayerInventory.Instance.OnChanged += Refresh;
        if (RevenueManager.Instance) RevenueManager.Instance.OnChanged += Refresh;
    }

    private void Start()
    {
        // In case this ran before the managers' Awake
        OnDisable();
        OnEnable();
        Refresh();
    }

    private void OnDisable()
    {
        if (PlayerInventory.Instance) PlayerInventory.Instance.OnChanged -= Refresh;
        if (RevenueManager.Instance) RevenueManager.Instance.OnChanged -= Refresh;
    }

    private void Refresh()
    {
        var inv = PlayerInventory.Instance;
        if (inv != null)
        {
            if (ingredientText) ingredientText.text = $"Ingredients: {inv.Ingredients}/{inv.MaxIngredients}";
            if (ingredientBarFill) ingredientBarFill.fillAmount = (float)inv.Ingredients / Mathf.Max(1, inv.MaxIngredients);
            if (dishText) dishText.text = $"Dishes: {inv.DishesOnHand}/{inv.MaxDishesOnHand}";
        }

        var rev = RevenueManager.Instance;
        if (rev != null)
        {
            if (revenueText) revenueText.text = $"Cash: ${rev.Cash}";
            if (revenueBarFill) revenueBarFill.fillAmount = (float)rev.Cash / Mathf.Max(1, rev.RevenueGoal);
        }
    }
}
