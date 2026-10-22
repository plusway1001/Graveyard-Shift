
using System.Collections.Generic;
using UnityEngine;

public class RestaurantTable : MonoBehaviour
{
    [Header("Seats")]
    [SerializeField]
    private List<Transform> seats =
        new List<Transform>();

    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private readonly List<Customer> seatedCustomers =
        new List<Customer>();

    private TableRiotController riotController;
    private TableService tableService;

    public int Capacity => seats.Count;

    public int OccupiedSeats =>
        seatedCustomers.FindAll(c => c != null).Count;

    public int FreeSeats => Capacity - OccupiedSeats;

    public bool IsFull => FreeSeats <= 0;

    public IReadOnlyList<Customer> SeatedCustomers =>
        seatedCustomers;

    // The player can interact only if the table has free seats
    // and is not currently busy with its service.
    public bool CanInteract =>
        !IsFull &&
        (tableService == null || !tableService.IsBusy);

    private bool isEating;

    public bool CanAcceptCustomers =>
        !isEating &&
        (riotController == null || !riotController.IsRiotActive) &&
        OccupiedSeats < Capacity;

    private void Awake()
    {
        riotController = GetComponent<TableRiotController>();
        tableService = GetComponent<TableService>();

        // Clear the seating list when all customers leave.
        if (tableService != null)
        {
            tableService.onTableCleared.AddListener(
                ClearSeatedCustomers
            );
        }
    }

    public void SetEating(bool eating)
    {
        isEating = eating;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!Input.GetKeyDown(interactKey))
            return;

        // If the table can be served, serve it instead
        // of opening the customer assignment UI.
        if (tableService != null)
        {
            if (tableService.TryServe())
                return;

            // Do not open the seating UI while customers
            // are eating or the table is otherwise busy.
            if (tableService.IsBusy)
                return;
        }

        // Open the seating UI when the table is available.
        if (CanInteract && TableUIManager.Instance != null)
        {
            if (!CanAcceptCustomers)
            {
                Debug.Log("This table cannot accept customers right now.");
                return;
            }
            TableUIManager.Instance.OpenTable(this);
        }
    }

    public bool TrySeatCustomer(Customer customer)
    {
        if (!CanAcceptCustomers)
        {
            Debug.Log("This table is busy eating or experiencing a riot.");
            return false;
        }

        if (customer == null || IsFull)
            return false;

        // Do not seat customers while the table is busy.
        if (tableService != null && tableService.IsBusy)
            return false;

        if (seatedCustomers.Contains(customer))
            return false;

        Transform freeSeat = GetRandomFreeSeat();

        if (freeSeat == null)
            return false;

        // Record the customer before moving them to the seat.
        seatedCustomers.Add(customer);

        // Remove the customer from their previous queue.
        customer.SetOwningQueue(null);

        // Move the customer to the selected seat and apply
        // the seating bonus implemented in Customer.SeatAt().
        customer.SeatAt(freeSeat);

        // Register the customer with the riot system.
        if (riotController != null)
        {
            riotController.RegisterCustomer(customer);
        }

        // Register the customer with the serving system.
        if (tableService != null)
        {
            tableService.RegisterCustomer(customer.gameObject);
        }

        return true;
    }

    private Transform GetRandomFreeSeat()
    {
        List<Transform> freeSeats = new List<Transform>();

        foreach (Transform seat in seats)
        {
            if (seat == null)
                continue;

            bool occupied = false;

            foreach (Customer customer in seatedCustomers)
            {
                if (customer != null &&
                    customer.transform.parent == seat)
                {
                    occupied = true;
                    break;
                }
            }

            if (!occupied)
            {
                freeSeats.Add(seat);
            }
        }

        if (freeSeats.Count == 0)
            return null;

        return freeSeats[Random.Range(0, freeSeats.Count)];
    }

    public void ClearSeatedCustomers()
    {
        seatedCustomers.Clear();
    }

    public bool ContainsMixedCustomerTypes()
    {
        Customer first = null;

        foreach (Customer customer in seatedCustomers)
        {
            if (customer == null)
                continue;

            if (first == null)
            {
                first = customer;
                continue;
            }

            if (customer.Type != first.Type)
                return true;
        }

        return false;
    }

    public List<CustomerType> GetCustomerTypes()
    {
        List<CustomerType> result =
            new List<CustomerType>();

        foreach (Customer customer in seatedCustomers)
        {
            if (customer != null)
            {
                result.Add(customer.Type);
            }
        }

        return result;
    }
}


/*using System.Collections.Generic;
using UnityEngine;

public class RestaurantTable : MonoBehaviour
{
    [Header("Seats")]
    [SerializeField] private List<Transform> seats = new List<Transform>();

    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private readonly List<Customer> seatedCustomers =
        new List<Customer>();

    private TableRiotController riotController;

    public int Capacity => seats.Count;
    public int OccupiedSeats => seatedCustomers.FindAll(c => c != null).Count;
    public int FreeSeats => Capacity - OccupiedSeats;
    public bool IsFull => FreeSeats <= 0;
    public IReadOnlyList<Customer> SeatedCustomers => seatedCustomers;
    public bool CanInteract => !IsFull;

    private void Awake()
    {
        riotController = GetComponent<TableRiotController>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (Input.GetKeyDown(interactKey) &&
            TableUIManager.Instance != null)
        {
            TableUIManager.Instance.OpenTable(this);
        }
    }

    public bool TrySeatCustomer(Customer customer)
    {
        if (customer == null || IsFull)
            return false;

        if (seatedCustomers.Contains(customer))
            return false;

        Transform freeSeat = GetRandomFreeSeat();

        if (freeSeat == null)
            return false;

        // Record the customer before seating.
        seatedCustomers.Add(customer);
        customer.SetOwningQueue(null);
        // SeatAt() moves the customer and applies the
        // seating bonus in your existing Customer script.
        customer.SeatAt(freeSeat);

        // Register with the riot controller.
        if (riotController != null)
        {
            riotController.RegisterCustomer(customer);
        }

        return true;
    }

    private Transform GetRandomFreeSeat()
    {
        List<Transform> freeSeats = new List<Transform>();

        foreach (Transform seat in seats)
        {
            if (seat == null)
                continue;

            bool occupied = false;

            foreach (Customer customer in seatedCustomers)
            {
                if (customer != null &&
                    customer.transform.parent == seat)
                {
                    occupied = true;
                    break;
                }
            }

            if (!occupied)
                freeSeats.Add(seat);
        }

        if (freeSeats.Count == 0)
            return null;

        return freeSeats[Random.Range(0, freeSeats.Count)];
    }

    public void ClearSeatedCustomers()
    {
        seatedCustomers.Clear();
    }

    public bool ContainsMixedCustomerTypes()
    {
        Customer first = null;

        foreach (Customer customer in seatedCustomers)
        {
            if (customer == null)
                continue;

            if (first == null)
            {
                first = customer;
                continue;
            }

            if (customer.Type != first.Type)
                return true;
        }

        return false;
    }

    public List<CustomerType> GetCustomerTypes()
    {
        List<CustomerType> result = new List<CustomerType>();

        foreach (Customer customer in seatedCustomers)
        {
            if (customer != null)
                result.Add(customer.Type);
        }

        return result;
    }
}*/