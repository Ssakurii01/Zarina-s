using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private GameObject _botPrefab;
    [SerializeField] private GameObject _portalPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] _spawnPoints;

    [Header("Settings")]
    [SerializeField] private int _botCount = 5;
    [SerializeField] private float _delayBetweenRounds = 3f;
    [SerializeField] private int _deathsPerPortal = 2;

    private List<GameObject> _aliveCharacters = new List<GameObject>();
    private int _currentRound;
    private int _deathsSinceLastPortal;
    private GameObject _player;
    private string _winnerName;

    public IReadOnlyList<GameObject> AliveCharacters => _aliveCharacters;
    public int CurrentRound => _currentRound;
    public string WinnerName => _winnerName;

    public static event Action<int> OnRoundStart;
    public static event Action<string> OnCharacterEliminated;
    public static event Action<string> OnGameOver; // winner name
    public static event Action<int> OnAliveCountChanged;

    private static readonly string[] BotNames = { "Alpha", "Bravo", "Charlie", "Delta", "Echo" };

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        SpawnBots();
        StartCoroutine(StartRoundAfterDelay(1f));
    }

    void OnEnable()
    {
        BombController.OnBombExploded += HandleBombExploded;
    }

    void OnDisable()
    {
        BombController.OnBombExploded -= HandleBombExploded;
    }

    private void SpawnBots()
    {
        if (_botPrefab == null) return;

        for (int i = 0; i < _botCount; i++)
        {
            Vector3 spawnPos;
            if (_spawnPoints != null && i < _spawnPoints.Length && _spawnPoints[i] != null)
                spawnPos = _spawnPoints[i].position;
            else
            {
                float halfW = (ArenaSetup.Instance?.ArenaWidth ?? 16f) * 0.45f;
                float halfH = (ArenaSetup.Instance?.ArenaHeight ?? 32f) * 0.45f;
                spawnPos = new Vector3(UnityEngine.Random.Range(-halfW, halfW), UnityEngine.Random.Range(-halfH, halfH), 0f);
            }

            GameObject bot = Instantiate(_botPrefab, spawnPos, Quaternion.identity);
            var botCtrl = bot.GetComponent<BotController>();
            if (botCtrl != null)
                botCtrl.BotName = BotNames[i % BotNames.Length];

            bot.name = $"Bot_{BotNames[i % BotNames.Length]}";
            _aliveCharacters.Add(bot);
        }

        // Add player to alive list
        if (_player != null)
            _aliveCharacters.Add(_player);

        OnAliveCountChanged?.Invoke(_aliveCharacters.Count);
    }

    private IEnumerator StartRoundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartNewRound();
    }

    private void StartNewRound()
    {
        _currentRound++;

        // Pick random alive character to hold bomb
        if (_aliveCharacters.Count == 0) return;

        int randomIndex = UnityEngine.Random.Range(0, _aliveCharacters.Count);
        GameObject bombHolder = _aliveCharacters[randomIndex];

        if (BombController.Instance != null)
            BombController.Instance.AssignBomb(bombHolder, _currentRound);

        SFXManager.Instance?.PlayRoundStart();
        OnRoundStart?.Invoke(_currentRound);
    }

    private void HandleBombExploded(GameObject victim)
    {
        if (victim != null)
        {
            _aliveCharacters.Remove(victim);
            _deathsSinceLastPortal++;

            string name = GetCharacterName(victim);
            OnCharacterEliminated?.Invoke(name);
            OnAliveCountChanged?.Invoke(_aliveCharacters.Count);

            // Spawn portal every N deaths
            if (_deathsSinceLastPortal >= _deathsPerPortal)
            {
                _deathsSinceLastPortal = 0;
                SpawnPortal();
            }
        }

        // Check win condition
        if (_aliveCharacters.Count <= 1)
        {
            if (_aliveCharacters.Count == 1)
                _winnerName = GetCharacterName(_aliveCharacters[0]);
            else
                _winnerName = "Nobody";

            OnGameOver?.Invoke(_winnerName);
            return;
        }

        // Next round after delay
        StartCoroutine(StartRoundAfterDelay(_delayBetweenRounds));
    }

    private string GetCharacterName(GameObject character)
    {
        if (character == null) return "Unknown";

        var player = character.GetComponent<PlayerController>();
        if (player != null) return "You";

        var bot = character.GetComponent<BotController>();
        if (bot != null) return bot.BotName;

        return character.name;
    }

    private void SpawnPortal()
    {
        if (_portalPrefab == null) return;

        float halfW = (ArenaSetup.Instance?.ArenaWidth ?? 16f) * 0.4f;
        float halfH = (ArenaSetup.Instance?.ArenaHeight ?? 32f) * 0.4f;
        Vector3 pos = new Vector3(UnityEngine.Random.Range(-halfW, halfW), UnityEngine.Random.Range(-halfH, halfH), 0f);
        Instantiate(_portalPrefab, pos, Quaternion.identity);
    }
}
