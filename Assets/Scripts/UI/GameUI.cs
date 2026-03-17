using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class GameUI : MonoBehaviour
{
    [Header("Player Model (for head shake)")]
    [SerializeField] private Transform _playerHead;

    // All UI is built from code - no scene references needed
    private Canvas _canvas;
    private RectTransform _canvasRT;

    // HUD
    private TextMeshProUGUI _bombTimerText;
    private TextMeshProUGUI _aliveCountText;
    private TextMeshProUGUI _roundText;
    private TextMeshProUGUI _bombHolderText;
    private TextMeshProUGUI _sessionTimerText;
    private TextMeshProUGUI _buttonHintsText;
    private TextMeshProUGUI _scoreText;
    private TextMeshProUGUI _highScoreText;

    // Start screen
    private GameObject _startPanel;
    private TextMeshProUGUI _startPressText;

    // Game over
    private GameObject _gameOverPanel;
    private TextMeshProUGUI _winnerText;
    private Button _restartButton;
    private TextMeshProUGUI _playAgainText;

    // Round announcement
    private TextMeshProUGUI _roundAnnounceText;

    // Score popup
    private TextMeshProUGUI _scorePopupText;

    // Proximity warning (red vignette)
    private Image _vignetteImage;

    // Bomb arrow indicator
    private RectTransform _bombArrowRT;
    private Image _bombArrowImage;

    // Screen flash
    private Image _screenFlashImage;

    private GameObject _playerObj;

    void OnEnable()
    {
        BombController.OnBombTimerChanged += UpdateBombTimer;
        BombController.OnBombTransferred += UpdateBombHolder;
        RoundManager.OnRoundStart += UpdateRound;
        RoundManager.OnAliveCountChanged += UpdateAliveCount;
        GameManager.OnGameStateChanged += HandleGameState;
        GameManager.OnTimerChanged += UpdateSessionTimer;
        GameManager.OnGameStarted += HideStartScreen;
    }

    void OnDisable()
    {
        BombController.OnBombTimerChanged -= UpdateBombTimer;
        BombController.OnBombTransferred -= UpdateBombHolder;
        RoundManager.OnRoundStart -= UpdateRound;
        RoundManager.OnAliveCountChanged -= UpdateAliveCount;
        GameManager.OnGameStateChanged -= HandleGameState;
        GameManager.OnTimerChanged -= UpdateSessionTimer;
        GameManager.OnGameStarted -= HideStartScreen;
    }

    void Start()
    {
        SetupCanvas();
        DestroyExistingChildren();
        BuildAllUI();

        if (_bombHolderText != null) _bombHolderText.text = "";
        if (_gameOverPanel != null) _gameOverPanel.SetActive(false);

        _playerObj = GameObject.FindGameObjectWithTag("Player");

        UpdateScoreDisplay();
    }

    void Update()
    {
        UpdateProximityWarning();
        UpdateBombArrow();

        // Pulse the "press space" text
        if (_startPressText != null && _startPanel != null && _startPanel.activeSelf)
        {
            float alpha = (Mathf.Sin(Time.unscaledTime * 3f) + 1f) * 0.5f;
            _startPressText.alpha = Mathf.Lerp(0.3f, 1f, alpha);
        }
    }

    private void SetupCanvas()
    {
        _canvas = GetComponent<Canvas>();
        if (_canvas == null)
            _canvas = gameObject.AddComponent<Canvas>();

        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 100;

        var rt = GetComponent<RectTransform>();
        if (rt != null)
            rt.localScale = Vector3.one;

        var scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
            scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();

        if (EventSystem.current == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        _canvasRT = _canvas.GetComponent<RectTransform>();
    }

    private void DestroyExistingChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }

    private void BuildAllUI()
    {
        // --- HUD ---
        _bombTimerText = CreateText("BombTimer", "", 48f, TextAlignmentOptions.Center,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -30), new Vector2(200, 60));

        _roundText = CreateText("RoundText", "", 20f, TextAlignmentOptions.TopLeft,
            new Vector2(0, 1f), new Vector2(0, 1f), new Vector2(15, -10), new Vector2(200, 35));

        _aliveCountText = CreateText("AliveCount", "", 20f, TextAlignmentOptions.TopRight,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-15, -10), new Vector2(200, 35));

        _bombHolderText = CreateText("BombHolder", "", 22f, TextAlignmentOptions.Center,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -90), new Vector2(500, 40));

        _sessionTimerText = CreateText("SessionTimer", "", 18f, TextAlignmentOptions.TopRight,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-15, -45), new Vector2(150, 30));

        _buttonHintsText = CreateText("ButtonHints", "MOVE: A/D  |  JUMP: Space", 16f, TextAlignmentOptions.Bottom,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 20), new Vector2(400, 30));

        // Score display - small, bottom-right
        _scoreText = CreateText("ScoreDisplay", "", 14f, TextAlignmentOptions.BottomRight,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-15, 50), new Vector2(200, 25));

        _highScoreText = CreateText("HighScoreDisplay", "", 14f, TextAlignmentOptions.BottomRight,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-15, 30), new Vector2(200, 25));
        _highScoreText.color = new Color(1f, 0.85f, 0.4f);

        // --- Round announcement (hidden by default) ---
        _roundAnnounceText = CreateText("RoundAnnounce", "", 60f, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500, 100));
        _roundAnnounceText.gameObject.SetActive(false);

        // --- Score popup (hidden by default) ---
        _scorePopupText = CreateText("ScorePopup", "", 24f, TextAlignmentOptions.Center,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-80, 80), new Vector2(100, 40));
        _scorePopupText.color = Color.green;
        _scorePopupText.gameObject.SetActive(false);

        // --- Proximity vignette ---
        BuildVignette();

        // --- Bomb arrow indicator ---
        BuildBombArrow();

        // --- Screen flash overlay ---
        BuildScreenFlash();

        // --- Game Over Panel ---
        BuildGameOverPanel();

        // --- Start Screen (on top of everything) ---
        BuildStartScreen();
    }

    // ======================== START SCREEN ========================

    private void BuildStartScreen()
    {
        var panelObj = new GameObject("StartPanel");
        panelObj.transform.SetParent(_canvasRT, false);
        var panelRT = panelObj.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        var bg = panelObj.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.02f, 0.1f, 0.9f);

        _startPanel = panelObj;

        // Title
        var title = CreateText("TitleText", "FLUXFURY", 72f, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 100), new Vector2(600, 120),
            panelRT);
        title.color = new Color(1f, 0.4f, 0.1f);

        // Subtitle
        CreateText("SubtitleText", "BOMB TAG ARENA", 24f, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(400, 40),
            panelRT).color = new Color(1f, 0.7f, 0.3f);

        // Press Space
        _startPressText = CreateText("PressSpace", "PRESS SPACE TO START", 28f, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -60), new Vector2(500, 50),
            panelRT);

        // Controls
        CreateText("Controls", "A/D = Move   |   SPACE = Jump   |   Pass the bomb!", 18f, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -140), new Vector2(600, 40),
            panelRT).color = new Color(0.7f, 0.7f, 0.7f);
    }

    private void HideStartScreen()
    {
        if (_startPanel != null)
            _startPanel.SetActive(false);
    }

    // ======================== VIGNETTE ========================

    private void BuildVignette()
    {
        var vigObj = new GameObject("Vignette");
        vigObj.transform.SetParent(_canvasRT, false);
        var vigRT = vigObj.AddComponent<RectTransform>();
        vigRT.anchorMin = Vector2.zero;
        vigRT.anchorMax = Vector2.one;
        vigRT.offsetMin = Vector2.zero;
        vigRT.offsetMax = Vector2.zero;

        _vignetteImage = vigObj.AddComponent<Image>();
        _vignetteImage.color = new Color(1f, 0f, 0f, 0f);
        _vignetteImage.raycastTarget = false;
    }

    private void UpdateProximityWarning()
    {
        if (_vignetteImage == null) return;

        if (_playerObj == null || !_playerObj.activeInHierarchy ||
            BombController.Instance == null || !BombController.Instance.IsActive)
        {
            _vignetteImage.color = new Color(1f, 0f, 0f, 0f);
            return;
        }

        // Don't show warning if player holds the bomb
        if (BombController.Instance.CurrentHolder == _playerObj)
        {
            _vignetteImage.color = new Color(1f, 0f, 0f, 0f);
            return;
        }

        GameObject holder = BombController.Instance.CurrentHolder;
        if (holder == null)
        {
            _vignetteImage.color = new Color(1f, 0f, 0f, 0f);
            return;
        }

        float dist = Vector3.Distance(_playerObj.transform.position, holder.transform.position);
        float warningDist = 4f;
        float alpha = dist < warningDist ? Mathf.Lerp(0.25f, 0f, dist / warningDist) : 0f;

        _vignetteImage.color = new Color(1f, 0f, 0f, alpha);
    }

    // ======================== BOMB ARROW ========================

    private void BuildBombArrow()
    {
        var arrowObj = new GameObject("BombArrow");
        arrowObj.transform.SetParent(_canvasRT, false);
        _bombArrowRT = arrowObj.AddComponent<RectTransform>();
        _bombArrowRT.sizeDelta = new Vector2(40, 40);

        _bombArrowImage = arrowObj.AddComponent<Image>();
        _bombArrowImage.color = new Color(1f, 0.3f, 0f, 0.8f);
        _bombArrowImage.raycastTarget = false;
        arrowObj.SetActive(false);
    }

    private void UpdateBombArrow()
    {
        if (_bombArrowRT == null || _bombArrowImage == null) return;

        if (_playerObj == null || !_playerObj.activeInHierarchy ||
            BombController.Instance == null || !BombController.Instance.IsActive)
        {
            _bombArrowRT.gameObject.SetActive(false);
            return;
        }

        // Hide if player holds bomb
        if (BombController.Instance.CurrentHolder == _playerObj)
        {
            _bombArrowRT.gameObject.SetActive(false);
            return;
        }

        GameObject holder = BombController.Instance.CurrentHolder;
        if (holder == null)
        {
            _bombArrowRT.gameObject.SetActive(false);
            return;
        }

        // Check if bomb holder is on screen
        Camera cam = Camera.main;
        if (cam == null)
        {
            _bombArrowRT.gameObject.SetActive(false);
            return;
        }

        Vector3 screenPos = cam.WorldToScreenPoint(holder.transform.position);
        bool onScreen = screenPos.z > 0 && screenPos.x > 0 && screenPos.x < Screen.width &&
                        screenPos.y > 0 && screenPos.y < Screen.height;

        if (onScreen)
        {
            _bombArrowRT.gameObject.SetActive(false);
            return;
        }

        // Show arrow at screen edge pointing toward holder
        _bombArrowRT.gameObject.SetActive(true);

        Vector3 dir = (holder.transform.position - _playerObj.transform.position).normalized;
        Vector2 screenDir = new Vector2(dir.x, dir.y).normalized;

        float margin = 50f;
        float halfW = Screen.width * 0.5f - margin;
        float halfH = Screen.height * 0.5f - margin;

        Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        float scale = Mathf.Min(
            Mathf.Abs(screenDir.x) > 0.001f ? halfW / Mathf.Abs(screenDir.x) : float.MaxValue,
            Mathf.Abs(screenDir.y) > 0.001f ? halfH / Mathf.Abs(screenDir.y) : float.MaxValue
        );

        Vector2 edgePos = center + screenDir * scale;
        _bombArrowRT.position = edgePos;

        float angle = Mathf.Atan2(screenDir.y, screenDir.x) * Mathf.Rad2Deg;
        _bombArrowRT.rotation = Quaternion.Euler(0, 0, angle - 90f);

        // Pulse
        float pulse = (Mathf.Sin(Time.time * 6f) + 1f) * 0.5f;
        _bombArrowImage.color = Color.Lerp(new Color(1f, 0.3f, 0f, 0.5f), new Color(1f, 0f, 0f, 1f), pulse);
    }

    // ======================== SCREEN FLASH ========================

    private void BuildScreenFlash()
    {
        var flashObj = new GameObject("ScreenFlash");
        flashObj.transform.SetParent(_canvasRT, false);
        var flashRT = flashObj.AddComponent<RectTransform>();
        flashRT.anchorMin = Vector2.zero;
        flashRT.anchorMax = Vector2.one;
        flashRT.offsetMin = Vector2.zero;
        flashRT.offsetMax = Vector2.zero;

        _screenFlashImage = flashObj.AddComponent<Image>();
        _screenFlashImage.color = new Color(1f, 1f, 1f, 0f);
        _screenFlashImage.raycastTarget = false;
    }

    public void TriggerScreenFlash()
    {
        if (_screenFlashImage != null)
            StartCoroutine(ScreenFlashRoutine());
    }

    private IEnumerator ScreenFlashRoutine()
    {
        _screenFlashImage.color = new Color(1f, 0.9f, 0.7f, 0.8f);
        float elapsed = 0f;
        float duration = 0.15f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            _screenFlashImage.color = new Color(1f, 0.9f, 0.7f, Mathf.Lerp(0.8f, 0f, t));
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        _screenFlashImage.color = new Color(1f, 1f, 1f, 0f);
    }

    // ======================== GAME OVER ========================

    private void BuildGameOverPanel()
    {
        var panelObj = new GameObject("GameOverPanel");
        panelObj.transform.SetParent(_canvasRT, false);
        var panelRT = panelObj.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        var panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.75f);

        _gameOverPanel = panelObj;

        _winnerText = CreateText("GameOverTitle", "", 24f, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 50), new Vector2(400, 40),
            panelRT);

        // PLAY AGAIN button
        var btnObj = new GameObject("PlayAgainBtn");
        btnObj.transform.SetParent(panelRT, false);
        var btnRT = btnObj.AddComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.5f, 0.5f);
        btnRT.anchorMax = new Vector2(0.5f, 0.5f);
        btnRT.anchoredPosition = new Vector2(0, -20);
        btnRT.sizeDelta = new Vector2(200, 50);

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

        _playAgainText = CreateText("PlayAgainLabel", "PLAY AGAIN", 20f, TextAlignmentOptions.Center,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, btnRT);
        _playAgainText.color = Color.white;
        _playAgainText.raycastTarget = false;

        _gameOverPanel.SetActive(false);
    }

    // ======================== HELPERS ========================

    private TextMeshProUGUI CreateText(string name, string text, float fontSize,
        TextAlignmentOptions alignment, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPos, Vector2 sizeDelta, RectTransform parent = null)
    {
        if (parent == null) parent = _canvasRT;

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

    // ======================== HUD UPDATES ========================

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
            string botName = bot != null ? bot.BotName : holder.name;
            _bombHolderText.text = $"{botName} has the bomb";
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

        // Round announcement
        StartCoroutine(RoundAnnounceRoutine(round));

        // Score popup (from round 2 onward)
        if (round > 1)
            StartCoroutine(ScorePopupRoutine());
    }

    private IEnumerator RoundAnnounceRoutine(int round)
    {
        if (_roundAnnounceText == null) yield break;

        _roundAnnounceText.text = $"ROUND {round}";
        _roundAnnounceText.gameObject.SetActive(true);

        // Scale up from 0
        var rt = _roundAnnounceText.GetComponent<RectTransform>();
        float elapsed = 0f;

        // Scale in (0.2s)
        while (elapsed < 0.2f)
        {
            float t = elapsed / 0.2f;
            rt.localScale = Vector3.one * Mathf.Lerp(0f, 1.2f, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        rt.localScale = Vector3.one;

        // Hold (1s)
        yield return new WaitForSeconds(1f);

        // Fade out (0.5s)
        elapsed = 0f;
        while (elapsed < 0.5f)
        {
            float t = elapsed / 0.5f;
            _roundAnnounceText.alpha = Mathf.Lerp(1f, 0f, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _roundAnnounceText.alpha = 1f;
        _roundAnnounceText.gameObject.SetActive(false);
    }

    private IEnumerator ScorePopupRoutine()
    {
        if (_scorePopupText == null) yield break;

        _scorePopupText.text = "+1";
        _scorePopupText.gameObject.SetActive(true);

        var rt = _scorePopupText.GetComponent<RectTransform>();
        Vector2 startPos = rt.anchoredPosition;

        float elapsed = 0f;
        float duration = 1f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            _scorePopupText.alpha = Mathf.Lerp(1f, 0f, t);
            rt.anchoredPosition = startPos + new Vector2(0, t * 50f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rt.anchoredPosition = startPos;
        _scorePopupText.alpha = 1f;
        _scorePopupText.gameObject.SetActive(false);
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
            _winnerText.text = winner == "You" ? "YOU WIN!" : "GAME OVER";
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

        while (elapsed < 2f)
        {
            float angle = Mathf.Sin(elapsed * 12f) * 25f;
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
