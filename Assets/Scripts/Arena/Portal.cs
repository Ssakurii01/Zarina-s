using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    [SerializeField] private float _lifetime = 15f;
    [SerializeField] private float _cooldown = 1f;
    [SerializeField] private float _xMin = -7f;
    [SerializeField] private float _xMax = 7f;
    [SerializeField] private float _yMin = -6f;
    [SerializeField] private float _yMax = 6f;

    private float _cooldownTimer;

    void Start()
    {
        // Use actual arena bounds instead of hardcoded values
        if (ArenaSetup.Instance != null)
        {
            _xMin = -ArenaSetup.Instance.ArenaWidth * 0.35f;
            _xMax = ArenaSetup.Instance.ArenaWidth * 0.35f;
            _yMin = -ArenaSetup.Instance.ArenaHeight * 0.3f;
            _yMax = ArenaSetup.Instance.ArenaHeight * 0.3f;
        }

        Destroy(gameObject, _lifetime);
    }

    void Update()
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_cooldownTimer > 0f) return;

        SFXManager.Instance?.PlayPortalTeleport();

        Vector3 randomPos = new Vector3(
            Random.Range(_xMin, _xMax),
            Random.Range(_yMin, _yMax),
            0f
        );

        var player = other.GetComponent<PlayerController>();
        if (player != null && player.IsAlive)
        {
            player.TeleportTo(randomPos);
            _cooldownTimer = _cooldown;
            return;
        }

        var bot = other.GetComponent<BotController>();
        if (bot != null && bot.IsAlive)
        {
            bot.TeleportTo(randomPos);
            _cooldownTimer = _cooldown;
        }
    }
}
