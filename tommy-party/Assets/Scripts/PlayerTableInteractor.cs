
using UnityEngine;

public class PlayerTableInteractor : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 1.5f;
    [SerializeField] private LayerMask tableLayer;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private void Update()
    {
        // PC controls: press E.
        if (Input.GetKeyDown(interactKey))
        {
            Interact();
        }
    }

    // Called by both the PC keyboard and mobile UI button.
    public void Interact()
    {
        RestaurantTable table = FindClosestTable();

        if (table == null)
            return;

        // Do not interact with a table that is rioting,
        // eating, or otherwise unavailable.
        if (!table.CanAcceptCustomers)
            return;

        // Serve the table first if enough dishes are available.
        TableService service = table.GetComponent<TableService>();

        if (service != null)
        {
            if (service.TryServe())
                return;

            // Do not open assignment UI while customers are eating.
            if (service.IsBusy)
                return;
        }

        // Open the customer assignment UI.
        if (table.CanInteract && TableUIManager.Instance != null)
        {
            TableUIManager.Instance.OpenTable(table);
        }
    }

    private RestaurantTable FindClosestTable()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            interactionDistance,
            tableLayer
        );

        RestaurantTable closest = null;
        float closestDistance = Mathf.Infinity;

        for (int i = 0; i < hits.Length; i++)
        {
            RestaurantTable table =
                hits[i].GetComponentInParent<RestaurantTable>();

            if (table == null)
                continue;

            float distance = Vector2.Distance(
                transform.position,
                table.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = table;
            }
        }

        return closest;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}


/*using UnityEngine;

public class PlayerTableInteractor : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 1.5f;
    [SerializeField] private LayerMask tableLayer;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private void Update()
    {
        if (!Input.GetKeyDown(interactKey))
            return;

        RestaurantTable table = FindClosestTable();

        if (table == null)
            return;

        // NEW: serve the table first if the player has enough dishes
        TableService service = table.GetComponent<TableService>();
        if (service != null)
        {
            if (service.TryServe())
                return;           // served, don't open the seating menu

            if (service.IsBusy)
                return;           // customers are eating
        }

        // Otherwise, open the seating menu like before
        if (table.CanInteract)
        {
            TableUIManager.Instance.OpenTable(table);
        }
    }

    // NEW: finds the closest table even when it's full,
    // so a full table can still be served.
    private RestaurantTable FindClosestTable()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            interactionDistance,
            tableLayer
        );

        RestaurantTable closest = null;
        float closestDistance = Mathf.Infinity;

        for (int i = 0; i < hits.Length; i++)
        {
            RestaurantTable table =
                hits[i].GetComponentInParent<RestaurantTable>();

            if (table == null)
                continue;

            float distance =
                Vector2.Distance(transform.position, table.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = table;
            }
        }

        return closest;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}*/