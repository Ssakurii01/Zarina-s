using UnityEngine;

public class BreakablePlatform : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _breakDelay = 4f;

    private float _standTimer;
    private bool _isStandingOnTop;
    private bool _isBroken;
    private Renderer _renderer;
    private Color _originalColor;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer != null)
            _originalColor = _renderer.material.color;
    }

    void Update()
    {
        if (_isBroken) return;

        if (_isStandingOnTop)
        {
            _standTimer += Time.deltaTime;

            // Visual warning: lerp color to red as timer progresses
            if (_renderer != null)
            {
                float t = Mathf.Clamp01(_standTimer / _breakDelay);
                _renderer.material.color = Color.Lerp(_originalColor, Color.red, t);
            }

            if (_standTimer >= _breakDelay)
            {
                Break();
            }
        }

        // Reset flag each frame; OnCollisionStay will set it again if still on top
        _isStandingOnTop = false;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (_isBroken) return;

        // Only count if a player/bot is standing ON TOP of this platform
        // The contact normal points from this platform toward the other object
        // When something is on top, the normal points downward (negative y from the other object's perspective)
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y < -0.7f) // Strict: must be mostly on top, not glancing
            {
                var player = collision.gameObject.GetComponent<PlayerController>();
                var bot = collision.gameObject.GetComponent<BotController>();
                if (player != null || bot != null)
                {
                    _isStandingOnTop = true;
                    return;
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        var player = collision.gameObject.GetComponent<PlayerController>();
        var bot = collision.gameObject.GetComponent<BotController>();
        if (player != null || bot != null)
        {
            _isStandingOnTop = false;
            _standTimer = 0f;

            // Reset color
            if (_renderer != null)
                _renderer.material.color = _originalColor;
        }
    }

    private void Break()
    {
        _isBroken = true;
        Destroy(gameObject);
    }
}
