using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject playerUI;

    public void StartGame()
    {
        if (PlayerUI.Instance != null)
        {
            PlayerUI.Instance.gameObject.SetActive(true);
        }
        SceneManager.LoadScene("HouseTest");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // Methods for the in-game UI:

    public void RestartGame()
    {
        PlayerUI.Instance.gameObject.SetActive(true);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
