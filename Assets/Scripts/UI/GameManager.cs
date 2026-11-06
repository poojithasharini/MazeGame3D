using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Level Progress")]
    public int CurrentLevelNumber = 1;
    public int TotalLevels = 3;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void OnLevelComplete()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.TriggerLevelComplete();
        }
    }

    public void LoadLevel(int level)
    {
        CurrentLevelNumber = Mathf.Clamp(level, 1, TotalLevels);
        Time.timeScale = 1f;

        // Load scene based on level number
        string sceneName = "Level" + CurrentLevelNumber;
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            SceneManager.LoadScene("MazeGameScene");
        }
    }

    public void LoadNextLevel()
    {
        if (CurrentLevelNumber < TotalLevels)
        {
            LoadLevel(CurrentLevelNumber + 1);
        }
        else
        {
            LoadVictoryScene();
        }
    }

    public void LoadVictoryScene()
    {
        Time.timeScale = 1f;
        if (Application.CanStreamedLevelBeLoaded("VictoryScene"))
        {
            SceneManager.LoadScene("VictoryScene");
        }
        else
        {
            LoadMainMenu();
        }
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        if (Application.CanStreamedLevelBeLoaded("MainMenuScene"))
        {
            SceneManager.LoadScene("MainMenuScene");
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }
}
