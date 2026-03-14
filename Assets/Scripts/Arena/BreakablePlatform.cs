using UnityEngine;

public class BreakablePlatform : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _breakDelay = 4f;

    private float _standTimer;
    private bool _playerOnTop;
    private bool _isBroken;

    void Update()
    {
        if (_isBroken) return;

        if (_playerOnTop)
        {
            _standTimer += Time.deltaTime;
            if (_standTimer >= _breakDelay)
            {
                Break();
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (_isBroken) return;

        // Only count if something is standing ON TOP (contact normal points upward)
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y < -0.5f)
            {
                var player = collision.gameObject.GetComponent<PlayerController>();
                var bot = collision.gameObject.GetComponent<BotController>();
                if (player != null || bot != null)
                {
                    _playerOnTop = true;
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
            _playerOnTop = false;
            _standTimer = 0f;
        }
    }

    private void Break()
    {
        _isBroken = true;
        Destroy(gameObject);
    }
}
