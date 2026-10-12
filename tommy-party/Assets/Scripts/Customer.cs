using UnityEngine;
using UnityEngine.UI;

public class Customer : MonoBehaviour
{
    [Header("Customer Data")]
    [SerializeField] private CustomerType customerType;
    [SerializeField] private string customerName;

    [Header("Patience")]
    [SerializeField, Range(0f, 100f)]
    private float maxPatience = 100f;

    [SerializeField, Range(0f, 100f)]
    private float patience = 100f;

    [SerializeField]
    private float patienceDecreasePerSecond = 5f;

    [Header("Patience UI")]
    [SerializeField]
    private Slider patienceSlider;

    private bool patienceEmpty = false;

    [SerializeField]
    private CustomerVisual customerVisual;

    public CustomerType Type => customerType;
    public string CustomerName =>
        string.IsNullOrWhiteSpace(customerName)
            ? customerType.ToString()
            : customerName;

    public float Patience => patience;
    public float MaxPatience => maxPatience;

    private bool patienceExpired = false;
    public bool PatienceExpired => patienceExpired;

    private void Start()
    {
        // Every customer starts with 100% patience.
        patience = maxPatience;

        UpdatePatienceUI();
    }

    private void Update()
    {
        DecreasePatience();
    }

    private void DecreasePatience()
    {
        if (patienceEmpty)
            return;

        float nightMultiplier = 1f;

        if (NightCycleManager.Instance != null)
        {
            nightMultiplier =
                NightCycleManager.Instance.GetPatienceMultiplier();
        }

        float currentDecreaseRate =
            patienceDecreasePerSecond *
            nightMultiplier;

        patience -=
            currentDecreaseRate *
            Time.deltaTime;

        patience = Mathf.Clamp(
            patience,
            0f,
            maxPatience
        );

        UpdatePatienceUI();

        if (patience <= 0f)
        {
            patienceEmpty = true;

            OnPatienceEmpty();
        }
    }

    private void UpdatePatienceUI()
    {
        if (patienceSlider == null)
            return;

        patienceSlider.value = patience / maxPatience;
    }

    public void SetCustomerType(CustomerType type)
    {
        customerType = type;

        if (customerVisual != null)
        {
            customerVisual.SetVisual(type);
        }
    }

    public void SetCustomerName(string value)
    {
        customerName = value;
    }

    /*public void SeatAt(Transform seat)
    {
        if (seat == null)
            return;

        transform.SetPositionAndRotation(
            seat.position,
            seat.rotation
        );

        transform.SetParent(seat);

        // Find the riot controller on the table root.
        TableRiotController table =
            GetComponentInParent<TableRiotController>();

        if (table != null)
        {
            table.RegisterCustomer(this);
        }
    }*/


    public void SeatAt(Transform seat)
    {
        if (seat == null)
            return;

        // Move the customer to the selected seat.
        transform.SetPositionAndRotation(
            seat.position,
            seat.rotation
        );

        // Parent the customer to the seat.
        transform.SetParent(seat);

        // Apply the patience bonus to this customer.
        OnCustomerSeated();

        // Register the customer with the table's riot controller.
        TableRiotController table =
            GetComponentInParent<TableRiotController>();

        if (table != null)
        {
            table.RegisterCustomer(this);
        }
    }



    // ============================================
    // FUTURE FOOD SYSTEM
    // ============================================

    public void OnFoodServed()
    {
        // BLANK FOR NOW.
        //
        // Later:
        // - Increase patience
        // - Stop/modify patience timer
        // - Play eating animation
        // - Update customer mood
        // - Notify the table that food was served
    }

    // ============================================
    // FUTURE RIOT / GAME OVER SYSTEM
    // ============================================


    private void OnPatienceEmpty()
    {
        // Queue customers have no TableRiotController parent,
        // so reaching zero while waiting will not cause a riot.
        TableRiotController table =
            GetComponentInParent<TableRiotController>();

        if (table != null)
        {
            table.NotifyCustomerPatienceEmpty(this);
        }

        // Future game-over or other handling can be added here.
    }

    public void ForcePatienceToZero()
    {
        if (patienceEmpty)
            return;

        patience = 0f;
        patienceEmpty = true;

        UpdatePatienceUI();
    }


    [Header("Seating Patience Bonus")]
    [SerializeField, Min(0f)]
    private float seatingPatienceBonus = 10f;

    public void OnCustomerSeated()
    {
        if (patienceExpired)
            return;

        patience = Mathf.Clamp(
            patience + seatingPatienceBonus,
            0f,
            maxPatience
        );

        UpdatePatienceUI();
    }

}
