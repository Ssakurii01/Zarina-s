using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Combo Settings")]
    [SerializeField] private float _comboResetTime = 3f;

    private int _score = 0;
    private int _highScore = 0;
    private int _comboMultiplier = 1;
    private int _comboKillCount = 0;
    private float _comboTimer = 0f;

    public int Score => _score;
    public int HighScore => _highScore;
    public int ComboMultiplier => _comboMultiplier;

    public delegate void ScoreEvent(int score, int combo);
    public static event ScoreEvent OnScoreChanged;
    public static event ScoreEvent OnComboChanged;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    void Update()
    {
        if (_comboMultiplier > 1)
        {
            _comboTimer -= Time.deltaTime;
            if (_comboTimer <= 0f)
            {
                ResetCombo();
            }
        }
    }

    public void AddScore(int basePoints)
    {
        int points = basePoints * _comboMultiplier;
        _score += points;

        // Update combo
        _comboKillCount++;
        _comboTimer = _comboResetTime;

        // Increase multiplier at kill thresholds
        if (_comboKillCount >= 20) _comboMultiplier = 5;
        else if (_comboKillCount >= 12) _comboMultiplier = 4;
        else if (_comboKillCount >= 6) _comboMultiplier = 3;
        else if (_comboKillCount >= 3) _comboMultiplier = 2;

        // Update high score
        if (_score > _highScore)
        {
            _highScore = _score;
            PlayerPrefs.SetInt("HighScore", _highScore);
        }

        OnScoreChanged?.Invoke(_score, _comboMultiplier);
        OnComboChanged?.Invoke(_comboKillCount, _comboMultiplier);
    }

    private void ResetCombo()
    {
        _comboMultiplier = 1;
        _comboKillCount = 0;
        OnComboChanged?.Invoke(0, 1);
    }

    public void ResetScore()
    {
        _score = 0;
        ResetCombo();
        OnScoreChanged?.Invoke(0, 1);
    }
}
