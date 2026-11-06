using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Level Buttons")]
    public Button Level1Button;
    public Button Level2Button;
    public Button Level3Button;

    [Header("Instructions Modal")]
    public Button HowToPlayButton;
    public GameObject InstructionsPanel;
    public Button CloseInstructionsButton;

    [Header("Audio")]
    public Button MusicToggleButton;
    public Text MusicStatusText;
    public AudioSource MenuAudioSource;

    private void Start()
    {
        // Unrestricted cursor for menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (Level1Button != null) Level1Button.onClick.AddListener(() => StartLevel(1));
        if (Level2Button != null) Level2Button.onClick.AddListener(() => StartLevel(2));
        if (Level3Button != null) Level3Button.onClick.AddListener(() => StartLevel(3));

        if (HowToPlayButton != null) HowToPlayButton.onClick.AddListener(OpenInstructions);
        if (CloseInstructionsButton != null) CloseInstructionsButton.onClick.AddListener(CloseInstructions);

        if (MusicToggleButton != null) MusicToggleButton.onClick.AddListener(ToggleMusic);

        if (InstructionsPanel != null) InstructionsPanel.SetActive(false);

        UpdateMusicButtonText();
    }

    public void StartLevel(int level)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadLevel(level);
        }
        else
        {
            SceneManager.LoadScene("Level" + level);
        }
    }

    public void OpenInstructions()
    {
        if (InstructionsPanel != null) InstructionsPanel.SetActive(true);
    }

    public void CloseInstructions()
    {
        if (InstructionsPanel != null) InstructionsPanel.SetActive(false);
    }

    public void ToggleMusic()
    {
        if (MenuAudioSource != null)
        {
            MenuAudioSource.mute = !MenuAudioSource.mute;
            UpdateMusicButtonText();
        }
    }

    private void UpdateMusicButtonText()
    {
        if (MusicStatusText != null)
        {
            bool isMuted = MenuAudioSource != null && MenuAudioSource.mute;
            MusicStatusText.text = isMuted ? "♫ Music: MUTED" : "♫ Music: ON";
            MusicStatusText.color = isMuted ? new Color(0.9f, 0.4f, 0.4f) : new Color(0.3f, 0.95f, 0.5f);
        }
    }
}
