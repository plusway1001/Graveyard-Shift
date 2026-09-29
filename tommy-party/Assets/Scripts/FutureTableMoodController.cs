using UnityEngine;

/// <summary>
/// Placeholder for the next feature.
/// This is intentionally separate from seating so the seating prototype
/// does not need to be rewritten when patience/riot gameplay is added.
/// </summary>
public class FutureTableMoodController : MonoBehaviour
{
    [SerializeField] private RestaurantTable table;

    [Header("Future Settings")]
    [SerializeField] private float mixedGroupDecayMultiplier = 2f;
    [SerializeField] private float timeOfDayDecayMultiplier = 1f;

    public float MixedGroupDecayMultiplier => mixedGroupDecayMultiplier;
    public float TimeOfDayDecayMultiplier => timeOfDayDecayMultiplier;

    // Later this can:
    // 1. Read table.SeatedCustomers.
    // 2. Reduce each customer's patience over time.
    // 3. Apply faster decay for mixed groups.
    // 4. Apply time-of-day effects.
    // 5. Apply food/service/queue penalties.
    // 6. Detect patience <= 0.
    // 7. Tell a TableRiotController to start the riot.
}
