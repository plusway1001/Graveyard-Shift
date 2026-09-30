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
        // Stop decreasing once patience has reached zero.
        if (patienceEmpty)
            return;

        patience -= patienceDecreasePerSecond * Time.deltaTime;

        patience = Mathf.Clamp(
            patience,
            0f,
            maxPatience
        );

        UpdatePatienceUI();

        // Patience has reached zero.
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

    public void SeatAt(Transform seat)
    {
        if (seat == null)
            return;

        transform.SetPositionAndRotation(
            seat.position,
            seat.rotation
        );

        transform.SetParent(seat);
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
        // BLANK FOR NOW.
        //
        // Later:
        // - Tell the table that this customer's patience is 0
        // - Start table riot
        // - Change customer FSM to RIOT
        // - Spawn projectiles
        // - Drop ingredients
    }
}

/*using UnityEngine;

public class Customer : MonoBehaviour
{
    [Header("Customer Data")]
    [SerializeField] private CustomerType customerType;
    [SerializeField] private string customerName;

    // Kept separate so the future patience/mood system can use these values.
    [Header("Future Mood System")]
    [SerializeField, Range(0f, 100f)] private float patience = 100f;
    [SerializeField] private float baseMoodDecay = 1f;

    [SerializeField]
    private CustomerVisual customerVisual;

    public CustomerType Type => customerType;
    public string CustomerName => string.IsNullOrWhiteSpace(customerName)
        ? customerType.ToString()
        : customerName;

    public float Patience => patience;
    public float BaseMoodDecay => baseMoodDecay;

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

    // Called by the table when this customer is assigned.
    public void SeatAt(Transform seat)
    {
        if (seat == null)
            return;

        transform.SetPositionAndRotation(seat.position, seat.rotation);
        transform.SetParent(seat);
    }

    // Future mood system can call this.
    public void ChangePatience(float amount)
    {
        patience = Mathf.Clamp(patience + amount, 0f, 100f);
    }

    

    
}*/
