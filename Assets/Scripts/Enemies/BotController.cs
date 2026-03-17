using UnityEngine;
using System.Collections;

public class BotController : MonoBehaviour
{
    public enum Personality { Aggressive, Cautious, Tricky }

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 4.5f;
    [SerializeField] private float _jumpForce = 5.5f;
    [SerializeField] private LayerMask _groundLayer;

    [Header("AI")]
    [SerializeField] private float _decisionInterval = 0.3f;

    private Rigidbody _rb;
    private bool _isAlive = true;
    private float _decisionTimer;
    private float _moveDirection;
    private Personality _personality;
    private float _wallStuckTimer;
    private bool _hasShield;

    // Power-up state
    private float _speedMultiplier = 1f;
    private float _speedBoostTimer;
    private float _freezeTimer;

    public bool IsAlive => _isAlive;
    public bool HasShield => _hasShield;
    public string BotName { get; set; }

    void Start()
    {
        _rb = GetComponent<Rigidbody>();

        _rb.useGravity = true;
        _rb.freezeRotation = true;
        _rb.constraints = RigidbodyConstraints.FreezePositionZ
                        | RigidbodyConstraints.FreezeRotationX
                        | RigidbodyConstraints.FreezeRotationY;

        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = false;

        _decisionTimer = Random.Range(0f, _decisionInterval);

        // Random personality
        _personality = (Personality)Random.Range(0, 3);

        // Personality affects base stats
        switch (_personality)
        {
            case Personality.Aggressive:
                _moveSpeed *= 1.2f;
                _decisionInterval = 0.2f;
                break;
            case Personality.Cautious:
                _moveSpeed *= 0.9f;
                _decisionInterval = 0.4f;
                break;
            case Personality.Tricky:
                _decisionInterval = 0.25f;
                break;
        }
    }

    void Update()
    {
        if (!_isAlive) return;

        // Power-up timers
        if (_speedBoostTimer > 0f)
        {
            _speedBoostTimer -= Time.deltaTime;
            if (_speedBoostTimer <= 0f) _speedMultiplier = 1f;
        }

        if (_freezeTimer > 0f)
        {
            _freezeTimer -= Time.deltaTime;
            return; // frozen, can't act
        }

        _decisionTimer -= Time.deltaTime;
        if (_decisionTimer <= 0f)
        {
            _decisionTimer = _decisionInterval;
            MakeDecision();
        }
    }

    void FixedUpdate()
    {
        if (!_isAlive || _freezeTimer > 0f) return;
        _rb.linearVelocity = new Vector3(_moveDirection * _moveSpeed * _speedMultiplier, _rb.linearVelocity.y, 0f);
    }

    private void MakeDecision()
    {
        if (BombController.Instance == null) return;

        GameObject bombHolder = BombController.Instance.CurrentHolder;
        if (bombHolder == null) return;

        bool iHoldBomb = bombHolder == gameObject;

        if (iHoldBomb)
        {
            DecideWithBomb();
        }
        else
        {
            DecideWithoutBomb(bombHolder);
        }

        // Wall avoidance
        float edge = (ArenaSetup.Instance?.ArenaWidth ?? 16f) * 0.45f;
        if (Mathf.Abs(transform.position.x) > edge)
        {
            _wallStuckTimer += _decisionInterval;
            if (_wallStuckTimer > 0.5f)
            {
                _moveDirection = -_moveDirection;
                if (IsGrounded()) Jump();
                _wallStuckTimer = 0f;
            }
        }
        else
        {
            _wallStuckTimer = 0f;
        }
    }

    private void DecideWithBomb()
    {
        GameObject nearest = FindNearestAlive();
        if (nearest == null) return;

        switch (_personality)
        {
            case Personality.Aggressive:
                // Beeline to nearest, jump aggressively
                _moveDirection = nearest.transform.position.x > transform.position.x ? 1f : -1f;
                if (IsGrounded() && (nearest.transform.position.y > transform.position.y + 0.5f || Random.value < 0.4f))
                    Jump();
                break;

            case Personality.Cautious:
                // Move toward nearest but hesitate
                _moveDirection = nearest.transform.position.x > transform.position.x ? 1f : -1f;
                if (Random.value < 0.15f) _moveDirection = -_moveDirection; // hesitate
                if (nearest.transform.position.y > transform.position.y + 1f && IsGrounded())
                    Jump();
                break;

            case Personality.Tricky:
                // Fake out: move away then suddenly reverse
                if (Random.value < 0.4f)
                    _moveDirection = nearest.transform.position.x > transform.position.x ? -1f : 1f; // fake
                else
                    _moveDirection = nearest.transform.position.x > transform.position.x ? 1f : -1f;
                if (IsGrounded() && Random.value < 0.35f) Jump();
                break;
        }

        // Seek jump pads when holding bomb
        SeekJumpPad();
    }

    private void DecideWithoutBomb(GameObject bombHolder)
    {
        float distToBomb = Vector3.Distance(transform.position, bombHolder.transform.position);

        switch (_personality)
        {
            case Personality.Aggressive:
                // Stay medium distance, ready to dodge
                if (distToBomb < 2f)
                    _moveDirection = bombHolder.transform.position.x > transform.position.x ? -1f : 1f;
                else if (distToBomb > 5f)
                    _moveDirection = Random.value < 0.5f ? 1f : -1f;
                else
                    _moveDirection = bombHolder.transform.position.x > transform.position.x ? -1f : 1f;
                if (IsGrounded() && Random.value < 0.25f) Jump();
                break;

            case Personality.Cautious:
                // Maximum distance from bomb holder
                _moveDirection = bombHolder.transform.position.x > transform.position.x ? -1f : 1f;
                if (IsGrounded() && Random.value < 0.15f) Jump();
                break;

            case Personality.Tricky:
                // Unpredictable movement
                if (Random.value < 0.3f)
                    _moveDirection = -_moveDirection; // random reversal
                else
                    _moveDirection = bombHolder.transform.position.x > transform.position.x ? -1f : 1f;
                if (IsGrounded() && Random.value < 0.3f) Jump();
                break;
        }
    }

    private void SeekJumpPad()
    {
        // If holding bomb, look for nearby jump pads to gain height advantage
        var jumpPads = Object.FindObjectsByType<JumpPad>(FindObjectsSortMode.None);
        float closestDist = 3f; // only seek if within range
        JumpPad closest = null;

        foreach (var pad in jumpPads)
        {
            float d = Vector3.Distance(transform.position, pad.transform.position);
            if (d < closestDist)
            {
                closestDist = d;
                closest = pad;
            }
        }

        if (closest != null)
            _moveDirection = closest.transform.position.x > transform.position.x ? 1f : -1f;
    }

    private GameObject FindNearestAlive()
    {
        if (RoundManager.Instance == null) return null;

        GameObject nearest = null;
        float minDist = float.MaxValue;

        foreach (var character in RoundManager.Instance.AliveCharacters)
        {
            if (character == null || character == gameObject) continue;
            float dist = Vector3.Distance(transform.position, character.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = character;
            }
        }
        return nearest;
    }

    private void Jump()
    {
        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, 0f);
        _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 0.6f, _groundLayer);
    }

    public void Eliminate()
    {
        if (!_isAlive) return;
        _isAlive = false;
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

    public void ApplyFreeze(float duration)
    {
        _freezeTimer = duration;
    }
}
