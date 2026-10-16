using System.Collections.Generic;
using UnityEngine;

// Put on the counter. It cooks dishes one at a time (cooldown each),
// using 1 ingredient per dish. No ingredients = no new dish.
// Player presses E nearby to collect the ready dishes.
public class DishCounter : MonoBehaviour
{
    [SerializeField] private float cookCooldown = 3f;
    [Tooltip("How many cooked dishes can sit on the counter waiting")]
    [SerializeField] private int maxReadyDishes = 4;
    [SerializeField] private float interactRange = 1.8f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Visuals")]
    [SerializeField] private Sprite dishSprite;
    [SerializeField] private Vector2 firstDishOffset = new Vector2(-0.6f, 0.4f);
    [SerializeField] private float dishSpacing = 0.4f;
    [SerializeField] private float dishScale = 0.6f;
    [SerializeField] private Vector2 cookBarOffset = new Vector2(0f, 1.1f);

    public int ReadyDishes { get; private set; }
    public bool IsCooking { get; private set; }

    private float cookTimer;
    private Transform player;
    private WorldBar cookBar;
    private readonly List<SpriteRenderer> dishVisuals = new List<SpriteRenderer>();

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p) player = p.transform;

        cookBar = WorldBar.Create("CookBar", (Vector2)transform.position + cookBarOffset,
                                  new Color(1f, 0.7f, 0.2f), 1.2f);
        cookBar.Hide();
    }

    private void Update()
    {
        HandleCooking();
        HandleCollect();
    }

    private void HandleCooking()
    {
        if (!IsCooking)
        {
            // Start the next dish if there's room and an ingredient to spend
            if (ReadyDishes < maxReadyDishes && PlayerInventory.Instance != null
                && PlayerInventory.Instance.TryUseIngredient())
            {
                IsCooking = true;
                cookTimer = 0f;
                cookBar.Show();
            }
            return;
        }

        cookTimer += Time.deltaTime;
        cookBar.SetProgress(cookTimer / cookCooldown);

        if (cookTimer >= cookCooldown)
        {
            IsCooking = false;
            cookBar.Hide();
            ReadyDishes++;
            RefreshDishVisuals();
        }
    }

    private void HandleCollect()
    {
        if (player == null || ReadyDishes == 0 || !Input.GetKeyDown(interactKey)) return;
        if (Vector2.Distance(player.position, transform.position) > interactRange) return;

        int taken = PlayerInventory.Instance.AddDishes(ReadyDishes);
        if (taken > 0)
        {
            ReadyDishes -= taken;
            RefreshDishVisuals();
        }
    }

    private void RefreshDishVisuals()
    {
        while (dishVisuals.Count < ReadyDishes)
        {
            var go = new GameObject("ReadyDish");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = dishSprite;
            sr.sortingOrder = 5;
            go.transform.localScale = Vector3.one * dishScale;
            go.transform.position = (Vector2)transform.position + firstDishOffset
                                    + Vector2.right * dishSpacing * dishVisuals.Count;
            dishVisuals.Add(sr);
        }
        for (int i = 0; i < dishVisuals.Count; i++)
            dishVisuals[i].enabled = i < ReadyDishes;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
