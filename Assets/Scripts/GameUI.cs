using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
     private static GameUI instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Keep this UI across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate UI
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check if another GameUI exists and destroy it
        GameUI[] uiManagers = FindObjectsOfType<GameUI>();
        if (uiManagers.Length > 1)
        {
            Destroy(uiManagers[1].gameObject); // Destroy the extra one
        }
    }
    public void StartGame()
    {
        gameManager.StartGame();
    } 
    public void QuitGame()
    {
        gameManager.QuitGame();
    } 
    public void ContinueGame()
    {
        gameManager.ResumeGame();
    }  
    public void NextScene()
    {
        gameManager.ContinueToNextLevel();
    } 
    public void RetryGame()
    {
        gameManager.RetryGame();
    }
    public void MainMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
