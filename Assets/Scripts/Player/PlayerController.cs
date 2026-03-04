using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 8f;
    [SerializeField] private float _rotationSpeed = 15f;

    [Header("Shooting")]
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _fireRate = 0.15f;
    [SerializeField] private float _bulletSpeed = 30f;
    [SerializeField] private float _bulletLifetime = 3f;

    [Header("Jumping")]
    [SerializeField] private float _jumpForce = 8f;
    [SerializeField] private LayerMask _groundLayer;

    [Header("Health")]
    [SerializeField] private int _maxHP = 5;
    private int _currentHP;

    [Header("Power-Ups")]
    private bool _hasShield = false;
    private bool _hasSpreadShot = false;
    private bool _hasSpeedBoost = false;
    private float _spreadShotTimer = 0f;
    private float _speedBoostTimer = 0f;
    private float _spreadShotDuration = 8f;
    private float _speedBoostDuration = 6f;

    private float _nextFireTime = 0f;
    private Rigidbody _rb;

    public int CurrentHP => _currentHP;
    public int MaxHP => _maxHP;
    public bool HasShield => _hasShield;

    public delegate void PlayerEvent();
    public delegate void PlayerHealthEvent(int currentHP, int maxHP);
    public static event PlayerHealthEvent OnHealthChanged;
    public static event PlayerEvent OnPlayerDeath;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _currentHP = _maxHP;

        _rb.freezeRotation = true;
        _rb.constraints = RigidbodyConstraints.FreezePositionZ
                        | RigidbodyConstraints.FreezeRotationX
                        | RigidbodyConstraints.FreezeRotationY;

        if (InputManager.Instance?.Input != null)
            InputManager.Instance.Input.OnJumpPressed += HandleJump;

        OnHealthChanged?.Invoke(_currentHP, _maxHP);
    }

    void OnDestroy()
    {
        if (InputManager.Instance?.Input != null)
            InputManager.Instance.Input.OnJumpPressed -= HandleJump;
    }

    void Update()
    {
        HandleRotation();
        HandleShooting();
        HandlePowerUpTimers();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        float horizontal = InputManager.Instance?.Input?.MoveHorizontal ?? 0f;
        float speed = _hasSpeedBoost ? _moveSpeed * 2f : _moveSpeed;
        _rb.linearVelocity = new Vector3(horizontal * speed, _rb.linearVelocity.y, 0f);
    }

    private void HandleJump()
    {
        if (IsGrounded())
        {
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, 0f);
            _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 0.6f, _groundLayer);
    }

    private void HandleRotation()
    {
        Vector2 aimDir = InputManager.Instance?.Input?.AimDirection ?? Vector2.up;

        if (aimDir.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                _rotationSpeed * Time.deltaTime);
        }
    }

    private void HandleShooting()
    {
        bool isAttacking = InputManager.Instance?.Input?.IsFireHeld ?? false;

        if (isAttacking && Time.time >= _nextFireTime)
        {
            _nextFireTime = Time.time + _fireRate;
            Shoot();
        }
    }

    private void Shoot()
    {
        if (_bulletPrefab == null || _firePoint == null) return;

        SpawnBullet(_firePoint.position, transform.up);

        if (_hasSpreadShot)
        {
            SpawnBullet(_firePoint.position, Quaternion.Euler(0, 0, 20f) * transform.up);
            SpawnBullet(_firePoint.position, Quaternion.Euler(0, 0, -20f) * transform.up);
        }
    }

    private void SpawnBullet(Vector3 position, Vector3 direction)
    {
        GameObject bullet = Instantiate(_bulletPrefab, position, Quaternion.LookRotation(Vector3.forward, direction));
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = direction.normalized * _bulletSpeed;
            bulletRb.constraints = RigidbodyConstraints.FreezePositionZ;
        }
        Destroy(bullet, _bulletLifetime);
    }

    private void HandlePowerUpTimers()
    {
        if (_hasSpreadShot)
        {
            _spreadShotTimer -= Time.deltaTime;
            if (_spreadShotTimer <= 0f) _hasSpreadShot = false;
        }

        if (_hasSpeedBoost)
        {
            _speedBoostTimer -= Time.deltaTime;
            if (_speedBoostTimer <= 0f) _hasSpeedBoost = false;
        }
    }

    public void TakeDamage(int damage)
    {
        if (_hasShield)
        {
            _hasShield = false;
            return;
        }

        _currentHP -= damage;
        OnHealthChanged?.Invoke(_currentHP, _maxHP);
        CameraShake.Instance?.Shake();

        if (_currentHP <= 0)
        {
            _currentHP = 0;
            OnPlayerDeath?.Invoke();
            gameObject.SetActive(false);
        }
    }

    public void Heal(int amount)
    {
        _currentHP = Mathf.Min(_currentHP + amount, _maxHP);
        OnHealthChanged?.Invoke(_currentHP, _maxHP);
    }

    public void ActivateShield()
    {
        _hasShield = true;
    }

    public void ActivateSpreadShot()
    {
        _hasSpreadShot = true;
        _spreadShotTimer = _spreadShotDuration;
    }

    public void ActivateSpeedBoost()
    {
        _hasSpeedBoost = true;
        _speedBoostTimer = _speedBoostDuration;
    }
}
