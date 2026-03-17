using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    [SerializeField] private float _spawnInterval = 20f;
    private float _timer;

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _spawnInterval)
        {
            _timer = 0f;
            SpawnRandomPowerUp();
        }
    }

    private void SpawnRandomPowerUp()
    {
        float halfW = (ArenaSetup.Instance?.ArenaWidth ?? 14f) * 0.35f;
        float halfH = (ArenaSetup.Instance?.ArenaHeight ?? 16f) * 0.3f;

        Vector3 pos = new Vector3(
            Random.Range(-halfW, halfW),
            Random.Range(-halfH * 0.5f, halfH * 0.5f),
            0f
        );

        // Create a small sphere as the pickup
        GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pickup.name = "PowerUp";
        pickup.transform.position = pos;
        pickup.transform.localScale = Vector3.one * 0.5f;

        var powerUp = pickup.AddComponent<PowerUp>();
        powerUp.SetType((PowerUp.Type)Random.Range(0, 3));
    }
}
