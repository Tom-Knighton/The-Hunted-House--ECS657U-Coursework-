using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    // Serialized fields for UI elements
    [SerializeField] private Slider healthBar = default;
    [SerializeField] private Slider staminaBar = default;
    [SerializeField] private Slider attackCooldownBar = default;
    [SerializeField] private TextMeshProUGUI healthText = default;

    [SerializeField] private FirstPersonController player;
    

    // Initialize UI elements when the component starts
    private void Start()
    {
        UpdateHealth(100);
        UpdateStamina(100);
        UpdateAttackCooldown(100);
    }

    // Update the health UI elements
    public void UpdateHealth(float currentHealth)
    {
        healthBar.value = currentHealth; // Update the health slider value
        healthText.text = currentHealth.ToString("00"); // Display the health value as text
    }

    // Update the stamina UI elements
    public void UpdateStamina(float currentStamina)
    {
        staminaBar.value = currentStamina; // Update the stamina slider value
        staminaBar.gameObject.SetActive(currentStamina < 100); // Hide the stamina bar if stamina is full
    }

    // Update the attack cooldown UI elements
    public void UpdateAttackCooldown(float currentCooldown)
    {
        attackCooldownBar.value = currentCooldown; // Update the attack cooldown slider value
        attackCooldownBar.gameObject.SetActive(currentCooldown > 0); // Hide the cooldown bar if there's no cooldown
    }
}
