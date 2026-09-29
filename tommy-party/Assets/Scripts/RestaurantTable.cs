using System.Collections.Generic;
using UnityEngine;

public class RestaurantTable : MonoBehaviour
{
    [Header("Seats")]
    [SerializeField] private List<Transform> seats = new List<Transform>();

    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private readonly List<Customer> seatedCustomers = new List<Customer>();

    public int Capacity => seats.Count;
    public int OccupiedSeats => seatedCustomers.Count;
    public int FreeSeats => Capacity - OccupiedSeats;
    public bool IsFull => OccupiedSeats >= Capacity;
    public IReadOnlyList<Customer> SeatedCustomers => seatedCustomers;

    public bool CanInteract => !IsFull;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (Input.GetKeyDown(interactKey))
        {
            TableUIManager.Instance.OpenTable(this);
        }
    }

    public bool TrySeatCustomer(Customer customer)
    {
        if (customer == null || IsFull)
            return false;

        // Safety check: don't seat the same customer twice.
        if (seatedCustomers.Contains(customer))
            return false;

        Transform freeSeat = GetRandomFreeSeat();

        if (freeSeat == null)
            return false;

        seatedCustomers.Add(customer);
        customer.SeatAt(freeSeat);

        return true;
    }

    private Transform GetRandomFreeSeat()
    {
        List<Transform> freeSeats = new List<Transform>();

        for (int i = 0; i < seats.Count; i++)
        {
            Transform seat = seats[i];

            if (seat == null)
                continue;

            bool occupied = false;

            for (int j = 0; j < seatedCustomers.Count; j++)
            {
                Customer customer = seatedCustomers[j];

                if (customer != null && customer.transform.parent == seat)
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

    // Useful later for riot/mood systems.
    public bool ContainsMixedCustomerTypes()
    {
        if (seatedCustomers.Count <= 1)
            return false;

        CustomerType firstType = seatedCustomers[0].Type;

        for (int i = 1; i < seatedCustomers.Count; i++)
        {
            if (seatedCustomers[i].Type != firstType)
                return true;
        }

        return false;
    }

    public List<CustomerType> GetCustomerTypes()
    {
        List<CustomerType> result = new List<CustomerType>();

        for (int i = 0; i < seatedCustomers.Count; i++)
        {
            if (seatedCustomers[i] != null)
                result.Add(seatedCustomers[i].Type);
        }

        return result;
    }
}
