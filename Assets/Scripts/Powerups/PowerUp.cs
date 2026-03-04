using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum PowerUpType { Shield, SpreadShot, SpeedBoost, Nuke, HealthPack }

    [SerializeField] private PowerUpType _type;
    [SerializeField] private float _lifetime = 8f;
    [SerializeField] private float _bobSpeed = 2f;
    [SerializeField] private float _bobHeight = 0.3f;
    [SerializeField] private float _rotateSpeed = 90f;

    private Vector3 _startPos;

    void Start()
    {
        _startPos = transform.position;
        Destroy(gameObject, _lifetime);
    }

    void Update()
    {
        // Bob up and down
        float yOffset = Mathf.Sin(Time.time * _bobSpeed) * _bobHeight;
        transform.position = _startPos + Vector3.up * yOffset;

        // Rotate (Rotate around Z for 2D orientation)
        transform.Rotate(Vector3.forward, _rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        switch (_type)
        {
            case PowerUpType.Shield:
                player.ActivateShield();
                break;
            case PowerUpType.SpreadShot:
                player.ActivateSpreadShot();
                break;
            case PowerUpType.SpeedBoost:
                player.ActivateSpeedBoost();
                break;
            case PowerUpType.Nuke:
                NukeAllEnemies();
                break;
            case PowerUpType.HealthPack:
                player.Heal(1);
                break;
        }

        Destroy(gameObject);
    }

    private void NukeAllEnemies()
    {
        EnemyBase[] enemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        foreach (EnemyBase enemy in enemies)
        {
            enemy.TakeDamage(999);
        }
    }
}
