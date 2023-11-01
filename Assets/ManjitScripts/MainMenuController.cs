using Game;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void StartGame()
    {
        GameManager.debugMode = false;
        SceneManager.LoadScene("HouseTest");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // Methods for the in-game UI:
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