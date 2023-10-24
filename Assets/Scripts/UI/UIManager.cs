using System;
using UnityEngine;

public class UIManager: MonoBehaviour
{
    public static UIManager Instance;
    
    [SerializeField] private PlayerUI playerUI;

    private void Awake()
    {
        if (Instance is not null)
        {
            DestroyImmediate(this);
            return;
        }
        
        Instance = this;
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
}