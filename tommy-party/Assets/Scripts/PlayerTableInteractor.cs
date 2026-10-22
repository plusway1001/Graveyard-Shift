using UnityEngine;

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

        if (table != null && table.CanInteract)
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

            if (table == null || !table.CanInteract)
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
