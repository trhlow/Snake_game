using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject gameUIPanel;
    public GameObject gameOverPanel;
    public GameObject pauseMenuPanel;
    public GameObject highscorePanel;

    [Header("Menu Buttons")]
    public Button singlePlayerButton;
    public Button multiplayerButton;
    public Button aiModeButton;
    public Button settingsButton;
    public Button highscoreButton;
    public Button quitButton;

    [Header("Settings UI")]
    public Text colorText;
    public Button colorPrevButton;
    public Button colorNextButton;
    public Button languageButton;
    public Button backToMenuButton;
    public Image colorPreview;

    [Header("Difficulty UI")]
    public Button easyButton;
    public Button normalButton;
    public Button hardButton;
    public Text difficultyText;

    [Header("Audio UI")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Button muteButton;
    public Text muteButtonText;

    [Header("Game UI")]
    public Text scoreText;
    public Text comboText;
    public Text player1ScoreText;
    public Text player2ScoreText;

    [Header("Pause Menu UI")]
    public Button resumeButton;
    public Button pauseSettingsButton;
    public Button pauseMenuButton;

    [Header("Game Over UI")]
    public Text gameOverTitle;
    public Text finalScoreText;
    public Text highscoreText;
    public Button restartButton;
    public Button menuButton;

    [Header("Highscore UI")]
    public Transform highscoreListParent;
    public GameObject highscoreEntryPrefab;
    public Button closeHighscoreButton;

    private MenuManager menuManager;

    void Awake()
    {
        Instance = this;
        menuManager = FindFirstObjectByType<MenuManager>();
    }

    void Start()
    {
        ShowMainMenu();
        SetupButtons();
        SetupAudioControls();
    }

    void SetupButtons()
    {
        if (singlePlayerButton != null)
            singlePlayerButton.onClick.AddListener(() => menuManager.StartGameFromUI(1));

        if (multiplayerButton != null)
            multiplayerButton.onClick.AddListener(() => menuManager.StartGameFromUI(2));

        if (aiModeButton != null)
            aiModeButton.onClick.AddListener(() => menuManager.StartGameFromUI(3));

        if (settingsButton != null)
            settingsButton.onClick.AddListener(() => ShowSettings());

        if (highscoreButton != null)
            highscoreButton.onClick.AddListener(() => ShowHighscorePanel());

        if (quitButton != null)
            quitButton.onClick.AddListener(() => Application.Quit());

        if (colorPrevButton != null)
            colorPrevButton.onClick.AddListener(() => menuManager.ChangeColor(-1));

        if (colorNextButton != null)
            colorNextButton.onClick.AddListener(() => menuManager.ChangeColor(1));

        if (languageButton != null)
            languageButton.onClick.AddListener(() => menuManager.ToggleLanguage());

        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(() => ShowMainMenu());

        if (easyButton != null)
            easyButton.onClick.AddListener(() => SetDifficulty(GameManager.Difficulty.Easy));

        if (normalButton != null)
            normalButton.onClick.AddListener(() => SetDifficulty(GameManager.Difficulty.Normal));

        if (hardButton != null)
            hardButton.onClick.AddListener(() => SetDifficulty(GameManager.Difficulty.Hard));

        if (resumeButton != null)
            resumeButton.onClick.AddListener(() => GameManager.Instance.ResumeGame());

        if (pauseSettingsButton != null)
            pauseSettingsButton.onClick.AddListener(() => ShowSettings());

        if (pauseMenuButton != null)
            pauseMenuButton.onClick.AddListener(() => {
                GameManager.Instance.ResumeGame();
                menuManager.ReturnToMenu();
            });

        if (restartButton != null)
            restartButton.onClick.AddListener(() => menuManager.RestartLastMode());

        if (menuButton != null)
            menuButton.onClick.AddListener(() => ShowMainMenu());

        if (closeHighscoreButton != null)
            closeHighscoreButton.onClick.AddListener(() => ShowMainMenu());

        UpdateDifficultyUI();
    }

    void SetupAudioControls()
    {
        if (AudioManager.Instance == null) return;

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = AudioManager.Instance.musicVolume;
            musicVolumeSlider.onValueChanged.AddListener((value) => {
                AudioManager.Instance.SetMusicVolume(value);
            });
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = AudioManager.Instance.sfxVolume;
            sfxVolumeSlider.onValueChanged.AddListener((value) => {
                AudioManager.Instance.SetSFXVolume(value);
            });
        }

        if (muteButton != null)
        {
            muteButton.onClick.AddListener(() => {
                AudioManager.Instance.ToggleMute();
                UpdateMuteButtonText();
            });
            UpdateMuteButtonText();
        }
    }

    void UpdateMuteButtonText()
    {
        if (muteButtonText != null && AudioManager.Instance != null)
        {
            muteButtonText.text = AudioManager.Instance.sfxVolume == 0 ? "UNMUTE" : "MUTE";
        }
    }

    void SetDifficulty(GameManager.Difficulty diff)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetDifficulty(diff);
            UpdateDifficultyUI();
        }
    }

    void UpdateDifficultyUI()
    {
        if (GameManager.Instance == null) return;

        GameManager.Difficulty current = GameManager.Instance.currentDifficulty;

        if (difficultyText != null)
        {
            difficultyText.text = "Difficulty: " + current.ToString();
        }

        Color selectedColor = new Color(0.3f, 0.8f, 0.3f);
        Color normalColor = new Color(0.5f, 0.5f, 0.5f);

        if (easyButton != null)
        {
            var img = easyButton.GetComponent<Image>();
            if (img != null) img.color = current == GameManager.Difficulty.Easy ? selectedColor : normalColor;
        }

        if (normalButton != null)
        {
            var img = normalButton.GetComponent<Image>();
            if (img != null) img.color = current == GameManager.Difficulty.Normal ? selectedColor : normalColor;
        }

        if (hardButton != null)
        {
            var img = hardButton.GetComponent<Image>();
            if (img != null) img.color = current == GameManager.Difficulty.Hard ? selectedColor : normalColor;
        }
    }

    public void ShowMainMenu()
    {
        SetActivePanel(mainMenuPanel);
        Time.timeScale = 1f;
    }

    public void ShowSettings()
    {
        SetActivePanel(settingsPanel);
        UpdateSettingsUI();
    }

    public void ShowGameUI()
    {
        SetActivePanel(gameUIPanel);

        if (comboText != null)
            comboText.gameObject.SetActive(false);
    }

    public void ShowPauseMenu()
    {
        SetActivePanel(pauseMenuPanel);
    }

    public void ShowGameOver(string winner, int score)
    {
        SetActivePanel(gameOverPanel);

        if (gameOverTitle != null)
            gameOverTitle.text = winner;

        if (finalScoreText != null)
            finalScoreText.text = "Score: " + score.ToString();

        if (highscoreText != null)
        {
            if (HighscoreManager.Instance != null)
            {
                bool isNewHighscore = HighscoreManager.Instance.IsNewHighscore();
                int highestScore = HighscoreManager.Instance.GetHighestScore();

                if (isNewHighscore)
                {
                    int rank = HighscoreManager.Instance.GetScoreRank(score);
                    highscoreText.text = "NEW HIGHSCORE! Rank #" + rank.ToString();
                    highscoreText.color = Color.yellow;
                }
                else
                {
                    highscoreText.text = "Best: " + highestScore.ToString();
                    highscoreText.color = Color.white;
                }
            }
        }
    }

    public void ShowHighscorePanel()
    {
        SetActivePanel(highscorePanel);
        UpdateHighscoreList();
    }

    void UpdateHighscoreList()
    {
        if (highscoreListParent == null || HighscoreManager.Instance == null) return;

        foreach (Transform child in highscoreListParent)
        {
            Destroy(child.gameObject);
        }

        var highscores = HighscoreManager.Instance.GetHighscores();

        for (int i = 0; i < highscores.Count; i++)
        {
            CreateHighscoreEntry(i + 1, highscores[i]);
        }
    }

    void CreateHighscoreEntry(int rank, HighscoreEntry entry)
    {
        GameObject entryObj;

        if (highscoreEntryPrefab != null)
        {
            entryObj = Instantiate(highscoreEntryPrefab, highscoreListParent);
        }
        else
        {
            entryObj = new GameObject("Entry_" + rank.ToString());
            entryObj.transform.SetParent(highscoreListParent, false);

            Text text = entryObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 28;
            text.alignment = TextAnchor.MiddleLeft;
            text.color = rank <= 3 ? Color.yellow : Color.white;

            RectTransform rt = entryObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(800, 40);
        }

        Text entryText = entryObj.GetComponent<Text>();
        if (entryText != null)
        {
            string medal = rank == 1 ? "1." : rank == 2 ? "2." : rank == 3 ? "3." : rank.ToString() + ".";
            entryText.text = medal + " " + entry.playerName + " " + entry.score.ToString() + " pts [" + entry.difficulty + "] " + entry.date;
        }
    }

    void SetActivePanel(GameObject panel)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (gameUIPanel != null) gameUIPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (highscorePanel != null) highscorePanel.SetActive(false);

        if (panel != null) panel.SetActive(true);
    }

    public void UpdateScoreUI(string text)
    {
        if (scoreText != null)
            scoreText.text = text;
    }

    public void UpdateMultiplayerScores(int score1, int score2)
    {
        if (player1ScoreText != null)
        {
            player1ScoreText.gameObject.SetActive(true);
            player1ScoreText.text = "P1: " + score1.ToString();
        }

        if (player2ScoreText != null)
        {
            player2ScoreText.gameObject.SetActive(true);
            player2ScoreText.text = "P2: " + score2.ToString();
        }

        if (scoreText != null)
            scoreText.gameObject.SetActive(false);
    }

    public void UpdateComboUI(int combo)
    {
        if (comboText != null)
        {
            if (combo > 1)
            {
                comboText.gameObject.SetActive(true);
                comboText.text = "COMBO x" + combo.ToString() + "!";
                comboText.transform.localScale = Vector3.one * (1f + combo * 0.1f);
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }
    }

    public void UpdateSettingsUI()
    {
        if (menuManager != null)
        {
            if (colorText != null)
                colorText.text = menuManager.GetCurrentColorName();

            if (colorPreview != null)
                colorPreview.color = menuManager.GetCurrentColor();

            if (languageButton != null)
            {
                Text btnText = languageButton.GetComponentInChildren<Text>();
                if (btnText != null)
                    btnText.text = "Language: " + menuManager.GetLanguageName();
            }
        }

        UpdateDifficultyUI();
    }
}