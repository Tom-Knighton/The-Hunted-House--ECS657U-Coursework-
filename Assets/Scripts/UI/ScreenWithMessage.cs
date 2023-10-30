using TMPro;
using UnityEngine;

namespace UI
{
    public class CanvasSingleMessage: MonoBehaviour
    {
        [SerializeField] public TextMeshProUGUI messageField;

        /// <summary>
        /// Sets the message on the screen
        /// </summary>
        public void SetMessage(string message)
        {
            messageField.text = message;
        }
    }
}