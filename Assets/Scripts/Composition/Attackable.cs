using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Attackable: MonoBehaviour
{
    /// <summary>
    /// The max health the entity can have
    /// </summary>
    public float maxHealth = 100f;
   
    /// <summary>
    /// The current health of the entity
    /// </summary>
    public float health  = 100f;
    
    /// <summary>
    /// The amount the entity heals by each regenWait period
    /// </summary>
    public float regenRate  = 0f;
    
    /// <summary>
    /// The amount of time between each health regen
    /// </summary>
    public float regenWait  = 1f;
    
    /// <summary>
    /// Whether or not the entity is allowed to regen health
    /// </summary>
    public bool canRegenHealth = true;
    
    
    private const float MinHealth = 0;
    private bool _isRegeningHealth = false;
    
    /// <summary>
    /// Whether or not the entity is dead
    /// </summary>
    public bool IsDead => health <= MinHealth;

    /// <summary>
    /// Raised when the health value changes, with the new health value and the damage taken
    /// </summary>
    public UnityEvent<float, float> OnHealthChanged;

    /// <summary>
    /// Raised when the entity's health reaches 0 or below
    /// </summary>
    public UnityEvent OnDeath;

    /// <summary>
    /// Attacks the entity with the given damage, destroying it if it's dead
    /// </summary>
    public void Attack(float damage) 
    {
        health -= damage;
        OnHealthChanged?.Invoke(health, damage);

        if (IsDead)
        {
            OnDeath?.Invoke();

            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        if (canRegenHealth && !_isRegeningHealth && health < maxHealth)
        {
            RegenHealth();
        }
    }

    private void RegenHealth()
    {
        StartCoroutine(AddHealth());
        return;
        
        IEnumerator AddHealth()
        {
            _isRegeningHealth = true;
            Attack(-regenRate);
            yield return new WaitForSeconds(5);
            _isRegeningHealth = false;
        }
    }
}

