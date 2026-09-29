using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TableUIManager : MonoBehaviour
{
    public static TableUIManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject tablePanel;
    [SerializeField] private Transform customerButtonContainer;
    [SerializeField] private Button customerButtonPrefab;
    [SerializeField] private TMP_Text tableTitleText;
    [SerializeField] private TMP_Text capacityText;

    [Header("References")]
    [SerializeField] private CustomerQueue customerQueue;

    private RestaurantTable currentTable;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (tablePanel != null)
            tablePanel.SetActive(false);
    }

    public void OpenTable(RestaurantTable table)
    {
        if (table == null || customerQueue == null)
            return;

        currentTable = table;

        if (tablePanel != null)
            tablePanel.SetActive(true);

        RefreshUI();
    }

    public void CloseTable()
    {
        currentTable = null;

        if (tablePanel != null)
            tablePanel.SetActive(false);
    }

    public void RefreshUI()
    {
        if (currentTable == null)
            return;

        ClearButtons();

        if (tableTitleText != null)
            tableTitleText.text = "Assign Customers";

        if (capacityText != null)
            capacityText.text =
                $"Seats: {currentTable.OccupiedSeats}/{currentTable.Capacity}";

        if (currentTable.IsFull)
            return;

        for (int i = 0; i < customerQueue.Count; i++)
        {
            Customer customer = customerQueue.GetCustomer(i);

            if (customer == null)
                continue;

            Button button = Instantiate(customerButtonPrefab, customerButtonContainer);

            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
            {
                buttonText.text =
                    $"{i + 1}. {customer.CustomerName} [{customer.Type}]";
            }

            Customer selectedCustomer = customer;

            button.onClick.AddListener(() =>
            {
                AssignCustomer(selectedCustomer);
            });
        }
    }

    private void AssignCustomer(Customer customer)
    {
        if (currentTable == null || customer == null)
            return;

        bool seated = currentTable.TrySeatCustomer(customer);

        if (!seated)
            return;

        // Customer disappears from queue because they teleported to the table.
        customerQueue.RemoveCustomer(customer);

        // Rebuild buttons so the queue indices update immediately.
        if (currentTable.IsFull || customerQueue.Count == 0)
        {
            CloseTable();
        }
        else
        {
            RefreshUI();
        }
    }

    private void ClearButtons()
    {
        if (customerButtonContainer == null)
            return;

        for (int i = customerButtonContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(customerButtonContainer.GetChild(i).gameObject);
        }
    }

    private void Update()
    {
        // Prototype convenience: Escape closes the table UI.
        if (tablePanel != null &&
            tablePanel.activeSelf &&
            Input.GetKeyDown(KeyCode.Escape))
        {
            CloseTable();
        }
    }
}
