using System;
using UnityEngine;

public class UIManager: MonoBehaviour
{
    public static UIManager Instance;
    
    [SerializeField] private PlayerUI playerUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {

            Debug.LogWarning("Multiple instances of UIManager detected. Deleting the new instance.");

            Destroy(gameObject);
            return;
        }
    }


    public void ShowPlayerUI()
    {
        playerUI.enabled = true;
    }

    public void HidePlayerUI()
    {
        playerUI.enabled = false;
    }
        
    public void UpdatePlayerHealth(float health, float maxHealth)
    {
        var healthPercentage = (health / maxHealth) * 100f;
        playerUI.UpdateHealth(healthPercentage);
    }   
        
    public void UpdatePlayerStamina(float stamina, float maxStamina)
    {
        var staminaPercentage = (stamina / maxStamina) * 100f;
        playerUI.UpdateStamina(staminaPercentage);
    }

    public void UpdateAttackCooldownPercentage(float percentage)
    {
        playerUI.UpdateAttackCooldown(percentage);
    }
}