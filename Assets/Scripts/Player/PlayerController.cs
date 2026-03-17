using UnityEngine;
using System;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 15f;

    [Header("Jumping")]
    [SerializeField] private float _jumpForce = 5.5f;
    [SerializeField] private LayerMask _groundLayer;

    private Rigidbody _rb;
    private bool _isAlive = true;
    private bool _hasShield;
    private float _speedMultiplier = 1f;
    private float _speedBoostTimer;

    public bool IsAlive => _isAlive;
    public bool HasShield => _hasShield;

    public static event Action OnPlayerEliminated;

    void Start()
    {
        // Force values at runtime (overrides scene-serialized values)
        _moveSpeed = 15f;
        _jumpForce = 5.5f;

        _rb = GetComponent<Rigidbody>();

        _rb.useGravity = true;
        _rb.freezeRotation = true;
        _rb.constraints = RigidbodyConstraints.FreezePositionZ
                        | RigidbodyConstraints.FreezeRotationX
                        | RigidbodyConstraints.FreezeRotationY;

        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = false;

        if (InputManager.Instance?.Input != null)
            InputManager.Instance.Input.OnJumpPressed += HandleJump;

        // Add trail renderer for visual polish
        if (GetComponent<TrailRenderer>() == null)
        {
            var trail = gameObject.AddComponent<TrailRenderer>();
            trail.time = 0.15f;
            trail.startWidth = 0.15f;
            trail.endWidth = 0f;
            trail.startColor = new Color(1f, 1f, 1f, 0.4f);
            trail.endColor = new Color(1f, 1f, 1f, 0f);
            trail.material = new Material(Shader.Find("Sprites/Default"));
        }
    }

    void OnDestroy()
    {
        if (InputManager.Instance?.Input != null)
            InputManager.Instance.Input.OnJumpPressed -= HandleJump;
    }

    void Update()
    {
        if (_speedBoostTimer > 0f)
        {
            _speedBoostTimer -= Time.deltaTime;
            if (_speedBoostTimer <= 0f) _speedMultiplier = 1f;
        }
    }

    void FixedUpdate()
    {
        if (!_isAlive) return;
        HandleMovement();
    }

    private void HandleMovement()
    {
        float horizontal = InputManager.Instance?.Input?.MoveHorizontal ?? 0f;
        _rb.linearVelocity = new Vector3(horizontal * _moveSpeed * _speedMultiplier, _rb.linearVelocity.y, 0f);
    }

    private void HandleJump()
    {
        if (!_isAlive) return;
        if (IsGrounded())
        {
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, 0f);
            _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            SFXManager.Instance?.PlayJump();
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 0.6f, _groundLayer);
    }

    public void Eliminate()
    {
        if (!_isAlive) return;
        _isAlive = false;
        CameraShake.Instance?.Shake();
        OnPlayerEliminated?.Invoke();
        StartCoroutine(DeathAnimation());
    }

    private IEnumerator DeathAnimation()
    {
        float elapsed = 0f;
        float duration = 0.5f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.localScale = startScale * Mathf.Lerp(1f, 0f, t);
            transform.Rotate(0f, 720f * Time.deltaTime, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        gameObject.SetActive(false);
        transform.localScale = startScale;
    }

    public void ApplyLaunch(float force)
    {
        if (!_isAlive) return;
        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, 0f);
        _rb.AddForce(Vector3.up * force, ForceMode.Impulse);
    }

    public void TeleportTo(Vector3 pos)
    {
        if (!_isAlive) return;
        transform.position = pos;
        _rb.linearVelocity = Vector3.zero;
    }

    // Power-up methods
    public void ApplySpeedBoost(float duration, float multiplier)
    {
        _speedMultiplier = multiplier;
        _speedBoostTimer = duration;
    }

    public void ActivateShield()
    {
        _hasShield = true;
    }

    public bool ConsumeShield()
    {
        if (!_hasShield) return false;
        _hasShield = false;
        return true;
    }
}
