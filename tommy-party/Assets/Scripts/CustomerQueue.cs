using System.Collections.Generic;
using UnityEngine;

public class CustomerQueue : MonoBehaviour
{
    [Header("Queue")]
    [SerializeField, Range(1, 6)] private int startingQueueSize = 6;
    [SerializeField, Range(1, 6)] private int maxQueueSize = 6;

    [Header("Customer Spawn")]
    [SerializeField] private Customer customerPrefab;
    [SerializeField] private Transform queueSpawnPoint;
    [SerializeField] private float queueSpacing = 1.0f;

    [Header("Randomisation")]
    [SerializeField] private bool randomizeCustomerTypes = true;

    private readonly List<Customer> customers = new List<Customer>();

    public IReadOnlyList<Customer> Customers => customers;
    public int Count => customers.Count;
    public int MaxQueueSize => maxQueueSize;

    private void Start()
    {
        FillQueue(startingQueueSize);
    }

    public void FillQueue(int amount)
    {
        int target = Mathf.Min(amount, maxQueueSize);

        while (customers.Count < target)
            AddRandomCustomer();

        RefreshQueuePositions();
    }

    public Customer GetCustomer(int index)
    {
        if (index < 0 || index >= customers.Count)
            return null;

        return customers[index];
    }

    public bool RemoveCustomer(Customer customer)
    {
        if (customer == null)
            return false;

        // Remove the customer from the QUEUE only.
        // Do NOT destroy the GameObject because the customer
        // has already been moved to the table.
        bool removed = customers.Remove(customer);

        if (removed)
        {
            RefreshQueuePositions();
        }

        return removed;
    }

    public bool AddRandomCustomer()
    {
        if (customerPrefab == null || customers.Count >= maxQueueSize)
            return false;

        Customer newCustomer = Instantiate(customerPrefab);

        CustomerType randomType = randomizeCustomerTypes
            ? (CustomerType)Random.Range(0, System.Enum.GetValues(typeof(CustomerType)).Length)
            : CustomerType.Ghost;

        newCustomer.SetCustomerType(randomType);
        newCustomer.SetCustomerName(randomType.ToString());

        customers.Add(newCustomer);
        RefreshQueuePositions();

        return true;
    }

    private void RefreshQueuePositions()
    {
        if (queueSpawnPoint == null)
            return;

        for (int i = 0; i < customers.Count; i++)
        {
            Customer customer = customers[i];

            if (customer == null)
                continue;

            // Simple prototype queue: customers stand behind each other.
            Vector3 targetPosition =
                queueSpawnPoint.position + queueSpawnPoint.right * (i * queueSpacing);

            customer.transform.SetPositionAndRotation(
                targetPosition,
                queueSpawnPoint.rotation
            );

            customer.transform.SetParent(null);
        }
    }
}
