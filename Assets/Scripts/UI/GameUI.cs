using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

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

    // Score display (always visible, bottom-right, small)
    private TextMeshProUGUI _scoreText;
    private TextMeshProUGUI _highScoreText;

    private Canvas _canvas;

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
        _canvas = GetComponent<Canvas>();
        EnsureCanvasSetup();
        BuildHUDIfNeeded();

        if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        if (_bombHolderText != null) _bombHolderText.text = "";
        if (_buttonHintsText != null)
            _buttonHintsText.text = "MOVE: A/D  |  JUMP: Space";

        UpdateScoreDisplay();
    }

    private void EnsureCanvasSetup()
    {
        if (_canvas == null) return;

        var rt = _canvas.GetComponent<RectTransform>();
        if (rt != null && rt.localScale == Vector3.zero)
            rt.localScale = Vector3.one;

        // High sort order so UI is always in front of 3D objects
        _canvas.sortingOrder = 100;

        if (EventSystem.current == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();
    }

    private void BuildHUDIfNeeded()
    {
        if (_canvas == null) return;
        var canvasRT = _canvas.GetComponent<RectTransform>();

        if (_bombTimerText == null)
            _bombTimerText = CreateText(canvasRT, "BombTimer", "", 48f, TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -30), new Vector2(200, 60));

        if (_roundText == null)
            _roundText = CreateText(canvasRT, "RoundText", "", 20f, TextAlignmentOptions.TopLeft,
                new Vector2(0, 1f), new Vector2(0, 1f), new Vector2(15, -10), new Vector2(200, 35));

        if (_aliveCountText == null)
            _aliveCountText = CreateText(canvasRT, "AliveCount", "", 20f, TextAlignmentOptions.TopRight,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-15, -10), new Vector2(200, 35));

        if (_bombHolderText == null)
            _bombHolderText = CreateText(canvasRT, "BombHolder", "", 22f, TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -90), new Vector2(500, 40));

        if (_sessionTimerText == null)
            _sessionTimerText = CreateText(canvasRT, "SessionTimer", "", 18f, TextAlignmentOptions.TopRight,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-15, -45), new Vector2(150, 30));

        if (_buttonHintsText == null)
            _buttonHintsText = CreateText(canvasRT, "ButtonHints", "", 16f, TextAlignmentOptions.Bottom,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 20), new Vector2(400, 30));

        // Score display - small, bottom-right corner
        _scoreText = CreateText(canvasRT, "ScoreDisplay", "", 14f, TextAlignmentOptions.BottomRight,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-15, 50), new Vector2(200, 25));

        _highScoreText = CreateText(canvasRT, "HighScoreDisplay", "", 14f, TextAlignmentOptions.BottomRight,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-15, 30), new Vector2(200, 25));
        _highScoreText.color = new Color(1f, 0.85f, 0.4f); // gold

        // Build Game Over panel
        if (_gameOverPanel == null)
            BuildGameOverPanel(canvasRT);
    }

    private void BuildGameOverPanel(RectTransform parent)
    {
        // Dark overlay - renders in front of everything
        var panelObj = new GameObject("GameOverPanel");
        panelObj.transform.SetParent(parent, false);
        var panelRT = panelObj.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        var panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.7f);

        _gameOverPanel = panelObj;

        // "GAME OVER" title - small
        _winnerText = CreateText(panelRT, "WinnerText", "", 28f, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 60), new Vector2(400, 50));

        // Play Again button - centered, clean
        var btnObj = new GameObject("PlayAgainBtn");
        btnObj.transform.SetParent(panelRT, false);
        var btnRT = btnObj.AddComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.5f, 0.5f);
        btnRT.anchorMax = new Vector2(0.5f, 0.5f);
        btnRT.anchoredPosition = new Vector2(0, -20);
        btnRT.sizeDelta = new Vector2(220, 55);

        var btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.65f, 0.2f, 1f);

        var btn = btnObj.AddComponent<Button>();
        var btnColors = btn.colors;
        btnColors.normalColor = new Color(0.2f, 0.65f, 0.2f, 1f);
        btnColors.highlightedColor = new Color(0.3f, 0.85f, 0.3f, 1f);
        btnColors.pressedColor = new Color(0.1f, 0.5f, 0.1f, 1f);
        btn.colors = btnColors;
        btn.onClick.AddListener(OnRestartButton);
        _restartButton = btn;

        _playAgainText = CreateText(btnRT, "PlayAgainText", "PLAY AGAIN", 22f, TextAlignmentOptions.Center,
            new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
        _playAgainText.color = Color.white;
        _playAgainText.raycastTarget = false;

        // No share button, no big scores on game over panel
        // Scores stay small in bottom-right corner (always visible from HUD)

        _gameOverPanel.SetActive(false);
    }

    private TextMeshProUGUI CreateText(RectTransform parent, string name, string text, float fontSize,
        TextAlignmentOptions alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        return tmp;
    }

    private void UpdateScoreDisplay()
    {
        if (_scoreText != null && ScoreManager.Instance != null)
            _scoreText.text = $"Score: {ScoreManager.Instance.RoundsSurvived}";

        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (_highScoreText != null)
            _highScoreText.text = $"Best: {highScore}";
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
            _bombTimerText.fontSize = Mathf.Lerp(48f, 60f, flash);
        }
        else
        {
            _bombTimerText.color = Color.white;
            _bombTimerText.fontSize = 48f;
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

        UpdateScoreDisplay();
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

        // Save high score
        if (ScoreManager.Instance != null)
        {
            int current = ScoreManager.Instance.RoundsSurvived;
            int best = PlayerPrefs.GetInt("HighScore", 0);
            if (current > best)
            {
                PlayerPrefs.SetInt("HighScore", current);
                PlayerPrefs.Save();
            }
        }

        UpdateScoreDisplay();

        _gameOverPanel.SetActive(true);

        if (_winnerText != null)
        {
            string winner = RoundManager.Instance != null ? RoundManager.Instance.WinnerName : "Unknown";
            if (winner == "You")
                _winnerText.text = "YOU WIN!";
            else
                _winnerText.text = $"GAME OVER";
        }

        if (_restartButton != null)
            EventSystem.current?.SetSelectedGameObject(_restartButton.gameObject);

        StartCoroutine(HeadShakeRoutine());
    }

    private IEnumerator HeadShakeRoutine()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            var allPlayers = Resources.FindObjectsOfTypeAll<PlayerController>();
            foreach (var p in allPlayers)
            {
                playerObj = p.gameObject;
                break;
            }
        }

        if (playerObj == null) yield break;

        playerObj.SetActive(true);
        var rb = playerObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
        }

        Transform head = _playerHead != null ? _playerHead : playerObj.transform;

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
