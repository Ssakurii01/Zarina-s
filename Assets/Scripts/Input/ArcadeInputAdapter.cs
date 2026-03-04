using UnityEngine;
using System;

public class ArcadeInputAdapter : MonoBehaviour, IInputAdapter
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
#if LUXODD_PLUGIN
        StickData stick = ArcadeControls.GetStick();
        MoveHorizontal = stick.Vector.x;

        if (ArcadeControls.GetButtonDown(ArcadeButtonColor.Red))
            IsFireHeld = true;
        if (ArcadeControls.GetButtonUp(ArcadeButtonColor.Red))
            IsFireHeld = false;

        if (ArcadeControls.GetButtonDown(ArcadeButtonColor.Black))
            OnJumpPressed?.Invoke();
#else
        // Fallback: standard Unity joystick/gamepad input for testing
        MoveHorizontal = Input.GetAxisRaw("Horizontal");
        IsFireHeld = Input.GetButton("Fire1");
        if (Input.GetButtonDown("Jump"))
            OnJumpPressed?.Invoke();
#endif

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
