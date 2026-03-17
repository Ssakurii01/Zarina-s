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

    private Renderer _holderRenderer;
    private Color _holderOriginalColor;

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

        // Tick sound (gets faster as timer decreases)
        SFXManager.Instance?.PlayBombTick(_timer / _roundTime);

        // Transfer cooldown
        if (_transferCooldownTimer > 0f)
            _transferCooldownTimer -= Time.deltaTime;

        // Visual: flash faster as timer decreases
        UpdateVisual();
        UpdateHolderGlow();

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
                if (player.ConsumeShield()) continue; // shield blocks transfer
                TransferTo(hit.gameObject);
                return;
            }

            var bot = hit.GetComponent<BotController>();
            if (bot != null && bot.IsAlive)
            {
                if (bot.ConsumeShield()) continue; // shield blocks transfer
                TransferTo(hit.gameObject);
                return;
            }
        }
    }

    private void TransferTo(GameObject newHolder)
    {
        ResetHolderGlow();
        _currentHolder = newHolder;
        ApplyHolderGlow();
        _transferCooldownTimer = _transferCooldown;
        SFXManager.Instance?.PlayBombTransfer();
        OnBombTransferred?.Invoke(_currentHolder);
    }

    private void ApplyHolderGlow()
    {
        if (_currentHolder == null) return;
        _holderRenderer = _currentHolder.GetComponent<Renderer>();
        if (_holderRenderer != null)
            _holderOriginalColor = _holderRenderer.material.color;
    }

    private void ResetHolderGlow()
    {
        if (_holderRenderer != null)
        {
            _holderRenderer.material.color = _holderOriginalColor;
            _holderRenderer = null;
        }
    }

    private void UpdateHolderGlow()
    {
        if (_holderRenderer == null) return;
        float pulse = (Mathf.Sin(Time.time * 8f) + 1f) * 0.5f;
        _holderRenderer.material.color = Color.Lerp(_holderOriginalColor, new Color(1f, 0.3f, 0f), pulse * 0.6f);
    }

    private void UpdateVisual()
    {
        if (_renderer == null) return;

        // Flash between yellow and red, faster as timer decreases
        float flashSpeed = Mathf.Lerp(2f, 15f, 1f - (_timer / _roundTime));
        float t = (Mathf.Sin(Time.time * flashSpeed) + 1f) * 0.5f;
        _renderer.material.color = Color.Lerp(_baseColor, Color.red, t);
    }

    public void AssignBomb(GameObject holder, int round = 1)
    {
        ResetHolderGlow();
        _currentHolder = holder;
        ApplyHolderGlow();
        // Difficulty: timer decreases each round (15s -> min 5s)
        _timer = Mathf.Max(5f, _roundTime - (round - 1) * 1f);
        _transferCooldownTimer = _transferCooldown;
        _isActive = true;
        gameObject.SetActive(true);

        OnBombTransferred?.Invoke(_currentHolder);
        OnBombTimerChanged?.Invoke(_timer);
    }

    private void Explode()
    {
        _isActive = false;
        ResetHolderGlow();
        GameObject victim = _currentHolder;

        SFXManager.Instance?.PlayBombExplode();
        CameraShake.Instance?.Shake();
        ExplosionEffect.SpawnAt(transform.position);

        // Screen flash
        var gameUI = Object.FindFirstObjectByType<GameUI>();
        if (gameUI != null) gameUI.TriggerScreenFlash();

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
