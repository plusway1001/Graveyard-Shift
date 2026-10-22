using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableRiotController : MonoBehaviour
{
    [Header("Riot Effect")]
    [SerializeField] private GameObject riotEffectPrefab;
    [SerializeField] private Vector3 effectOffset;

    [Header("Riot Duration")]
    [SerializeField, Min(0.1f)]
    private float riotDuration = 5f;

    [Header("Projectile Attack")]
    [SerializeField] private RiotProjectile projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Transform player;
    [SerializeField, Min(0.05f)]
    private float projectileInterval = 0.75f;

    [Header("Ingredient Drops")]
    [SerializeField] private GameObject ingredientPickupPrefab;
    [SerializeField] private Transform ingredientDropPoint;
    [SerializeField] private float dropRadius = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool logRiotEvents = true;

    private readonly List<Customer> seatedCustomers =
        new List<Customer>();

    private bool riotTriggered;
    private Coroutine riotRoutine;

    public bool IsRioting => riotTriggered;

    public IReadOnlyList<Customer> SeatedCustomers =>
        seatedCustomers;

    [Header("Riot Duration UI")]
    [SerializeField] private GameObject riotBarCanvas;
    [SerializeField] private Slider riotDurationSlider;

    public bool IsRiotActive => riotTriggered;

    private RestaurantTable table;
    public TableUIManager tableUI;

    private void Awake()
    {
        if (projectileSpawnPoint == null)
            projectileSpawnPoint = transform;

        if (ingredientDropPoint == null)
            ingredientDropPoint = transform;

        if (riotBarCanvas != null)
            riotBarCanvas.SetActive(false);

        if (riotDurationSlider != null)
        {
            riotDurationSlider.minValue = 0f;
            riotDurationSlider.maxValue = 1f;
            riotDurationSlider.value = 1f;
        }
    }

    public void RegisterCustomer(Customer customer)
    {
        if (customer == null || riotTriggered)
            return;

        if (!seatedCustomers.Contains(customer))
            seatedCustomers.Add(customer);

        // A seated customer at zero patience triggers a riot.
        if (customer.Patience <= 0f)
        {
            TriggerRiot("A seated customer has zero patience.");
            return;
        }

        // All customers at this table must share the same type.
        if (HasMixedCustomerTypes())
        {
            if (logRiotEvents)
                Debug.LogWarning(
                    $"{name}: Customer types do not match!"
                );

            // Trigger the riot first so all customers are captured
            // before any patience callbacks can trigger it again.
            tableUI.CloseTable();
            TriggerRiot("Customer group mismatch.");

            foreach (Customer seated in seatedCustomers)
            {
                if (seated != null)
                    seated.ForcePatienceToZero();
            }
        }
    }

    public void NotifyCustomerPatienceEmpty(Customer customer)
    {
        if (customer == null || riotTriggered)
            return;

        if (!seatedCustomers.Contains(customer))
            return;

        TriggerRiot(
            $"{customer.CustomerName} reached zero patience."
        );
    }

    private bool HasMixedCustomerTypes()
    {
        CustomerType? firstType = null;

        foreach (Customer customer in seatedCustomers)
        {
            if (customer == null)
                continue;

            if (firstType == null)
                firstType = customer.Type;
            else if (customer.Type != firstType.Value)
                return true;
        }

        return false;
    }


    public void TriggerRiot(string reason)
    {
        if (riotTriggered)
            return;

        // Mark the riot as active immediately.
        riotTriggered = true;

        // Show this table's riot duration bar.
        if (riotBarCanvas != null)
            riotBarCanvas.SetActive(true);

        if (riotDurationSlider != null)
            riotDurationSlider.value = 1f;

        if (logRiotEvents)
            Debug.LogWarning($"{name}: RIOT! {reason}");

        // Spawn one riot particle effect.
        if (riotEffectPrefab != null)
        {
            GameObject effect = Instantiate(
                riotEffectPrefab,
                transform.position + effectOffset,
                Quaternion.identity
            );

            ParticleSystem particles =
                effect.GetComponentInChildren<ParticleSystem>();

            if (particles != null)
            {
                float duration =
                    particles.main.duration +
                    particles.main.startLifetime.constantMax;

                Destroy(effect, duration);
            }
        }

        // IMPORTANT:
        // Count and remove all customers immediately,
        // but keep the count for ingredient drops later.
        int customerCount = seatedCustomers.Count;

        Customer[] customersToRemove =
            seatedCustomers.ToArray();

        seatedCustomers.Clear();

        foreach (Customer customer in customersToRemove)
        {
            if (customer == null)
                continue;

            customer.ClearSeatedTable(this);

            // Customers disappear as soon as the riot starts.
            Destroy(customer.gameObject);
        }

        // Start the projectile attack and delayed ingredient drops.
        if (riotRoutine == null)
        {
            riotRoutine = StartCoroutine(
                RiotSequence(customerCount)
            );
        }
    }



    private IEnumerator RiotSequence(int customerCount)
    {
        float elapsed = 0f;
        float nextShotTime = 0f;

        // Customers are already gone.
        // Continue the projectile attack for the riot duration.
        while (elapsed < riotDuration)
        {
            if (elapsed >= nextShotTime)
            {
                FireProjectile();

                nextShotTime += Mathf.Max(
                    0.05f,
                    projectileInterval
                );
            }

            elapsed += Time.deltaTime;

            // Update the remaining riot duration.
            if (riotDurationSlider != null)
            {
                float remaining = Mathf.Clamp01(
                    1f - (elapsed / riotDuration)
                );

                riotDurationSlider.value = remaining;
            }

            yield return null;
        }
        

        // Riot has ended. Drop one ingredient per removed customer.
        for (int i = 0; i < customerCount; i++)
        {
            DropIngredient();
        }

        // Hide the bar when the riot is over.
        if (riotBarCanvas != null)
            riotBarCanvas.SetActive(false);

        if (riotDurationSlider != null)
            riotDurationSlider.value = 1f;

        riotTriggered = false;
        riotRoutine = null;

        

        if (logRiotEvents)
        {
            Debug.Log(
                $"{name}: Riot ended. " +
                $"{customerCount} ingredient(s) dropped."
            );
        }
    }


    private void FireProjectile()
    {
        if (projectilePrefab == null || player == null)
            return;

        Vector3 spawnPosition =
            projectileSpawnPoint != null
                ? projectileSpawnPoint.position
                : transform.position;

        RiotProjectile projectile = Instantiate(
            projectilePrefab,
            spawnPosition,
            Quaternion.identity
        );

        projectile.Launch(player);
    }

    private void EndRiot()
    {
        // Capture the number of customers before clearing the list.
        int customerCount = seatedCustomers.Count;

        Customer[] customersToRemove =
            seatedCustomers.ToArray();

        seatedCustomers.Clear();

        foreach (Customer customer in customersToRemove)
        {
            if (customer == null)
                continue;

            customer.ClearSeatedTable(this);

            // Destroy the customer without giving cash.
            Destroy(customer.gameObject);
        }

        // One ingredient for every customer involved in the riot.
        for (int i = 0; i < customerCount; i++)
        {
            DropIngredient();
        }

        riotTriggered = false;

        if (logRiotEvents)
            Debug.Log(
                $"{name}: Riot ended. " +
                $"{customerCount} ingredient(s) dropped."
            );
    }

    private void DropIngredient()
    {
        if (ingredientPickupPrefab == null)
        {
            Debug.LogWarning(
                $"{name}: Ingredient Pickup Prefab is not assigned."
            );
            return;
        }

        Vector2 randomOffset =
            Random.insideUnitCircle * dropRadius;

        Vector3 spawnPosition =
            (ingredientDropPoint != null
                ? ingredientDropPoint.position
                : transform.position)
            + new Vector3(randomOffset.x, randomOffset.y, 0f);

        Instantiate(
            ingredientPickupPrefab,
            spawnPosition,
            Quaternion.identity
        );
    }

    public void UnregisterCustomer(Customer customer)
    {
        if (customer != null)
            seatedCustomers.Remove(customer);
    }
}


/*using System.Collections.Generic;
using UnityEngine;

public class TableRiotController : MonoBehaviour
{
    [Header("Riot Effect")]
    [SerializeField] private GameObject riotEffectPrefab;
    [SerializeField] private float effectLifetime = 5f;

    private readonly List<Customer> seatedCustomers =
        new List<Customer>();

    private bool riotInProgress = false;

    public bool RiotInProgress => riotInProgress;

    public void RegisterCustomer(Customer customer)
    {
        if (customer == null || riotInProgress)
            return;

        if (customer.PatienceExpired || customer.Patience <= 0f)
        {
            StartRiot();
            return;
        }

        if (!seatedCustomers.Contains(customer))
            seatedCustomers.Add(customer);

        // A customer who reached zero patience in the queue
        // causes a riot once seated.
        if (customer.Patience <= 0f)
        {
            StartRiot();
            return;
        }

        CheckCustomerGroup();
    }

    public void NotifyCustomerPatienceEmpty(Customer customer)
    {
        if (riotInProgress || customer == null)
            return;

        if (seatedCustomers.Contains(customer))
            StartRiot();
    }

    private void CheckCustomerGroup()
    {
        if (riotInProgress || seatedCustomers.Count <= 1)
            return;

        CustomerType firstType = seatedCustomers[0].Type;

        foreach (Customer customer in seatedCustomers)
        {
            if (customer == null)
                continue;

            if (customer.Type != firstType)
            {
                // Prevent further registration while the
                // mixed-group riot is being processed.
                foreach (Customer seated in seatedCustomers)
                {
                    if (seated != null)
                        seated.ForcePatienceToZero();
                }

                StartRiot();
                return;
            }
        }
    }


    public void StartRiot()
    {
        if (riotInProgress)
            return;

        riotInProgress = true;

        Debug.Log($"Riot started at {gameObject.name}");

        // Spawn exactly one effect for this riot event.
        SpawnRiotEffect();

        List<Customer> customersToDestroy =
            new List<Customer>(seatedCustomers);

        seatedCustomers.Clear();

        // Also clear the table's own occupancy tracking.
        RestaurantTable table = GetComponent<RestaurantTable>();

        if (table != null)
            table.ClearSeatedCustomers();

        foreach (Customer customer in customersToDestroy)
        {
            if (customer != null)
                Destroy(customer.gameObject);
        }

        // Permit another riot at this table in the future.
        riotInProgress = false;
    }


    /*public void StartRiot()
    {
        if (riotInProgress)
            return;

        // One effect per riot event.
        riotInProgress = true;

        Debug.Log($"Riot started at {gameObject.name}");

        SpawnRiotEffect();

        // Snapshot the customers before clearing the list.
        List<Customer> customersToDestroy =
            new List<Customer>(seatedCustomers);

        seatedCustomers.Clear();

        // Destroy every customer registered at this table,
        // regardless of customer type or patience.
        foreach (Customer customer in customersToDestroy)
        {
            if (customer != null)
                Destroy(customer.gameObject);
        }

        // Allow this table to riot again on a future event.
        // This does not create another effect for this event.
        riotInProgress = false;
    }*/

/* private void SpawnRiotEffect()
 {
     if (riotEffectPrefab == null)
     {
         Debug.LogWarning(
             $"Riot effect prefab missing on {gameObject.name}"
         );
         return;
     }

     GameObject effect = Instantiate(
         riotEffectPrefab,
         transform.position,
         Quaternion.identity
     );

     // Play particle systems on the prefab and its children.
     ParticleSystem[] particles =
         effect.GetComponentsInChildren<ParticleSystem>();

     foreach (ParticleSystem particle in particles)
     {
         particle.Play();
     }

     Destroy(effect, effectLifetime);
 }

 // Optional hook for your table's seating script.
 public void ClearSeatedCustomers()
 {
     seatedCustomers.Clear();
 }
}*/
