using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [SerializeField] private float _defaultDuration = 0.15f;
    [SerializeField] private float _defaultMagnitude = 0.3f;

    private Vector3 _originalPos;
    private Coroutine _currentShake;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        _originalPos = transform.localPosition;
    }

    public void Shake(float duration = -1f, float magnitude = -1f)
    {
        if (duration < 0) duration = _defaultDuration;
        if (magnitude < 0) magnitude = _defaultMagnitude;

        if (_currentShake != null)
        {
            StopCoroutine(_currentShake);
            transform.localPosition = _originalPos;
        }
        _currentShake = StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = _originalPos + new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = _originalPos;
        _currentShake = null;
    }
}
