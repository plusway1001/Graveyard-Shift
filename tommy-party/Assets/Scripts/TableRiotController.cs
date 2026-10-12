
using System.Collections.Generic;
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

    private void SpawnRiotEffect()
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
}
