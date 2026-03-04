using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _waveText;
    [SerializeField] private TextMeshProUGUI _comboText;
    [SerializeField] private Slider _healthBar;

    [Header("Panels")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private TextMeshProUGUI _finalScoreText;
    [SerializeField] private TextMeshProUGUI _highScoreText;
    [SerializeField] private TextMeshProUGUI _finalWaveText;

    [Header("Combo Popup")]
    [SerializeField] private float _comboFadeSpeed = 2f;
    private float _comboDisplayTimer;

    void OnEnable()
    {
        ScoreManager.OnScoreChanged += UpdateScore;
        ScoreManager.OnComboChanged += UpdateCombo;
        PlayerController.OnHealthChanged += UpdateHealth;
        WaveManager.OnWaveStart += UpdateWave;
        GameManager.OnGameStateChanged += HandleGameState;
    }

    void OnDisable()
    {
        ScoreManager.OnScoreChanged -= UpdateScore;
        ScoreManager.OnComboChanged -= UpdateCombo;
        PlayerController.OnHealthChanged -= UpdateHealth;
        WaveManager.OnWaveStart -= UpdateWave;
        GameManager.OnGameStateChanged -= HandleGameState;
    }

    void Start()
    {
        if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        if (_pausePanel != null) _pausePanel.SetActive(false);
        if (_comboText != null) _comboText.gameObject.SetActive(false);
    }

    void Update()
    {
        // Fade out combo text
        if (_comboDisplayTimer > 0)
        {
            _comboDisplayTimer -= Time.deltaTime;
            if (_comboDisplayTimer <= 0 && _comboText != null)
            {
                _comboText.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateScore(int score, int combo)
    {
        if (_scoreText != null)
            _scoreText.text = score.ToString("N0");
    }

    private void UpdateCombo(int killCount, int multiplier)
    {
        if (_comboText == null) return;

        if (multiplier > 1)
        {
            _comboText.gameObject.SetActive(true);
            _comboText.text = $"x{multiplier} COMBO!";
            _comboDisplayTimer = _comboFadeSpeed;

            // Scale based on combo
            float scale = 1f + (multiplier - 1) * 0.2f;
            _comboText.transform.localScale = Vector3.one * scale;
        }
        else
        {
            _comboText.gameObject.SetActive(false);
        }
    }

    private void UpdateHealth(int current, int max)
    {
        if (_healthBar != null)
        {
            _healthBar.maxValue = max;
            _healthBar.value = current;
        }
    }

    private void UpdateWave(int wave)
    {
        if (_waveText != null)
        {
            _waveText.text = $"WAVE {wave}";
            // Could add animation here
        }
    }

    private void HandleGameState(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.GameOver:
                ShowGameOver();
                break;
            case GameManager.GameState.Paused:
                if (_pausePanel != null) _pausePanel.SetActive(true);
                break;
            case GameManager.GameState.Playing:
                if (_pausePanel != null) _pausePanel.SetActive(false);
                break;
        }
    }

    private void ShowGameOver()
    {
        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(true);

            if (_finalScoreText != null && ScoreManager.Instance != null)
                _finalScoreText.text = $"SCORE: {ScoreManager.Instance.Score:N0}";

            if (_highScoreText != null && ScoreManager.Instance != null)
                _highScoreText.text = $"BEST: {ScoreManager.Instance.HighScore:N0}";

            if (_finalWaveText != null && WaveManager.Instance != null)
                _finalWaveText.text = $"WAVE {WaveManager.Instance.CurrentWave}";
        }
    }

    // Called by UI buttons
    public void OnRestartButton()
    {
        GameManager.Instance.RestartGame();
    }

    public void OnResumeButton()
    {
        GameManager.Instance.ResumeGame();
    }

    public void OnQuitButton()
    {
        GameManager.Instance.QuitGame();
    }
}
