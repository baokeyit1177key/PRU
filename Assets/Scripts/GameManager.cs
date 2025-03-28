using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject gameUi;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject gameOverMenu;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private UpgradeMenu upgradeMenu;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MainMenu();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void MainMenu()
    {
        mainMenu.SetActive(true);
        gameOverMenu.SetActive(false);
        pauseMenu.SetActive(false);
        upgradeMenu.Hide();
        Time.timeScale = 0f;
    }
    public void GameOverMenu()
    {
        gameOverMenu.SetActive(true);
        mainMenu.SetActive(false);
        pauseMenu.SetActive(false);
        upgradeMenu.Hide();
        Time.timeScale = 0f;
    }
    public void GamePauseMenu()
    {
        pauseMenu.SetActive(true);
        mainMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        upgradeMenu.Hide();
        Time.timeScale = 0f;
    }
    public void StartGame()
    {
        pauseMenu.SetActive(false);
        mainMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        upgradeMenu.Hide();
        Time.timeScale = 1f;
    }
    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        mainMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        upgradeMenu.Hide();
        Time.timeScale = 1f;
    }
    public void RetryGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void CompleteMap()
    {
        upgradeMenu.ShowUpgradeMenu();
        Time.timeScale = 0f;
    }
    public void ContinueToNextLevel()
    {
        // Resume game time
        Time.timeScale = 1f;

        // Hide the upgrade menu
        if (upgradeMenu != null)
        {
            upgradeMenu.Hide();
        }

        // Load the next scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
