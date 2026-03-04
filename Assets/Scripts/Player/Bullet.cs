using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private int _damage = 1;
    [SerializeField] private GameObject _hitEffectPrefab;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyBase enemy = other.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(_damage);
            }

            // Spawn hit effect
            if (_hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(_hitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }

            Destroy(gameObject);
        }

        // Destroy bullet when hitting arena walls
        else if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
