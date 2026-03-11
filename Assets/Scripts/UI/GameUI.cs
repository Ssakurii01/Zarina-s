using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI _bombTimerText;
    [SerializeField] private TextMeshProUGUI _aliveCountText;
    [SerializeField] private TextMeshProUGUI _roundText;
    [SerializeField] private TextMeshProUGUI _bombHolderText;

    [Header("Session Timer")]
    [SerializeField] private TextMeshProUGUI _sessionTimerText;

    [Header("Button Hints")]
    [SerializeField] private TextMeshProUGUI _buttonHintsText;

    [Header("Game Over Panel")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private TextMeshProUGUI _winnerText;
    [SerializeField] private TextMeshProUGUI _roundsSurvivedText;
    [SerializeField] private Button _restartButton;

    void OnEnable()
    {
        BombController.OnBombTimerChanged += UpdateBombTimer;
        BombController.OnBombTransferred += UpdateBombHolder;
        RoundManager.OnRoundStart += UpdateRound;
        RoundManager.OnAliveCountChanged += UpdateAliveCount;
        GameManager.OnGameStateChanged += HandleGameState;
        GameManager.OnTimerChanged += UpdateSessionTimer;
    }

    void OnDisable()
    {
        BombController.OnBombTimerChanged -= UpdateBombTimer;
        BombController.OnBombTransferred -= UpdateBombHolder;
        RoundManager.OnRoundStart -= UpdateRound;
        RoundManager.OnAliveCountChanged -= UpdateAliveCount;
        GameManager.OnGameStateChanged -= HandleGameState;
        GameManager.OnTimerChanged -= UpdateSessionTimer;
    }

    void Start()
    {
        if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        if (_bombHolderText != null) _bombHolderText.text = "";

        if (_buttonHintsText != null)
            _buttonHintsText.text = "MOVE: A/D  |  JUMP: Space";
    }

    private void UpdateBombTimer(float time)
    {
        if (_bombTimerText == null) return;

        int seconds = Mathf.CeilToInt(time);
        _bombTimerText.text = seconds.ToString();

        if (time <= 5f)
        {
            float flash = (Mathf.Sin(Time.time * 10f) + 1f) * 0.5f;
            _bombTimerText.color = Color.Lerp(Color.red, Color.white, flash);
            _bombTimerText.fontSize = Mathf.Lerp(72f, 90f, flash);
        }
        else
        {
            _bombTimerText.color = Color.white;
            _bombTimerText.fontSize = 72f;
        }
    }

    private void UpdateBombHolder(GameObject holder)
    {
        if (_bombHolderText == null || holder == null) return;

        var player = holder.GetComponent<PlayerController>();
        if (player != null)
        {
            _bombHolderText.text = "YOU HAVE THE BOMB!";
            _bombHolderText.color = Color.red;
        }
        else
        {
            var bot = holder.GetComponent<BotController>();
            string name = bot != null ? bot.BotName : holder.name;
            _bombHolderText.text = $"{name} has the bomb";
            _bombHolderText.color = Color.yellow;
        }
    }

    private void UpdateRound(int round)
    {
        if (_roundText != null)
            _roundText.text = $"ROUND {round}";

        if (_bombHolderText != null)
            _bombHolderText.text = "";
    }

    private void UpdateAliveCount(int count)
    {
        if (_aliveCountText != null)
            _aliveCountText.text = $"{count} ALIVE";
    }

    private void UpdateSessionTimer(float timeRemaining)
    {
        if (_sessionTimerText == null) return;

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        _sessionTimerText.text = $"{minutes}:{seconds:00}";
        _sessionTimerText.color = timeRemaining < 30f ? Color.red : Color.white;
    }

    private void HandleGameState(GameManager.GameState state)
    {
        if (state == GameManager.GameState.GameOver)
            ShowGameOver();
    }

    private void ShowGameOver()
    {
        if (_gameOverPanel == null) return;

        _gameOverPanel.SetActive(true);

        if (_winnerText != null)
        {
            string winner = RoundManager.Instance != null ? RoundManager.Instance.WinnerName : "Unknown";
            if (winner == "You")
                _winnerText.text = "YOU WIN!";
            else
                _winnerText.text = $"{winner} WINS!";
        }

        if (_roundsSurvivedText != null && ScoreManager.Instance != null)
            _roundsSurvivedText.text = $"Rounds Survived: {ScoreManager.Instance.RoundsSurvived}";

        if (_restartButton != null)
            EventSystem.current.SetSelectedGameObject(_restartButton.gameObject);
    }

    public void OnRestartButton()
    {
        GameManager.Instance?.RestartGame();
    }

    public void OnQuitButton()
    {
        GameManager.Instance?.QuitGame();
    }
}
