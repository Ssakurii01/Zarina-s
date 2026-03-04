using UnityEngine;

/// <summary>
/// Orbiter enemy — circles around the player while slowly closing in.
/// Appears from Wave 5.
/// </summary>
public class EnemyOrbiter : EnemyBase
{
    [Header("Orbit Settings")]
    [SerializeField] private float _orbitSpeed = 90f; // degrees per second
    [SerializeField] private float _closeInSpeed = 0.5f;

    private float _currentAngle;
    private float _currentRadius;

    protected override void Start()
    {
        _maxHP = 3;
        _moveSpeed = 3f;
        _scoreValue = 300;
        base.Start();

        // Calculate initial angle and radius from player
        if (_playerTransform != null)
        {
            Vector3 offset = transform.position - _playerTransform.position;
            _currentRadius = new Vector2(offset.x, offset.y).magnitude;
            _currentAngle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
        }
    }

    protected override void Update()
    {
        if (!_isAlive || _playerTransform == null) return;

        // Orbit around player
        _currentAngle += _orbitSpeed * Time.deltaTime;
        _currentRadius -= _closeInSpeed * Time.deltaTime;
        _currentRadius = Mathf.Max(_currentRadius, 1f); // Don't go inside the player

        float radians = _currentAngle * Mathf.Deg2Rad;
        Vector3 targetPos = _playerTransform.position + new Vector3(
            Mathf.Cos(radians) * _currentRadius,
            Mathf.Sin(radians) * _currentRadius,
            0f
        );

        transform.position = Vector3.Lerp(transform.position, targetPos, 5f * Time.deltaTime);

        // Face the player
        Vector3 lookDir = (_playerTransform.position - transform.position).normalized;
        if (lookDir.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
