using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SensitivitySettings : MonoBehaviour
{
    [SerializeField] private Slider xSensitivitySlider;
    [SerializeField] private Slider ySensitivitySlider;
    [SerializeField] private TextMeshProUGUI xSensitivityText;
    [SerializeField] private TextMeshProUGUI ySensitivityText;

    private const float DefaultSensitivity = 1f; // Default multiplier

    private void Start()
    {
        // Load the saved values or default to DefaultSensitivity if they don't exist
        xSensitivitySlider.value = PlayerPrefs.GetFloat("XSensitivity", DefaultSensitivity);
        ySensitivitySlider.value = PlayerPrefs.GetFloat("YSensitivity", DefaultSensitivity);

        // Update the text to display the current multiplier
        xSensitivityText.text = xSensitivitySlider.value.ToString("0.0") + "x";
        ySensitivityText.text = ySensitivitySlider.value.ToString("0.0") + "x";

        // Add listeners for the sliders
        xSensitivitySlider.onValueChanged.AddListener(HandleXSensitivityChanged);
        ySensitivitySlider.onValueChanged.AddListener(HandleYSensitivityChanged);
    }

    private void HandleXSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat("XSensitivity", value);
        PlayerPrefs.Save(); // Save the changes immediately
        xSensitivityText.text = value.ToString("0.0") + "x"; // Update the text display
    }

    private void HandleYSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat("YSensitivity", value);
        PlayerPrefs.Save(); // Save the changes immediately
        ySensitivityText.text = value.ToString("0.0") + "x"; // Update the text display
    }

    // This method will be called when the "Confirm" button is clicked
    public void ConfirmSensitivitySettings()
    {
        // Save the current slider values
        PlayerPrefs.SetFloat("XSensitivity", xSensitivitySlider.value);
        PlayerPrefs.SetFloat("YSensitivity", ySensitivitySlider.value);
        PlayerPrefs.Save();

        // Optionally, update other parts of the game that depend on these settings
        // For example, you might need to update the sensitivity settings of the player controller
        if (FirstPersonController.instance != null)
        {
            FirstPersonController.instance.UpdateLookSensitivity(xSensitivitySlider.value, ySensitivitySlider.value);
        }
    }
}
