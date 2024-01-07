using UnityEngine.SceneManagement;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public GameObject settingsUI; // Reference to the settings UI

    // Call this method when the pause button is pressed
    public void TogglePauseMenu()
    {
        if (Time.timeScale == 0)
        {
            ResumeGame();
            UIManager.Instance.ShowPlayerUI(); // Re-enable the player's UI
        }
        else
        {
            PauseGame();
            UIManager.Instance.HidePlayerUI(); // Disable the player's UI
        }
    }


    public void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        settingsUI.SetActive(false); // Hide settings initially
        FirstPersonController.instance.ToggleMove();
        Time.timeScale = 0; // Pause the game
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1; // Resume the game

        // Re-enable the player's UI
        UIManager.Instance.ShowPlayerUI();

        // Re-enable the player controls
        FirstPersonController.instance.ToggleMove();
        FirstPersonController.instance.LoadBindingOverrides();
        UIManager.Instance.RefreshInteractText();
    }


    public void OpenSettings()
    {
        settingsUI.SetActive(true); // Show settings
        // Optionally, move settings UI to be a child of the pause menu UI
        settingsUI.transform.SetParent(pauseMenuUI.transform, false);
    }

    public void CloseSettings()
    {
        settingsUI.SetActive(false); // Hide settings
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1; // Ensure game speed is normal
        SceneManager.LoadScene("MainMenu"); // Load the main menu scene
    }
}
