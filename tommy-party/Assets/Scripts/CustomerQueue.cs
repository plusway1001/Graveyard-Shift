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

    [Header("Automatic Queue Respawn")]
    [SerializeField] private int minBatchSize = 4;
    [SerializeField] private int maxBatchSize = 6;

    [SerializeField] private float minSpawnDelay = 20f;
    [SerializeField] private float maxSpawnDelay = 30f;

    private bool isWaitingForNextBatch = false;
    public int Count => customers.Count;
    public int MaxQueueSize => maxQueueSize;

    public AudioClip clip, clip2;

    public TableUIManager tableUI;

    private void Start()
    {
        //FillQueue(startingQueueSize);
        FillQueue(Random.Range(minBatchSize, maxBatchSize + 1));
    }

    private void Update()
    {
        // Do not start another cooldown if one is already running.
        if (customers.Count == 0 && !isWaitingForNextBatch)
        {
            StartCoroutine(SpawnNextBatch());
        }
    }

    public void RemoveExpiredCustomer(Customer customer)
    {
        if (customer == null)
            return;

        if (customers.Remove(customer))
        {
            Debug.Log(
                $"{customer.CustomerName} left because their patience reached zero."
            );

            RefreshQueuePositions();

            Destroy(customer.gameObject);
        }

        tableUI.CloseTableSpecial();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clip2);
        }
        else
        {
            Debug.LogWarning("AudioManager instance is missing from the scene!");
        }
    }

    private System.Collections.IEnumerator SpawnNextBatch()
    {
        isWaitingForNextBatch = true;

        // Random waiting period between 20 and 30 seconds.
        float waitTime = Random.Range(minSpawnDelay, maxSpawnDelay);
        yield return new WaitForSeconds(waitTime);

        // Only spawn if the queue is still empty.
        if (customers.Count == 0)
        {
            int batchSize = Random.Range(minBatchSize, maxBatchSize + 1);

            FillQueue(batchSize);

            Debug.Log(
                $"New customer batch spawned: {batchSize} customers."
            );
        }

        isWaitingForNextBatch = false;
    }

    public void FillQueue(int amount)
    {
        int target = Mathf.Min(amount, maxQueueSize);

        while (customers.Count < target)
            AddRandomCustomer();

        RefreshQueuePositions();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clip);
        }
        else
        {
            Debug.LogWarning("AudioManager instance is missing from the scene!");
        }
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

        newCustomer.SetOwningQueue(this);

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
