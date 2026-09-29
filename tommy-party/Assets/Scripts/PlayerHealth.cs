using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    
    [Header("Health")]
    public int maxHealth = 10;

    private int currentHealth;

    [Header("UI")]
    public TextMeshProUGUI healthText;

    private void Start()
    {
        currentHealth = maxHealth;

        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();

        Debug.Log("Player HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthUI()
    {
        healthText.text = "HP: " + currentHealth;
    }

    void Die()
    {
        Debug.Log("Player Died");

        Destroy(gameObject);
    }

    
}
