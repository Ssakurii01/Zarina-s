using UnityEngine;

public class BotController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 4.5f;
    [SerializeField] private float _jumpForce = 5.5f;
    [SerializeField] private LayerMask _groundLayer;

    [Header("AI")]
    [SerializeField] private float _decisionInterval = 0.3f;

    private Rigidbody _rb;
    private bool _isAlive = true;
    private float _decisionTimer;
    private float _moveDirection; // -1 or 1

    public bool IsAlive => _isAlive;
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
    }

    void Update()
    {
        if (!_isAlive) return;

        _decisionTimer -= Time.deltaTime;
        if (_decisionTimer <= 0f)
        {
            _decisionTimer = _decisionInterval;
            MakeDecision();
        }
    }

    void FixedUpdate()
    {
        if (!_isAlive) return;
        _rb.linearVelocity = new Vector3(_moveDirection * _moveSpeed, _rb.linearVelocity.y, 0f);
    }

    private void MakeDecision()
    {
        if (BombController.Instance == null) return;

        GameObject bombHolder = BombController.Instance.CurrentHolder;
        if (bombHolder == null) return;

        bool iHoldBomb = bombHolder == gameObject;

        if (iHoldBomb)
        {
            // Chase nearest alive character to pass the bomb
            GameObject nearest = FindNearestAlive();
            if (nearest != null)
            {
                _moveDirection = nearest.transform.position.x > transform.position.x ? 1f : -1f;

                // Jump if target is above
                if (nearest.transform.position.y > transform.position.y + 1f && IsGrounded())
                    Jump();
            }
        }
        else
        {
            // Flee from bomb holder
            _moveDirection = bombHolder.transform.position.x > transform.position.x ? -1f : 1f;

            // Random jumps to be less predictable
            if (IsGrounded() && Random.value < 0.2f)
                Jump();
        }

        // Jump at arena edges to avoid getting stuck
        float edge = (ArenaSetup.Instance?.ArenaWidth ?? 16f) * 0.45f;
        if (Mathf.Abs(transform.position.x) > edge && IsGrounded())
        {
            _moveDirection = -_moveDirection;
            Jump();
        }
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
        gameObject.SetActive(false);
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
}
