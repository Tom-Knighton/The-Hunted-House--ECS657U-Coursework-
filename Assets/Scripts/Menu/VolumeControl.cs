using UnityEngine;
using UnityEngine.UI; // For UI components like Slider.
using TMPro; // For TextMeshPro components.
using Game;

public class VolumeControl : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider; // Reference to the volume slider.
    [SerializeField] private TextMeshProUGUI volumeText; // Reference to the TextMeshProUGUI that displays the volume.

    private void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat("Volume", 1f) * 100; // Get saved volume
        volumeSlider.value = savedVolume; // Set the slider's value
        volumeText.text = savedVolume.ToString("0"); // Set the volume text

        volumeSlider.onValueChanged.AddListener(HandleVolumeChanged);
    }

    private void HandleVolumeChanged(float value)
    {
        float volume = value / 100;
        AudioManager.Instance.SetVolume(volume); // Set the volume
        PlayerPrefs.SetFloat("Volume", volume); // Save the volume setting
        volumeText.text = value.ToString("0"); // Update the volume text
    }

    private void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks.
        volumeSlider.onValueChanged.RemoveListener(HandleVolumeChanged);
    }
}
