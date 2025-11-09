using UnityEngine;

public class EnemyStatsController : MonoBehaviour
{
    [Header("Estadísticas del enemigo")]
    public int maxHealth = 1;
    public int currentHealth;
    public int damage = 1;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void ReceiveDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Puedes agregar animación o efectos aquí
        Destroy(gameObject);
    }
}
