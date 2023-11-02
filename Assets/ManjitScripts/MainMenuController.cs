using Game;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{

    public GameObject instructionsPanel;
    public void StartGame()
    {
        GameManager.debugMode = false;
        SceneManager.LoadScene("HouseTest");
    }
    public void OpenInstructions()
    {
        instructionsPanel.SetActive(true);
    }

    public void CloseInstructions()
    {
        instructionsPanel.SetActive(false);
    }

    public void RestartGame()
    {
        GameManager.debugMode = true;
        UIManager.Instance.ShowPlayerUI();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        gameObject.SetActive(false);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        gameObject.SetActive(false);
    }
}