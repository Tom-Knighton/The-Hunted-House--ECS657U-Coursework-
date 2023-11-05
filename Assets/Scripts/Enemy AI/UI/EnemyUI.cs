using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Enemy_AI.UI
{
    public class EnemyUI: MonoBehaviour
    {

        [SerializeField] private Slider healthBar;
        [SerializeField] private GameObject DamagePopupPrefab;

        private Canvas _canvas;

        // Initialize the canvas reference
        private void Awake()
        {
            _canvas = GetComponentInChildren<Canvas>();
        }

        // Update the health bar's value
        public void SetHealthBarPercentage(float percentage)
        {
            healthBar.value = percentage;
        }

        // Instantiate and show the damage popup
        public void ShowDamagePopup(float damage)
        {
            // Create the popup at the enemy's position with no rotation
            var damagePopup = Instantiate(DamagePopupPrefab, transform.position, Quaternion.identity);
            // Get the TMP component and set the damage text
            var text = damagePopup.GetComponentInChildren<TextMeshProUGUI>();
            text.text = damage.ToString("0");
            // Set the popup's parent to the canvas so it's displayed properly
            damagePopup.transform.SetParent(_canvas.transform);
            // Destroy the popup after 1 second
            Destroy(damagePopup, 1f);
        }
    }
}