using System;
using UI;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    [SerializeField] private PlayerUI playerUI;
    [SerializeField] private CanvasSingleMessage victoryUI;
    [SerializeField] private CanvasSingleMessage defeatUI;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
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

    /// <summary>
    /// Fades the screen out to black over a period of time
    /// </summary>
    public void FadeScreenOut(float fadeTime = 1f)
    {
        
    }

    /// <summary>
    /// Shows the defeat screen with a specified message
    /// </summary>
    public void ShowDefeatScreen(string message = "")
    {
        defeatUI.SetMessage(message);
        defeatUI.gameObject.SetActive(true);
    }
    
    /// <summary>
    /// Shows the victory screen with a specific message
    /// </summary>
    public void ShowVictoryScreen(string message = "")
    {
        victoryUI.SetMessage(message);
        victoryUI.gameObject.SetActive(true);
    }
}