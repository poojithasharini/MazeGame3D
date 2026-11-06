using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Top-Right Objective Banner (Slide 3)")]
    public GameObject ObjectivePanel;
    public Image ObjectiveIcon;
    public Text ObjectiveTitleText;
    public Text ObjectiveSubtitleText;
    public Text ExitDistanceText;

    [Header("Top-Left Level & Timer HUD")]
    public GameObject LevelTimerPanel;
    public Text LevelNameText;
    public Text TimerText;

    [Header("Bottom-Left Controls HUD")]
    public GameObject ControlsPanel;
    public Text ControlsText;

    [Header("Bottom-Right Audio Status HUD (Slide 4)")]
    public GameObject AudioPanel;
    public Text AudioStatusText;
    public Button AudioMuteButton;

    [Header("Pause Menu")]
    public GameObject PausePanel;
    public Button ResumeButton;
    public Button PauseRestartButton;
    public Button PauseMainMenuButton;

    [Header("Collision Screen / Restart Alert (Slide 4)")]
    public GameObject CollisionPanel;
    public Text CollisionText;

    [Header("Level Complete Modal (Slide 1 & 3)")]
    public GameObject LevelCompletePanel;
    public Text VictoryTitleText;
    public Text VictoryLevelPassedText;
    public Text VictoryStatsText;
    public Button NextLevelButton;
    public Button PlayAgainButton;
    public Button MainMenuButton;

    private float _elapsedTime = 0f;
    private bool _gameFinished = false;
    private bool _isPaused = false;
    private AudioSource _bgmAudioSource;
    private Transform _playerTransform;
    private Transform _exitTransform;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Find Player & Exit Portal for distance tracking
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
            var controller = player.GetComponent<StarterAssets.ThirdPersonController>();
            if (controller != null && controller.BackgroundMusic != null)
            {
                _bgmAudioSource = controller.BackgroundMusic;
            }
            else
            {
                _bgmAudioSource = player.GetComponent<AudioSource>();
            }
        }
        if (_bgmAudioSource == null)
        {
            _bgmAudioSource = FindAnyObjectByType<AudioSource>();
        }

        var exit = GameObject.FindGameObjectWithTag("Goal");
        if (exit != null)
        {
            _exitTransform = exit.transform;
        }

        // Setup Pause Menu buttons
        if (ResumeButton != null) ResumeButton.onClick.AddListener(ResumeGame);
        if (PauseRestartButton != null) PauseRestartButton.onClick.AddListener(RestartCurrentLevel);
        if (PauseMainMenuButton != null) PauseMainMenuButton.onClick.AddListener(ReturnToMainMenu);

        // Setup Victory Modal buttons
        if (NextLevelButton != null) NextLevelButton.onClick.AddListener(OnNextLevelClicked);
        if (PlayAgainButton != null) PlayAgainButton.onClick.AddListener(RestartCurrentLevel);
        if (MainMenuButton != null) MainMenuButton.onClick.AddListener(ReturnToMainMenu);

        // Setup Audio toggle
        if (AudioMuteButton != null) AudioMuteButton.onClick.AddListener(ToggleMusic);

        if (CollisionPanel != null) CollisionPanel.SetActive(false);
        if (LevelCompletePanel != null) LevelCompletePanel.SetActive(false);
        if (PausePanel != null) PausePanel.SetActive(false);

        UpdateAudioHUD();
    }

    private void Update()
    {
        if (!_gameFinished && !_isPaused)
        {
            _elapsedTime += Time.deltaTime;
            if (TimerText != null)
            {
                int minutes = Mathf.FloorToInt(_elapsedTime / 60F);
                int seconds = Mathf.FloorToInt(_elapsedTime % 60F);
                TimerText.text = string.Format("⏱ {0:00}:{1:00}", minutes, seconds);
            }

            // Real-time distance to Exit indicator
            if (_playerTransform != null && _exitTransform != null && ExitDistanceText != null)
            {
                float dist = Vector3.Distance(_playerTransform.position, _exitTransform.position);
                if (dist <= 12f)
                {
                    ExitDistanceText.text = "🌟 EXIT IN SIGHT (" + Mathf.RoundToInt(dist) + "m)! Step inside to pass!";
                    ExitDistanceText.color = new Color(0.2f, 1.0f, 0.4f);
                }
                else
                {
                    ExitDistanceText.text = "Exit Portal: ~" + Mathf.RoundToInt(dist) + "m away (Follow green sky beacon)";
                    ExitDistanceText.color = new Color(0.75f, 0.85f, 0.95f);
                }
            }
        }

        // Hotkey [M] for Mute
        if (Input.GetKeyDown(KeyCode.M)) ToggleMusic();

        // Hotkey [P] or [Esc] for Pause
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (!_gameFinished) TogglePause();
        }

        // Hotkey [R] for Quick Restart
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!_gameFinished) RestartCurrentLevel();
        }
    }

    public void TogglePause()
    {
        _isPaused = !_isPaused;
        Time.timeScale = _isPaused ? 0f : 1f;
        if (PausePanel != null) PausePanel.SetActive(_isPaused);
    }

    public void ResumeGame()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        if (PausePanel != null) PausePanel.SetActive(false);
    }

    public void ToggleMusic()
    {
        if (_bgmAudioSource != null)
        {
            _bgmAudioSource.mute = !_bgmAudioSource.mute;
            UpdateAudioHUD();
        }
    }

    private void UpdateAudioHUD()
    {
        if (AudioStatusText != null)
        {
            bool isMuted = _bgmAudioSource != null && _bgmAudioSource.mute;
            AudioStatusText.text = isMuted ? "♫ Music: MUTED [M]" : "♫ Music: PLAYING [M]";
            AudioStatusText.color = isMuted ? new Color(0.9f, 0.4f, 0.4f) : new Color(0.3f, 0.95f, 0.5f);
        }
    }

    public void TriggerCollisionRestart()
    {
        if (_gameFinished) return;
        _gameFinished = true;

        StartCoroutine(CollisionSequence());
    }

    private IEnumerator CollisionSequence()
    {
        if (CollisionPanel != null)
        {
            CollisionPanel.SetActive(true);
            if (CollisionText != null)
            {
                CollisionText.text = "⚠️ COLLISION DETECTED!\nHit obstacle (Tree / Rock)\nRestarting maze...";
            }
        }

        yield return new WaitForSeconds(0.65f);

        RestartCurrentLevel();
    }

    public void TriggerLevelComplete()
    {
        if (_gameFinished) return;
        _gameFinished = true;

        if (LevelCompletePanel != null)
        {
            LevelCompletePanel.SetActive(true);

            int currentLevel = GameManager.Instance != null ? GameManager.Instance.CurrentLevelNumber : 1;
            int minutes = Mathf.FloorToInt(_elapsedTime / 60F);
            int seconds = Mathf.FloorToInt(_elapsedTime % 60F);

            if (VictoryTitleText != null)
            {
                VictoryTitleText.text = "🎉 LEVEL " + currentLevel + " PASSED! 🎉";
            }

            if (VictoryLevelPassedText != null)
            {
                VictoryLevelPassedText.text = "✓ SUCCESS: You escaped the boundaries of Level " + currentLevel + "!";
            }

            if (VictoryStatsText != null)
            {
                VictoryStatsText.text = string.Format("Completion Time: {0:00}:{1:00}\nStatus: Objective Achieved!\nNext Stage Unlocked!", minutes, seconds);
            }
        }
    }

    public void OnNextLevelClicked()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadNextLevel();
        }
        else
        {
            RestartCurrentLevel();
        }
    }

    public void RestartCurrentLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainMenu();
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }
}
