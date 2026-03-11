using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, GameOver }
    private GameState _currentState = GameState.Playing;

    public GameState CurrentState => _currentState;

    [Header("Session Timer (Luxodd: max 10 min)")]
    [SerializeField] private float _maxSessionTime = 600f;
    private float _sessionTimeRemaining;

    [Header("Inactivity Timeout")]
    [SerializeField] private float _menuTimeoutSeconds = 30f;
    [SerializeField] private float _startTimeoutSeconds = 60f;
    private float _inactivityTimer;
    private float _gameOverTimer;
    private bool _hasReceivedFirstInput = false;

    public float SessionTimeRemaining => _sessionTimeRemaining;

    public delegate void GameStateEvent(GameState state);
    public static event GameStateEvent OnGameStateChanged;

    public delegate void TimerEvent(float timeRemaining);
    public static event TimerEvent OnTimerChanged;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        Application.targetFrameRate = -1;
    }

    void Start()
    {
        _sessionTimeRemaining = _maxSessionTime;
    }

    void OnEnable()
    {
        RoundManager.OnGameOver += HandleGameOver;
        PlayerController.OnPlayerEliminated += HandlePlayerEliminated;
    }

    void OnDisable()
    {
        RoundManager.OnGameOver -= HandleGameOver;
        PlayerController.OnPlayerEliminated -= HandlePlayerEliminated;
    }

    void Update()
    {
        if (_currentState == GameState.GameOver)
        {
            _gameOverTimer += Time.unscaledDeltaTime;
            if (_gameOverTimer >= _menuTimeoutSeconds)
                EndSession();
            return;
        }

        // Session timer countdown
        _sessionTimeRemaining -= Time.deltaTime;
        OnTimerChanged?.Invoke(_sessionTimeRemaining);

        if (_sessionTimeRemaining <= 0f)
        {
            _sessionTimeRemaining = 0f;
            TriggerGameOver();
            return;
        }

        // Inactivity detection
        bool anyInput = InputManager.Instance?.Input != null
            && Mathf.Abs(InputManager.Instance.Input.MoveHorizontal) > 0.1f;

        if (anyInput)
        {
            _inactivityTimer = 0f;
            _hasReceivedFirstInput = true;
        }
        else
        {
            _inactivityTimer += Time.deltaTime;
        }

        if (!_hasReceivedFirstInput && _inactivityTimer >= _startTimeoutSeconds)
            EndSession();
    }

    private void HandleGameOver(string winnerName)
    {
        TriggerGameOver();
    }

    private void HandlePlayerEliminated()
    {
        TriggerGameOver();
    }

    private void TriggerGameOver()
    {
        if (_currentState == GameState.GameOver) return;

        _currentState = GameState.GameOver;
        _gameOverTimer = 0f;
        Time.timeScale = 1f;
        OnGameStateChanged?.Invoke(_currentState);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void EndSession()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void QuitGame()
    {
        EndSession();
    }
}
