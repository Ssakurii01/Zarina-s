using UnityEngine;
using System;

public class KeyboardInputAdapter : MonoBehaviour, IInputAdapter
{
    public float MoveHorizontal { get; private set; }
    public bool IsFireHeld { get; private set; }
    public Vector2 AimDirection { get; private set; }

    public event Action OnJumpPressed;

    private GameObject _playerObj;
    private GameObject[] _cachedEnemies;
    private float _enemyCacheTimer;
    private const float ENEMY_CACHE_INTERVAL = 0.1f;

    void Update()
    {
        MoveHorizontal = Input.GetAxisRaw("Horizontal");

        IsFireHeld = Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.Z);

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            OnJumpPressed?.Invoke();
        }

        AimDirection = FindNearestEnemyDirection();
    }

    private Vector2 FindNearestEnemyDirection()
    {
        if (_playerObj == null || !_playerObj.activeInHierarchy)
            _playerObj = GameObject.FindGameObjectWithTag("Player");

        if (_playerObj == null) return Vector2.up;

        _enemyCacheTimer -= Time.deltaTime;
        if (_enemyCacheTimer <= 0f || _cachedEnemies == null)
        {
            _cachedEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            _enemyCacheTimer = ENEMY_CACHE_INTERVAL;
        }

        if (_cachedEnemies.Length == 0) return Vector2.up;

        float closestDist = float.MaxValue;
        Vector3 closestDir = Vector3.up;

        foreach (GameObject enemy in _cachedEnemies)
        {
            if (enemy == null) continue;
            Vector3 toEnemy = enemy.transform.position - _playerObj.transform.position;
            toEnemy.z = 0f;
            float dist = toEnemy.sqrMagnitude;
            if (dist < closestDist)
            {
                closestDist = dist;
                closestDir = toEnemy;
            }
        }

        return closestDir.normalized;
    }
}
