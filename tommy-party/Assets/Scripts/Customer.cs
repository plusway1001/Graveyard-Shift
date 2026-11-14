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

    
    private TableRiotController seatedTable;

    // Code-driven sprite animation (Idle / Sitting / Eating per customer type)
    private CustomerSpriteAnimator spriteAnimator;

    public CustomerType Type => customerType;
    public string CustomerName =>
        string.IsNullOrWhiteSpace(customerName)
            ? customerType.ToString()
            : customerName;

    public float Patience => patience;
    public float MaxPatience => maxPatience;

    private bool patienceExpired = false;
    public bool PatienceExpired => patienceExpired;

    private CustomerQueue owningQueue;

    private bool hasBeenServed = false;

    [System.Serializable]
    private struct TypeSprite
    {
        public CustomerType type;
        public Sprite sprite;
    }

    [SerializeField] private TypeSprite[] sprites;

    public void SetVisual(CustomerType type)
    {
        foreach (var entry in sprites)
        {
            if (entry.type == type && entry.sprite != null)
            {
                this.GetComponent<SpriteRenderer>().sprite = entry.sprite;
                return;
            }
        }

        Debug.LogError($"{name}: No sprite for {type}", this);
    }

    private void Awake()
    {
        spriteAnimator = GetComponent<CustomerSpriteAnimator>();

        if (spriteAnimator == null)
            spriteAnimator = GetComponentInChildren<CustomerSpriteAnimator>(true);
    }

    private void Start()
    {
        // Every customer starts with 100% patience.
        //patience = maxPatience;

        //UpdatePatienceUI();
    }

    private void Update()
    {
        //DecreasePatience();
    }

    public void OnServed()
    {
        if (hasBeenServed)
            return;

        hasBeenServed = true;

        // Food served -> eating animation.
        if (spriteAnimator != null)
            spriteAnimator.PlayEating();

        // Hide the patience bar.
        if (patienceSlider != null)
        {
            patienceSlider.gameObject.SetActive(false);
        }

        Debug.Log($"{CustomerName} has been served. Patience stopped.");
    }

    public void SetOwningQueue(CustomerQueue queue)
    {
        owningQueue = queue;
    }

    private void DecreasePatience()
    {
        if (hasBeenServed || patienceExpired)
            return;
        if (patience <= 0f)
            //if (patienceEmpty)
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
            //patienceEmpty = true;

            OnPatienceEmpty();
        }
    }

    private void UpdatePatienceUI()
    {
        if (patienceSlider == null)
            return;

        patienceSlider.value = patience / maxPatience;
    }



    

    public void SetSeatedTable(TableRiotController table)
    {
        seatedTable = table;
    }

    public void ClearSeatedTable(TableRiotController table)
    {
        if (seatedTable == table)
            seatedTable = null;
    }
    public void SetCustomerType(CustomerType type)
    {
        customerType = type;
        customerName = type.ToString();

        SetVisual(type);

        // Queuing -> idle animation for this customer type.
        if (spriteAnimator == null)
            spriteAnimator = GetComponentInChildren<CustomerSpriteAnimator>(true);

        if (spriteAnimator != null)
            spriteAnimator.SetCustomerType(type);

        
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

        // Find the table controller from the seat's hierarchy.
        TableRiotController table =
            seat.GetComponentInParent<TableRiotController>();

        if (table == null)
        {
            Debug.LogError(
                $"No TableRiotController found for seat {seat.name}."
            );
            return;
        }

        seatedTable = table;

        // Seated -> sitting animation.
        if (spriteAnimator != null)
            spriteAnimator.PlaySitting();

        //OnCustomerSeated();

        // Register after the customer has been seated.
        table.RegisterCustomer(this);
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

    /*private void OnPatienceEmpty()
    {
        // Only the table where this customer is seated can riot.
        if (seatedTable != null)
        {
            seatedTable.NotifyCustomerPatienceEmpty(this);
        }

        // Queued customers do not cause riots.
    }*/

    private void OnPatienceEmpty()
    {
        // A customer still waiting in the queue leaves.
        if (owningQueue != null)
        {
            owningQueue.RemoveExpiredCustomer(this);
            return;
        }

        // A seated customer can trigger the table riot system.
        if (seatedTable != null)
        {
            seatedTable.NotifyCustomerPatienceEmpty(this);
        }
    }

    public void ForcePatienceToZero()
    {
        if (patienceEmpty)
            return;

        patience = 0f;
        patienceEmpty = true;

        //UpdatePatienceUI();
    }


    [Header("Seating Patience Bonus")]
    [SerializeField, Min(0f)]
    private float seatingPatienceBonus = 10f;

    public void OnCustomerSeated()
    {
        // A customer who already expired must remain at zero
        // so the table can detect and handle the riot.
        if (patienceEmpty || patienceExpired)
            return;

        patience = Mathf.Clamp(
            patience + seatingPatienceBonus,
            0f,
            maxPatience
        );

        //UpdatePatienceUI();
    }
    

}
