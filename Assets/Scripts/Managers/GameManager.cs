using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, Paused, GameOver }
    private GameState _currentState = GameState.Playing;

    public GameState CurrentState => _currentState;

    public delegate void GameStateEvent(GameState state);
    public static event GameStateEvent OnGameStateChanged;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void OnEnable()
    {
        PlayerController.OnPlayerDeath += HandlePlayerDeath;
    }

    void OnDisable()
    {
        PlayerController.OnPlayerDeath -= HandlePlayerDeath;
    }

    void Update()
    {
        if (_currentState == GameState.GameOver) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_currentState == GameState.Playing)
                PauseGame();
            else if (_currentState == GameState.Paused)
                ResumeGame();
        }
    }

    private void HandlePlayerDeath()
    {
        _currentState = GameState.GameOver;
        Time.timeScale = 1f;
        OnGameStateChanged?.Invoke(_currentState);
    }

    public void PauseGame()
    {
        _currentState = GameState.Paused;
        Time.timeScale = 0f;
        OnGameStateChanged?.Invoke(_currentState);
    }

    public void ResumeGame()
    {
        _currentState = GameState.Playing;
        Time.timeScale = 1f;
        OnGameStateChanged?.Invoke(_currentState);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
