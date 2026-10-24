using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Put on each Table (next to RestaurantTable).
// Flow: customers seated -> player serves ALL of them at once
// (needs dishes >= customers) -> eating bar fills -> customers leave
// 1 cash each on the table -> seats free up.
public class TableService : MonoBehaviour
{
    public enum TableState { Empty, Waiting, Eating }

    [SerializeField] private float eatDuration = 3f;
    [SerializeField] private CashPickup cashPrefab;
    [SerializeField] private Vector2 eatingBarOffset = new Vector2(0f, 1.6f);
    [SerializeField] private float cashSpread = 0.5f;

    [Tooltip("Hook your RestaurantTable's 'free all seats' method here")]
    public UnityEvent onTableCleared;

    public TableState State { get; private set; } = TableState.Empty;
    public int CustomerCount => seated.Count;
    public bool IsBusy => State == TableState.Eating;   // block seating while eating

    private readonly List<GameObject> seated = new List<GameObject>();
    private WorldBar eatingBar;
    private RestaurantTable table;
    public TableUIManager tableUI;

    public AudioClip clip, clip2;

    private void Awake()
    {
        table = GetComponent<RestaurantTable>();
    }

    private void Start()
    {
        eatingBar = WorldBar.Create("EatingBar", (Vector2)transform.position + eatingBarOffset,
                                    new Color(0.55f, 1f, 0.45f), 1.6f);
        eatingBar.Hide();
    }

    // Call this from your seating code when a customer sits here
    public void RegisterCustomer(GameObject customer)
    {
        if (customer == null || IsBusy || seated.Contains(customer)) return;
        seated.Add(customer);
        State = TableState.Waiting;
    }

    // Removes a customer who left early (e.g. ran out of patience)
    public void UnregisterCustomer(GameObject customer)
    {
        seated.Remove(customer);
        if (seated.Count == 0 && State == TableState.Waiting) State = TableState.Empty;
    }

    // Picks up every customer the RestaurantTable says is seated,
    // so no one is missed however they were seated.
    private void SyncFromTable()
    {
        if (table == null || State == TableState.Eating) return;
        foreach (var c in table.SeatedCustomers)
        {
            if (c != null && !seated.Contains(c.gameObject))
                seated.Add(c.gameObject);
        }
        if (seated.Count > 0 && State == TableState.Empty)
            State = TableState.Waiting;
    }

    public bool CanServe()
    {
        SyncFromTable();
        seated.RemoveAll(c => c == null);
        return State == TableState.Waiting && seated.Count > 0
               && PlayerInventory.Instance != null
               && PlayerInventory.Instance.DishesOnHand >= seated.Count;
    }

    // Called by PlayerTableInteractor. Returns true if it served the table.
    public bool TryServe()
    {
        if (!CanServe()) return false;
        PlayerInventory.Instance.TryTakeDishes(seated.Count);
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clip);
        }
        else
        {
            Debug.LogWarning("AudioManager instance is missing from the scene!");
        }
        StartCoroutine(EatRoutine());
        return true;
    }

    private IEnumerator EatRoutine()
    {
        State = TableState.Eating;

        tableUI.CloseTable();

        // Prevent new customers from being assigned while eating.
        if (table != null)
            table.SetEating(true);

        foreach (var c in seated)
        {
            // Lets the customer's own script stop its patience timer:
            // add  void OnServed() { ... }  to your customer script.
            c.SendMessage("OnServed", SendMessageOptions.DontRequireReceiver);
            StartEatingAnimation(c);
        }

        eatingBar.Show();
        float t = 0f;
        while (t < eatDuration)
        {
            t += Time.deltaTime;
            eatingBar.SetProgress(t / eatDuration);
            yield return null;
        }
        eatingBar.Hide();

        // 1 cash per customer, left on the table
        for (int i = 0; i < seated.Count; i++)
        {
            if (cashPrefab == null) break;
            Vector2 offset = new Vector2(
                (i - (seated.Count - 1) / 2f) * cashSpread, Random.Range(-0.15f, 0.15f));
            Instantiate(cashPrefab, (Vector2)transform.position + offset, Quaternion.identity);
        }

        foreach (var c in seated)
            if (c != null) Destroy(c);
        seated.Clear();

        State = TableState.Empty;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clip2);
        }
        else
        {
            Debug.LogWarning("AudioManager instance is missing from the scene!");
        }

        // Allow new customers to be assigned again.
        if (table != null)
            table.SetEating(false);

        onTableCleared?.Invoke();
    }

    private void StartEatingAnimation(GameObject customer)
    {
        Animator anim = customer.GetComponentInChildren<Animator>();
        if (anim != null && HasTrigger(anim, "Eat"))
        {
            anim.SetTrigger("Eat");
            return;
        }
        // Fallback when there's no "Eat" animation yet: a chomping bob
        customer.AddComponent<EatBob>();
    }

    private static bool HasTrigger(Animator anim, string name)
    {
        if (anim.runtimeAnimatorController == null) return false;
        foreach (var p in anim.parameters)
            if (p.name == name && p.type == AnimatorControllerParameterType.Trigger) return true;
        return false;
    }
}

// Placeholder eating animation: squash-and-stretch chomp
public class EatBob : MonoBehaviour
{
    private Vector3 baseScale;
    private void Start() => baseScale = transform.localScale;
    private void Update()
    {
        float s = Mathf.Sin(Time.time * 18f) * 0.08f;
        transform.localScale = new Vector3(baseScale.x * (1f + s), baseScale.y * (1f - s), baseScale.z);
    }
}
