using Game;
using UnityEngine;
using UnityEngine.SceneManagement;

// Controller for the main menu interactions
public class MainMenuController : MonoBehaviour
{
    public GameObject SettingsPage;
    public GameObject MainPage;
    public GameObject DifficultyPage;
    
    public void StartGame()
    {
        SceneManager.LoadScene("MainGame"); // Load the main game scene
    }
    // Method to open the instructions panel
    public void OpenSettings()
    {
        SettingsPage.SetActive(true);
        MainPage.SetActive(false);

    }

    // Method to close the instructions panel
    public void CloseSettings()
    {
        SettingsPage.SetActive(false); // Deactivate the settings panel
        MainPage.SetActive(true);
    }

    // Method to restart the game
    public void RestartGame()
    {
        GameManager.Instance.debugMode = true; // Enable debug mode
        UIManager.Instance.ShowPlayerUI(); // Show the player UI
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload the current scene
        gameObject.SetActive(false); // Deactivate the menu object
    }

    // Method to return to the main menu
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Load the main menu scene
        gameObject.SetActive(false); // Deactivate the menu object
    }

    // Opens the difficulty menu
    public void GoToDifficulty()
    {
        DifficultyPage.SetActive(true);
        MainPage.SetActive(false);
    }

    // Closes the difficulty page
    public void CloseDifficulty()
    {
        DifficultyPage.SetActive(false);
        MainPage.SetActive(true);
    }

    public void Quit()
    {
        Application.Quit();
    }
}