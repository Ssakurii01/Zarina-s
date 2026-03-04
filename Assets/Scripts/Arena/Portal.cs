using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    [SerializeField] private GameObject _portalToClone; // If null, clones itself
    [SerializeField] private float _xMinLimit = -10.05f;
    [SerializeField] private float _xMaxLimit = 10.05f; // Symmetry as default
    [SerializeField] private float _ySpawnHeight = 1f;

    private static int _cloneCount = 0;
    private const int MAX_CLONES = 2;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _cloneCount = 0;
    }

    void Start()
    {
        // Only the "Master" portal (the one in the scene) should spawn clones
        // We check if it's a clone by looking at the static count or a naming convention
        if (_cloneCount < MAX_CLONES && !name.Contains("(Clone)"))
        {
            SpawnClones();
        }
    }

    private void SpawnClones()
    {
        for (int i = 0; i < MAX_CLONES; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            GameObject clone = Instantiate(_portalToClone != null ? _portalToClone : gameObject, spawnPos, Quaternion.identity);
            
            // Mark it so it doesn't try to spawn clones itself
            clone.name = name + " (Clone) " + (i + 1);
            _cloneCount++;
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        // Ensures it stays within the camera -10.05 limit as requested
        float randomX = Random.Range(_xMinLimit, _xMaxLimit);
        // Assuming side-scroller on XY plane based on previous PlayerController edits
        return new Vector3(randomX, _ySpawnHeight, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered portal!");
            // Implement teleportation logic here if needed
        }
    }
}
