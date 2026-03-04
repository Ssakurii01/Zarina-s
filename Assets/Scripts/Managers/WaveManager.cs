using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject _swarmerPrefab;
    [SerializeField] private GameObject _dasherPrefab;
    [SerializeField] private GameObject _orbiterPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float _arenaRadius = 20f;
    [SerializeField] private float _spawnDelay = 0.3f;
    [SerializeField] private float _timeBetweenWaves = 3f;

    [Header("Wave Settings")]
    [SerializeField] private int _baseEnemyCount = 5;
    [SerializeField] private int _enemiesPerWaveIncrease = 3;

    private int _currentWave = 0;
    private int _enemiesAlive = 0;
    private bool _spawning = false;

    public int CurrentWave => _currentWave;

    public delegate void WaveEvent(int waveNumber);
    public static event WaveEvent OnWaveStart;
    public static event WaveEvent OnWaveComplete;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void OnEnable()
    {
        EnemyBase.OnEnemyDeath += HandleEnemyDeath;
    }

    void OnDisable()
    {
        EnemyBase.OnEnemyDeath -= HandleEnemyDeath;
    }

    void Start()
    {
        StartCoroutine(StartNextWave());
    }

    private void HandleEnemyDeath(EnemyBase enemy)
    {
        _enemiesAlive--;

        if (_enemiesAlive <= 0 && !_spawning)
        {
            OnWaveComplete?.Invoke(_currentWave);
            StartCoroutine(StartNextWave());
        }
    }

    private IEnumerator StartNextWave()
    {
        yield return new WaitForSeconds(_timeBetweenWaves);

        _currentWave++;
        _spawning = true;

        OnWaveStart?.Invoke(_currentWave);

        List<GameObject> enemiesToSpawn = GetEnemiesForWave(_currentWave);
        _enemiesAlive = enemiesToSpawn.Count;

        foreach (GameObject enemyPrefab in enemiesToSpawn)
        {
            SpawnEnemy(enemyPrefab);
            yield return new WaitForSeconds(_spawnDelay);
        }

        _spawning = false;

        // Check if all enemies died during spawning
        if (_enemiesAlive <= 0)
        {
            OnWaveComplete?.Invoke(_currentWave);
            StartCoroutine(StartNextWave());
        }
    }

    private List<GameObject> GetEnemiesForWave(int wave)
    {
        List<GameObject> enemies = new List<GameObject>();
        int totalEnemies = _baseEnemyCount + (wave - 1) * _enemiesPerWaveIncrease;

        for (int i = 0; i < totalEnemies; i++)
        {
            // Determine enemy type based on wave progression
            if (wave >= 5 && Random.value < 0.2f && _orbiterPrefab != null)
            {
                enemies.Add(_orbiterPrefab);
            }
            else if (wave >= 3 && Random.value < 0.3f && _dasherPrefab != null)
            {
                enemies.Add(_dasherPrefab);
            }
            else
            {
                enemies.Add(_swarmerPrefab);
            }
        }

        return enemies;
    }

    private void SpawnEnemy(GameObject prefab)
    {
        if (prefab == null) return;

        // Spawn at random position in a circle on the XY plane
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 spawnPos = new Vector3(
            Mathf.Cos(angle) * _arenaRadius,
            Mathf.Sin(angle) * _arenaRadius,
            0f
        );

        Instantiate(prefab, spawnPos, Quaternion.identity);
    }
}
