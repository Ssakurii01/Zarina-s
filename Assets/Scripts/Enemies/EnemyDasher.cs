using UnityEngine;

/// <summary>
/// Dasher enemy — pauses, then dashes at high speed toward the player.
/// Appears from Wave 3.
/// </summary>
public class EnemyDasher : EnemyBase
{
    [Header("Dash Settings")]
    [SerializeField] private float _dashSpeed = 18f;
    [SerializeField] private float _pauseDuration = 1.5f;
    [SerializeField] private float _dashDuration = 0.4f;

    private enum DashState { Moving, Pausing, Dashing }
    private DashState _state = DashState.Moving;
    private float _stateTimer;
    private Vector3 _dashDirection;

    protected override void Start()
    {
        _maxHP = 2;
        _moveSpeed = 3f;
        _scoreValue = 200;
        base.Start();
        _stateTimer = Random.Range(1f, 3f); // Random first pause time
    }

    protected override void Update()
    {
        if (!_isAlive || _playerTransform == null) return;

        _stateTimer -= Time.deltaTime;

        switch (_state)
        {
            case DashState.Moving:
                MoveTowardsPlayer();
                if (_stateTimer <= 0f)
                {
                    _state = DashState.Pausing;
                    _stateTimer = _pauseDuration;
                }
                break;

            case DashState.Pausing:
                // Stand still, face the player
                Vector3 lookDir = (_playerTransform.position - transform.position).normalized;
                lookDir.z = 0;
                if (lookDir.sqrMagnitude > 0.01f)
                {
                    float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
                    transform.rotation = Quaternion.Euler(0, 0, angle);
                }

                if (_stateTimer <= 0f)
                {
                    _state = DashState.Dashing;
                    _dashDirection = lookDir;
                    _stateTimer = _dashDuration;
                }
                break;

            case DashState.Dashing:
                transform.position += _dashDirection * _dashSpeed * Time.deltaTime;
                if (_stateTimer <= 0f)
                {
                    _state = DashState.Moving;
                    _stateTimer = Random.Range(2f, 4f);
                }
                break;
        }
    }
}
