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
}
