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
    [SerializeField] private TextMeshProUGUI _playAgainText;

    [Header("Player Model (for head shake)")]
    [SerializeField] private Transform _playerHead;

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

        // Ensure EventSystem exists for button clicks
        if (EventSystem.current == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        if (_winnerText != null)
        {
            string winner = RoundManager.Instance != null ? RoundManager.Instance.WinnerName : "Unknown";
            if (winner == "You")
                _winnerText.text = "YOU WIN!";
            else
                _winnerText.text = $"{winner} WINS!";
            _winnerText.fontSize = 36f;
        }

        if (_roundsSurvivedText != null && ScoreManager.Instance != null)
        {
            _roundsSurvivedText.text = $"Rounds Survived: {ScoreManager.Instance.RoundsSurvived}";
            _roundsSurvivedText.fontSize = 24f;
        }

        // Make Play Again text a proper clickable button
        if (_playAgainText != null)
        {
            _playAgainText.text = "PLAY AGAIN";
            _playAgainText.fontSize = 28f;
            _playAgainText.raycastTarget = true;

            // Add Button component if missing
            var button = _playAgainText.GetComponent<Button>();
            if (button == null)
                button = _playAgainText.gameObject.AddComponent<Button>();

            // Set up color transition so it looks clickable
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.yellow;
            colors.pressedColor = new Color(1f, 0.5f, 0f);
            button.colors = colors;
            button.targetGraphic = _playAgainText;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnRestartButton);

            // Also select it so keyboard/gamepad works
            EventSystem.current?.SetSelectedGameObject(_playAgainText.gameObject);
        }
        else if (_restartButton != null)
        {
            EventSystem.current?.SetSelectedGameObject(_restartButton.gameObject);
        }

        // Start head shake on player model
        StartCoroutine(HeadShakeRoutine());
    }

    private System.Collections.IEnumerator HeadShakeRoutine()
    {
        // Find the player object (may be inactive after elimination)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            // Search inactive objects
            var allPlayers = Resources.FindObjectsOfTypeAll<PlayerController>();
            foreach (var p in allPlayers)
            {
                playerObj = p.gameObject;
                break;
            }
        }

        if (playerObj == null) yield break;

        // Re-enable the player so the shake is visible, but keep it "dead"
        playerObj.SetActive(true);
        var rb = playerObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
        }

        Transform head = _playerHead != null ? _playerHead : playerObj.transform;

        // Shake head side-to-side (like saying "no")
        Quaternion originalRot = head.localRotation;
        float elapsed = 0f;
        float shakeDuration = 2f;
        float shakeSpeed = 12f;
        float shakeAngle = 25f;

        while (elapsed < shakeDuration)
        {
            float angle = Mathf.Sin(elapsed * shakeSpeed) * shakeAngle;
            head.localRotation = originalRot * Quaternion.Euler(0f, angle, 0f);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        head.localRotation = originalRot;
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
