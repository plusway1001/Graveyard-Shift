
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    public AudioClip clip, clip2;

    [SerializeField] private KeyCode interactKey = KeyCode.E;

    public float SetVolumeLow = 0.3f;

    private bool OpenTableUI = false;

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

        if (!table.CanAcceptCustomers || !table.CanInteract)
        {
            Debug.Log("Cannot open assignment UI: table is unavailable.");
            return;
        }

        TableService service = table.GetComponent<TableService>();

        if (service != null && service.IsBusy)
            return;

        currentTable = table;

        if (tablePanel != null)
            tablePanel.SetActive(true);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clip);
        }
        else
        {
            Debug.LogWarning("AudioManager instance is missing from the scene!");
        }

        OpenTableUI = true;

        RefreshUI();
    }

    // Connect this method to the mobile X button.
    public void CloseTable()
    {
        currentTable = null;

        ClearButtons();

        if (tablePanel != null)
            tablePanel.SetActive(false);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clip);
        }
        else
        {
            Debug.LogWarning("AudioManager instance is missing from the scene!");
        }

        OpenTableUI = false;
    }

    public void RefreshUI()
    {
        if (currentTable == null || customerQueue == null)
            return;

        ClearButtons();

        if (tableTitleText != null)
            tableTitleText.text = "Assign Customers";

        if (capacityText != null)
        {
            capacityText.text =
                $"Seats: {currentTable.OccupiedSeats}/{currentTable.Capacity}";
        }

        if (currentTable.IsFull)
            return;

        for (int i = 0; i < customerQueue.Count; i++)
        {
            Customer customer = customerQueue.GetCustomer(i);

            if (customer == null)
                continue;

            Button button = Instantiate(
                customerButtonPrefab,
                customerButtonContainer
            );

            TMP_Text buttonText =
                button.GetComponentInChildren<TMP_Text>();

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

        // Prevent assignment if the table became unavailable
        // while the UI was open.
        if (!currentTable.CanAcceptCustomers ||
            !currentTable.CanInteract)
        {
            CloseTable();
            return;
        }

        if (!customerQueue.Customers.Contains(customer))
        {
            RefreshUI();
            return;
        }

        bool seated = currentTable.TrySeatCustomer(customer);

        if (!seated)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(clip2);
            }
            else
            {
                Debug.LogWarning("AudioManager instance is missing from the scene!");
            }
            RefreshUI();
            return;
        }

        customerQueue.RemoveCustomer(customer);

        if (currentTable == null)
            return;

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
            GameObject button =
                customerButtonContainer.GetChild(i).gameObject;

            button.SetActive(false);
            Destroy(button);
        }
    }

    private void Update()
    {
        if (OpenTableUI)
        {
            AudioManager.Instance.SetVolume(SetVolumeLow);
        }
        else
        {
            if (!PauseMenu.isPaused)
            {
                AudioManager.Instance.SetVolume(1f);
            }
        }

        if (tablePanel == null || !tablePanel.activeSelf)
            return;

        // PC controls: press interactkey to close.
        if (Input.GetKeyDown(interactKey))
        {
            CloseTable();
            return;
        }

        // Automatically close if the selected table is destroyed,
        // starts rioting, begins eating, or becomes unavailable.
        if (currentTable == null ||
            !currentTable.CanAcceptCustomers ||
            !currentTable.CanInteract)
        {
            CloseTable();
            return;
        }

        TableService service = currentTable.GetComponent<TableService>();

        if (service != null && service.IsBusy)
        {
            CloseTable();
        }
    }
}


/*using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        if (table == null || !table.CanAcceptCustomers)
        {
            Debug.Log("Cannot open assignment UI: table is eating or rioting.");
            return;
        }
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

        if (!customerQueue.Customers.Contains(customer))
        {
            RefreshUI();
            return;
        }

        bool seated = currentTable.TrySeatCustomer(customer);

        if (!seated)
        {
            RefreshUI();
            return;
        }

        customerQueue.RemoveCustomer(customer);

        // A riot may have destroyed the seated customer immediately.
        // Refresh the UI after the seating operation.
        if (currentTable == null)
            return;

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
            GameObject button =
                customerButtonContainer.GetChild(i).gameObject;

            // Hide immediately so old buttons don't remain visible.
            button.SetActive(false);

            Destroy(button);
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
}*/
