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

        private void Awake()
        {
            _canvas = GetComponentInChildren<Canvas>();
        }

        public void SetHealthBarPercentage(float percentage)
        {
            healthBar.value = percentage;
        }

        public void ShowDamagePopup(float damage)
        {
            var damagePopup = Instantiate(DamagePopupPrefab, transform.position, Quaternion.identity);
            var text = damagePopup.GetComponentInChildren<TextMeshProUGUI>();
            text.text = damage.ToString("0");
            damagePopup.transform.SetParent(_canvas.transform);
            Destroy(damagePopup, 1f);
        }
    }
}