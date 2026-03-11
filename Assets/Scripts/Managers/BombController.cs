using UnityEngine;
using System;

public class BombController : MonoBehaviour
{
    public static BombController Instance { get; private set; }

    [Header("Bomb Settings")]
    [SerializeField] private float _roundTime = 15f;
    [SerializeField] private float _transferCooldown = 0.5f;
    [SerializeField] private float _detectRadius = 1.5f;
    [SerializeField] private Vector3 _followOffset = new Vector3(0f, 1.2f, 0f);

    private GameObject _currentHolder;
    private float _timer;
    private float _transferCooldownTimer;
    private bool _isActive;
    private Renderer _renderer;
    private Color _baseColor = Color.yellow;

    public GameObject CurrentHolder => _currentHolder;
    public float Timer => _timer;
    public float RoundTime => _roundTime;
    public bool IsActive => _isActive;

    public static event Action<float> OnBombTimerChanged;
    public static event Action<GameObject> OnBombTransferred;
    public static event Action<GameObject> OnBombExploded;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        if (!_isActive) return;

        // Follow holder
        if (_currentHolder != null)
            transform.position = _currentHolder.transform.position + _followOffset;

        // Countdown
        _timer -= Time.deltaTime;
        OnBombTimerChanged?.Invoke(_timer);

        // Transfer cooldown
        if (_transferCooldownTimer > 0f)
            _transferCooldownTimer -= Time.deltaTime;

        // Visual: flash faster as timer decreases
        UpdateVisual();

        // Check for nearby characters to transfer
        CheckTransfer();

        // Explode when timer runs out
        if (_timer <= 0f)
        {
            _timer = 0f;
            Explode();
        }
    }

    private void CheckTransfer()
    {
        if (_currentHolder == null || _transferCooldownTimer > 0f) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, _detectRadius);
        foreach (var hit in hits)
        {
            if (hit.gameObject == _currentHolder) continue;

            // Check if it's an alive player or bot
            var player = hit.GetComponent<PlayerController>();
            if (player != null && player.IsAlive)
            {
                TransferTo(hit.gameObject);
                return;
            }

            var bot = hit.GetComponent<BotController>();
            if (bot != null && bot.IsAlive)
            {
                TransferTo(hit.gameObject);
                return;
            }
        }
    }

    private void TransferTo(GameObject newHolder)
    {
        _currentHolder = newHolder;
        _transferCooldownTimer = _transferCooldown;
        OnBombTransferred?.Invoke(_currentHolder);
    }

    private void UpdateVisual()
    {
        if (_renderer == null) return;

        // Flash between yellow and red, faster as timer decreases
        float flashSpeed = Mathf.Lerp(2f, 15f, 1f - (_timer / _roundTime));
        float t = (Mathf.Sin(Time.time * flashSpeed) + 1f) * 0.5f;
        _renderer.material.color = Color.Lerp(_baseColor, Color.red, t);
    }

    public void AssignBomb(GameObject holder)
    {
        _currentHolder = holder;
        _timer = _roundTime;
        _transferCooldownTimer = _transferCooldown;
        _isActive = true;
        gameObject.SetActive(true);

        OnBombTransferred?.Invoke(_currentHolder);
        OnBombTimerChanged?.Invoke(_timer);
    }

    private void Explode()
    {
        _isActive = false;
        GameObject victim = _currentHolder;

        CameraShake.Instance?.Shake();
        OnBombExploded?.Invoke(victim);

        // Eliminate the holder
        if (victim != null)
        {
            var player = victim.GetComponent<PlayerController>();
            if (player != null) player.Eliminate();

            var bot = victim.GetComponent<BotController>();
            if (bot != null) bot.Eliminate();
        }

        gameObject.SetActive(false);
    }

    public void Deactivate()
    {
        _isActive = false;
        gameObject.SetActive(false);
    }
}
