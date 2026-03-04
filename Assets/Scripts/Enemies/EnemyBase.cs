using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected int _maxHP = 1;
    [SerializeField] protected float _moveSpeed = 4f;
    [SerializeField] protected int _damage = 1;
    [SerializeField] protected int _scoreValue = 100;

    [Header("Effects")]
    [SerializeField] protected GameObject _deathEffectPrefab;
    [SerializeField] protected GameObject _powerUpDropPrefab;
    [SerializeField] [Range(0f, 1f)] protected float _powerUpDropChance = 0.15f;

    protected int _currentHP;
    protected Transform _playerTransform;
    protected bool _isAlive = true;

    private Renderer _cachedRenderer;
    private Color _originalColor;

    public delegate void EnemyEvent(EnemyBase enemy);
    public static event EnemyEvent OnEnemyDeath;

    protected virtual void Start()
    {
        _currentHP = _maxHP;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }

        _cachedRenderer = GetComponentInChildren<Renderer>();
        if (_cachedRenderer != null)
            _originalColor = _cachedRenderer.material.color;
    }

    protected virtual void Update()
    {
        if (!_isAlive || _playerTransform == null) return;
        MoveTowardsPlayer();
    }

    protected virtual void MoveTowardsPlayer()
    {
        Vector3 direction = (_playerTransform.position - transform.position).normalized;
        direction.z = 0;
        transform.position += direction * _moveSpeed * Time.deltaTime;

        // Face movement direction in 2D plane
        if (direction.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public virtual void TakeDamage(int damage)
    {
        if (!_isAlive) return;

        _currentHP -= damage;
        // Flash red effect
        StartCoroutine(FlashRed());

        if (_currentHP <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        _isAlive = false;

        // Spawn death effect
        if (_deathEffectPrefab != null)
        {
            GameObject effect = Instantiate(_deathEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }

        // Chance to drop power-up
        if (_powerUpDropPrefab != null && Random.value <= _powerUpDropChance)
        {
            // Spawn power-up on the Same XY plane
            Instantiate(_powerUpDropPrefab, transform.position, Quaternion.identity);
        }

        OnEnemyDeath?.Invoke(this);
        ScoreManager.Instance?.AddScore(_scoreValue);
        CameraShake.Instance?.Shake(0.1f, 0.15f);

        Destroy(gameObject);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(_damage);
            }
        }
    }

    private System.Collections.IEnumerator FlashRed()
    {
        if (_cachedRenderer != null)
        {
            _cachedRenderer.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            if (_cachedRenderer != null)
                _cachedRenderer.material.color = _originalColor;
        }
    }
}
