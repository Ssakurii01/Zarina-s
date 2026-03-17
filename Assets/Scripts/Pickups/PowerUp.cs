using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum Type { SpeedBoost, Shield, Freeze }

    [SerializeField] private Type _type = Type.SpeedBoost;
    [SerializeField] private float _lifetime = 10f;
    [SerializeField] private float _speedBoostDuration = 5f;
    [SerializeField] private float _speedBoostMultiplier = 2f;
    [SerializeField] private float _freezeDuration = 3f;

    private static readonly Color[] TypeColors = {
        new Color(0f, 0.8f, 1f),   // SpeedBoost: cyan
        new Color(0.3f, 0.7f, 1f), // Shield: blue
        new Color(0.6f, 0f, 1f)    // Freeze: purple
    };

    void Start()
    {
        // Set visual color based on type
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
            renderer.material.color = TypeColors[(int)_type];

        // Make it a trigger
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        // Auto-destroy after lifetime
        Destroy(gameObject, _lifetime);
    }

    void Update()
    {
        // Gentle bob and spin
        transform.Rotate(0f, 90f * Time.deltaTime, 0f);
        transform.position += new Vector3(0f, Mathf.Sin(Time.time * 3f) * 0.5f * Time.deltaTime, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player != null && player.IsAlive)
        {
            ApplyToPlayer(player);
            SFXManager.Instance?.PlayPickup();
            Destroy(gameObject);
            return;
        }

        var bot = other.GetComponent<BotController>();
        if (bot != null && bot.IsAlive)
        {
            ApplyToBot(bot);
            SFXManager.Instance?.PlayPickup();
            Destroy(gameObject);
        }
    }

    private void ApplyToPlayer(PlayerController player)
    {
        switch (_type)
        {
            case Type.SpeedBoost:
                player.ApplySpeedBoost(_speedBoostDuration, _speedBoostMultiplier);
                break;
            case Type.Shield:
                player.ActivateShield();
                break;
            case Type.Freeze:
                // Freeze the bomb holder (if it's not the player)
                FreezeCurrentBombHolder(player.gameObject);
                break;
        }
    }

    private void ApplyToBot(BotController bot)
    {
        switch (_type)
        {
            case Type.SpeedBoost:
                bot.ApplySpeedBoost(_speedBoostDuration, _speedBoostMultiplier);
                break;
            case Type.Shield:
                bot.ActivateShield();
                break;
            case Type.Freeze:
                FreezeCurrentBombHolder(bot.gameObject);
                break;
        }
    }

    private void FreezeCurrentBombHolder(GameObject picker)
    {
        if (BombController.Instance == null) return;
        var holder = BombController.Instance.CurrentHolder;
        if (holder == null || holder == picker) return;

        var holderBot = holder.GetComponent<BotController>();
        if (holderBot != null) holderBot.ApplyFreeze(_freezeDuration);

        // If player is holder, we don't freeze them (would be unfun)
    }

    public void SetType(Type type)
    {
        _type = type;
    }
}
