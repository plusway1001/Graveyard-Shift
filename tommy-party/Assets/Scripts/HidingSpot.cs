using UnityEngine;
using UnityEngine.UI;

// Place on a hiding box with a trigger collider. Player stays protected while inside.
// The box has health, shows a world-space health bar, and breaks when health reaches 0.
public class HidingSpot : MonoBehaviour
{
    [Header("Health")]
    [SerializeField, Min(1)]
    private int maxHealth = 3;

    [Header("Health Bar (World Space)")]
    [Tooltip("Parent object of the bar (the world-space Canvas).")]
    [SerializeField]
    private GameObject healthBarRoot;

    [Tooltip("Image with Image Type = Filled, Fill Method = Horizontal.")]
    [SerializeField]
    private Image healthFill;

    [Tooltip("If true, the bar only appears after the box takes its first hit.")]
    [SerializeField]
    private bool hideBarUntilDamaged = true;

    [Tooltip("Keeps the bar upright even if the box is rotated.")]
    [SerializeField]
    private bool keepBarUpright = true;

    [Header("Break Effects")]
    [SerializeField]
    private GameObject breakEffects;

    [SerializeField]
    private AudioClip breakClip;

    private int currentHealth;
    private bool isBroken;
    private PlayerHealth playerInside;

    private void Awake()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();

        if (healthBarRoot != null)
            healthBarRoot.SetActive(!hideBarUntilDamaged);
    }

    private void LateUpdate()
    {
        if (keepBarUpright && healthBarRoot != null)
            healthBarRoot.transform.rotation = Quaternion.identity;
    }

    // Called by the projectile when it hits this box.
    public void TakeDamage(int amount)
    {
        if (isBroken) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);

        if (healthBarRoot != null)
            healthBarRoot.SetActive(true);

        UpdateHealthBar();

        if (currentHealth <= 0)
            Break();
    }

    private void UpdateHealthBar()
    {
        if (healthFill != null)
            healthFill.fillAmount = (float)currentHealth / maxHealth;
    }

    private void Break()
    {
        isBroken = true;

        if (healthBarRoot != null)
            Destroy(healthBarRoot);

        // OnTriggerExit2D is not reliable when the object is destroyed,
        // so make sure the player is no longer "hiding".
        if (playerInside != null)
        {
            playerInside.SetHiding(false);
            playerInside = null;
        }

        if (breakEffects != null)
            Instantiate(breakEffects, transform.position, Quaternion.identity);

        if (AudioManager.Instance != null && breakClip != null)
            AudioManager.Instance.PlaySFX(breakClip);

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isBroken || !other.CompareTag("Player")) return;

        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health != null)
        {
            playerInside = health;
            health.SetHiding(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health != null)
        {
            health.SetHiding(false);
            if (playerInside == health) playerInside = null;
        }
    }
}

/*using UnityEngine;

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
}*/
