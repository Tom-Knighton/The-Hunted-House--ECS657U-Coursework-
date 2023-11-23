using System;
using TMPro;
using UnityEngine;

namespace UI
{
    public class OverlayUI: MonoBehaviour
    {
        public GameObject HintContainer;
        [SerializeField] TextMeshProUGUI fullScreenMessage;
        [SerializeField] private TextMeshProUGUI countdownMessage;
        [SerializeField] private GameObject countdownContainer;
        
        private TextMeshProUGUI _hintText;
        
        
        private void Start()
        {
            _hintText = HintContainer.GetComponentInChildren<TextMeshProUGUI>();
        }

        // Displays a hint message
        public void ShowHint(string message)
        {
            _hintText.text = message;
            HintContainer.SetActive(true);
        }

        // Hides the hint message
        public void HideHint()
        {
            _hintText.text = string.Empty;
            HintContainer.SetActive(false);
        }

        // Sets the full screen message text
        public void SetFullScreenMessage(string message)
        {
            fullScreenMessage.text = message;
        }

        // Updates the countdown timer display
        public void UpdateTimeLeft(TimeSpan timeSpan)
        {
            countdownMessage.text = $"Police arrive in: {timeSpan.Hours}h {timeSpan.Minutes}m";
        }
        
        // Sets the visibility of the overlay UI
        public void SetOverlayVisibility(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }
        
        // Sets the visibility of the countdown timer
        public void SetCountdownVisibility(bool isVisible)
        {
            countdownContainer.SetActive(isVisible);
        }
    }
}