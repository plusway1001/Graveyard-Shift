
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

        // Serve the table first if enough dishes are available.
        // (A full table must still be servable.)
        TableService service = table.GetComponent<TableService>();

        if (service != null)
        {
            if (service.TryServe())
                return;

            // Do not open assignment UI while customers are eating.
            if (service.IsBusy)
                return;
        }

        // Only the seating UI needs a table that can accept customers.
        if (!table.CanAcceptCustomers)
            return;

        // Open the customer assignment UI.
        if (table.CanInteract && TableUIManager.Instance != null)
        {
            TableUIManager.Instance.OpenTable(table);
        }
    }

    /*public void Interact()
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
    }*/

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