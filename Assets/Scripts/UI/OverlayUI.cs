using System;
using TMPro;
using UnityEngine;

namespace UI
{
    public class OverlayUI: MonoBehaviour
    {
        public GameObject HintContainer;
        [SerializeField] TextMeshProUGUI fullScreenMessage;
        
        private TextMeshProUGUI _hintText;
        
        
        private void Start()
        {
            _hintText = HintContainer.GetComponentInChildren<TextMeshProUGUI>();
        }

        public void ShowHint(string message)
        {
            _hintText.text = message;
            HintContainer.SetActive(true);
        }

        public void HideHint()
        {
            _hintText.text = string.Empty;
            HintContainer.SetActive(false);
        }

        public void SetFullScreenMessage(string message)
        {
            fullScreenMessage.text = message;
        }
    }
}