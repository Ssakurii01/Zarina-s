using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 8f;

    [Header("Jumping")]
    [SerializeField] private float _jumpForce = 5.5f;
    [SerializeField] private LayerMask _groundLayer;

    private Rigidbody _rb;
    private bool _isAlive = true;

    public bool IsAlive => _isAlive;

    public static event Action OnPlayerEliminated;

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

        if (InputManager.Instance?.Input != null)
            InputManager.Instance.Input.OnJumpPressed += HandleJump;
    }

    void OnDestroy()
    {
        if (InputManager.Instance?.Input != null)
            InputManager.Instance.Input.OnJumpPressed -= HandleJump;
    }

    void FixedUpdate()
    {
        if (!_isAlive) return;
        HandleMovement();
    }

    private void HandleMovement()
    {
        float horizontal = InputManager.Instance?.Input?.MoveHorizontal ?? 0f;
        _rb.linearVelocity = new Vector3(horizontal * _moveSpeed, _rb.linearVelocity.y, 0f);
    }

    private void HandleJump()
    {
        if (!_isAlive) return;
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

    public void Eliminate()
    {
        if (!_isAlive) return;
        _isAlive = false;
        CameraShake.Instance?.Shake();
        OnPlayerEliminated?.Invoke();
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
