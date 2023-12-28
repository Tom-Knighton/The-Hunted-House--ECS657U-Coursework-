using Game;
using UnityEngine;
using UnityEngine.SceneManagement;

// Controller for the main menu interactions
public class MainMenuController : MonoBehaviour
{

    public GameObject instructionsPanel;
    public void StartGame()
    {
        GameManager.Instance.debugMode = false; // Ensure debug mode is off
        SceneManager.LoadScene("MainGame"); // Load the main game scene
    }
    // Method to open the instructions panel
    public void OpenInstructions()
    {
        instructionsPanel.SetActive(true);
    }

    // Method to close the instructions panel
    public void CloseInstructions()
    {
        instructionsPanel.SetActive(false); // Deactivate the instructions panel
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
}