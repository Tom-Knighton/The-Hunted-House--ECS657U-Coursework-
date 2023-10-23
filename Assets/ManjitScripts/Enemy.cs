using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float health = 100.0f;

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Handle enemy death, e.g., play animation, remove from scene, etc.
        Destroy(gameObject);
    }
}
