using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// Attach to each table. A riot becomes a temporary follower that pursues
// the player, fires life-damaging projectiles, and drops collectible ingredients.
public class TableRiotController : MonoBehaviour
{
    [Header("Riot Effect")]
    [SerializeField] private GameObject riotEffectPrefab;
    [SerializeField] private Vector3 effectOffset;
    [SerializeField] private GameObject riotFollowerPrefab;
    [SerializeField] private Vector3 followerOffset = new Vector3(0f, 1f, 0f);
    [SerializeField, Min(0.1f)] private float followSpeed = 2.5f;

    [Header("Riot Duration")]
    [SerializeField, Min(0.1f)] private float riotDuration = 7f;
    [SerializeField] private GameObject riotBarCanvas;
    [SerializeField] private Slider riotDurationSlider;
    [SerializeField] private Vector3 timerOffset = new Vector3(0f, 1.5f, 0f);

    [Header("Projectile Attack")]
    [SerializeField] private RiotProjectile projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Transform player;
    [SerializeField, Min(0.05f)] private float projectileInterval = 0.75f;

    [Header("Ingredient Drops During Riot")]
    [SerializeField] private GameObject ingredientPickupPrefab;
    [SerializeField, Min(0.1f)] private float minDropInterval = 1.5f;
    [SerializeField, Min(0.1f)] private float maxDropInterval = 3f;
    [SerializeField] private float dropRadius = 0.7f;

    [Header("Debug")]
    [SerializeField] private bool logRiotEvents = true;

    private readonly List<Customer> seatedCustomers = new List<Customer>();
    private readonly List<GameObject> riotIngredientDrops = new List<GameObject>();
    private bool riotTriggered;
    private Coroutine riotRoutine;
    private Transform follower;
    private RestaurantTable table;

    public bool IsRioting => riotTriggered;
    public bool IsRiotActive => riotTriggered;
    public IReadOnlyList<Customer> SeatedCustomers => seatedCustomers;
    public TableUIManager tableUI;
    public AudioClip clip, clip2, clip3;

    private void Awake()
    {
        table = GetComponent<RestaurantTable>();
        if (projectileSpawnPoint == null) projectileSpawnPoint = transform;
        if (riotBarCanvas != null) riotBarCanvas.SetActive(false);
        if (riotDurationSlider != null)
        {
            riotDurationSlider.minValue = 0f;
            riotDurationSlider.maxValue = 1f;
            riotDurationSlider.value = 1f;
        }
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
        }
    }

    private void LateUpdate()
    {
        if (riotTriggered && follower != null && player != null && riotBarCanvas != null)
            riotBarCanvas.transform.position = follower.position + timerOffset;
    }

    public void RegisterCustomer(Customer customer)
    {
        if (customer == null || riotTriggered) return;
        if (!seatedCustomers.Contains(customer)) seatedCustomers.Add(customer);
        if (customer.Patience <= 0f) { TriggerRiot("A seated customer has zero patience."); return; }
        if (HasMixedCustomerTypes())
        {
            if (tableUI != null) tableUI.CloseTableSpecial();
            TriggerRiot("Customer group mismatch.");
            foreach (Customer seated in seatedCustomers)
                if (seated != null) seated.ForcePatienceToZero();
        }
    }

    public void NotifyCustomerPatienceEmpty(Customer customer)
    {
        if (customer != null && !riotTriggered && seatedCustomers.Contains(customer))
            TriggerRiot(customer.CustomerName + " reached zero patience.");
    }

    private bool HasMixedCustomerTypes()
    {
        CustomerType? first = null;
        foreach (Customer customer in seatedCustomers)
        {
            if (customer == null) continue;
            if (first == null) first = customer.Type;
            else if (customer.Type != first.Value) return true;
        }
        return false;
    }

    public void TriggerRiot(string reason)
    {
        if (riotTriggered) return;
        riotTriggered = true;
        if (AudioManager.Instance != null && clip != null) AudioManager.Instance.PlaySFX(clip);
        if (logRiotEvents) Debug.LogWarning(name + ": RIOT! " + reason);

        if (riotEffectPrefab != null)
        {
            GameObject effect = Instantiate(riotEffectPrefab, transform.position + effectOffset, Quaternion.identity);
            ParticleSystem ps = effect.GetComponentInChildren<ParticleSystem>();
            if (ps != null) Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
        }

        Vector3 startPosition = transform.position + followerOffset;
        GameObject followerObject = riotFollowerPrefab != null
            ? Instantiate(riotFollowerPrefab, startPosition, Quaternion.identity)
            : new GameObject(name + " Riot Follower");
        if (riotFollowerPrefab == null) followerObject.transform.position = startPosition;
        follower = followerObject.transform;

        if (riotBarCanvas != null) riotBarCanvas.SetActive(true);
        if (riotDurationSlider != null) riotDurationSlider.value = 1f;

        // Remove the table's customers immediately, without cash rewards.
        Customer[] removed = seatedCustomers.ToArray();
        seatedCustomers.Clear();
        foreach (Customer customer in removed)
        {
            if (customer == null) continue;
            customer.ClearSeatedTable(this);
            Destroy(customer.gameObject);
        }

        if (riotRoutine == null) riotRoutine = StartCoroutine(RiotSequence());
    }

    private IEnumerator RiotSequence()
    {
        float elapsed = 0f;
        float nextShot = 0f;
        float nextDrop = Random.Range(minDropInterval, Mathf.Max(minDropInterval, maxDropInterval));
        while (elapsed < riotDuration)
        {
            if (player == null)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) player = p.transform;
            }

            if (follower != null && player != null)
                follower.position = Vector3.MoveTowards(follower.position, player.position + followerOffset, followSpeed * Time.deltaTime);

            if (elapsed >= nextShot)
            {
                FireProjectile();
                nextShot = elapsed + projectileInterval;
            }
            if (elapsed >= nextDrop)
            {
                DropIngredientDuringRiot();
                nextDrop = elapsed + Random.Range(minDropInterval, Mathf.Max(minDropInterval, maxDropInterval));
            }

            elapsed += Time.deltaTime;
            if (riotDurationSlider != null) riotDurationSlider.value = Mathf.Clamp01(1f - elapsed / riotDuration);
            yield return null;
        }

        // Any uncollected ingredients vanish when this riot ends.
        foreach (GameObject pickup in riotIngredientDrops)
            if (pickup != null) Destroy(pickup);
        riotIngredientDrops.Clear();

        if (follower != null) Destroy(follower.gameObject);
        follower = null;
        if (riotBarCanvas != null) riotBarCanvas.SetActive(false);
        if (riotDurationSlider != null) riotDurationSlider.value = 1f;
        riotTriggered = false;
        riotRoutine = null;
        if (AudioManager.Instance != null && clip3 != null) AudioManager.Instance.PlaySFX(clip3);
        if (logRiotEvents) Debug.Log(name + ": Riot ended. Uncollected riot ingredients removed.");
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null || player == null) return;
        Vector3 spawn = follower != null ? follower.position : transform.position;
        RiotProjectile projectile = Instantiate(projectilePrefab, spawn, Quaternion.identity);
        projectile.Launch(player);
        if (AudioManager.Instance != null && clip2 != null) AudioManager.Instance.PlaySFX(clip2);
    }

    private void DropIngredientDuringRiot()
    {
        if (ingredientPickupPrefab == null) return;
        Vector2 offset = Random.insideUnitCircle * dropRadius;
        Vector3 origin = follower != null ? follower.position : transform.position;
        GameObject pickup = Instantiate(ingredientPickupPrefab, origin + new Vector3(offset.x, offset.y, 0f), Quaternion.identity);
        riotIngredientDrops.Add(pickup);
    }

    public void UnregisterCustomer(Customer customer)
    {
        if (customer != null) seatedCustomers.Remove(customer);
    }
}
