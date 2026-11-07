using UnityEngine;

// Place on a hiding box with a trigger collider. Player stays protected while inside.
public class HidingSpot : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health != null) health.SetHiding(true);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health != null) health.SetHiding(false);
    }
}
