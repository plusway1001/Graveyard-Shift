using UnityEngine;

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

    

    
}
