using System;
using UnityEngine;

// Put on the Player. Holds the ingredient stock and the dishes in hand.
// 1 ingredient = 1 dish.
public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    [SerializeField] private int startingIngredients = 10;
    [SerializeField] private int maxIngredients = 20;
    [Tooltip("Most dishes the player can carry at once (tables have 4 seats)")]
    [SerializeField] private int maxDishesOnHand = 4;

    public int Ingredients { get; private set; }
    public int MaxIngredients => maxIngredients;
    public int DishesOnHand { get; private set; }
    public int MaxDishesOnHand => maxDishesOnHand;
    public bool HandsFull => DishesOnHand >= maxDishesOnHand;

    // UI listens to this
    public event Action OnChanged;

    public AudioClip clip2, clip;

    private void Awake()
    {
        Instance = this;
        Ingredients = Mathf.Clamp(startingIngredients, 0, maxIngredients);
    }

    private void Start() => OnChanged?.Invoke();

    public int AddIngredients(int amount)
    {
        int added = Mathf.Min(amount, maxIngredients - Ingredients);
        if (added <= 0) return 0;
        Ingredients += added;
        OnChanged?.Invoke();
        return added;
    }

    // Used by the counter when it starts cooking a dish
    public bool TryUseIngredient()
    {
        if (Ingredients <= 0) 
            return false;
        
        Ingredients--;
        OnChanged?.Invoke();
        return true;
    }

    // Used when hit by a projectile
    public bool LoseIngredient()
    {
        if (Ingredients <= 0) return false;
        Ingredients--;
        OnChanged?.Invoke();
        return true;
    }

    // Returns how many dishes were actually picked up
    public int AddDishes(int amount)
    {
        int added = Mathf.Min(amount, maxDishesOnHand - DishesOnHand);
        if (added <= 0)
        {
            if (AudioManager.Instance != null)
            {
                 AudioManager.Instance.PlaySFX(clip2);
            }
            else
            {
                 Debug.LogWarning("AudioManager instance is missing from the scene!");
            }
            return 0;
        }
        else 
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(clip);
            }
            else
            {
                Debug.LogWarning("AudioManager instance is missing from the scene!");
            }
        }

        DishesOnHand += added;
        OnChanged?.Invoke();
        return added;
    }

    public bool TryTakeDishes(int amount)
    {
        if (amount <= 0 || DishesOnHand < amount) return false;
        DishesOnHand -= amount;
        OnChanged?.Invoke();
        return true;
    }
}
