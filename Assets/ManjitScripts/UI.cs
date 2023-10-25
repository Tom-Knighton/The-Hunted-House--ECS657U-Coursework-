using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public static PlayerUI Instance;

    // Serialized fields for UI elements
    [SerializeField] private Slider healthBar = default;
    [SerializeField] private Slider staminaBar = default;
    [SerializeField] private Slider attackCooldownBar = default;
    [SerializeField] private TextMeshProUGUI healthText = default;

<<<<<<< Updated upstream:Assets/ManjitScripts/UI.cs
    // Subscribe to events when the component is enabled
    private void OnEnable()
    {
        FirstPersonController.OnDamage += UpdateHealth;
        FirstPersonController.OnHeal += UpdateHealth;
        FirstPersonController.OnStaminaChange += UpdateStamina;
        FirstPersonController.OnAttackCooldown += UpdateAttackCooldown;
    }

    // Unsubscribe from events when the component is disabled
    private void OnDisable()
    {
        FirstPersonController.OnDamage -= UpdateHealth;
        FirstPersonController.OnHeal -= UpdateHealth;
        FirstPersonController.OnStaminaChange -= UpdateStamina;
        FirstPersonController.OnAttackCooldown -= UpdateAttackCooldown;
    }
=======
    [SerializeField] private FirstPersonController player;

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

>>>>>>> Stashed changes:Assets/ManjitScripts/PlayerUI.cs

    // Initialize UI elements when the component starts
    private void Start()
    {
        UpdateHealth(100);
        UpdateStamina(100);
        UpdateAttackCooldown(100);
    }

    // Update the health UI elements
    private void UpdateHealth(float currentHealth)
    {
        healthBar.value = currentHealth; // Update the health slider value
        healthText.text = currentHealth.ToString("00"); // Display the health value as text
    }

    // Update the stamina UI elements
    private void UpdateStamina(float currentStamina)
    {
        staminaBar.value = currentStamina; // Update the stamina slider value
        staminaBar.gameObject.SetActive(currentStamina < 100); // Hide the stamina bar if stamina is full
    }

    // Update the attack cooldown UI elements
    private void UpdateAttackCooldown(float currentCooldown)
    {
        attackCooldownBar.value = currentCooldown; // Update the attack cooldown slider value
        attackCooldownBar.gameObject.SetActive(currentCooldown > 0); // Hide the cooldown bar if there's no cooldown
    }
}
