using UnityEngine;

public class DynamicCamera : MonoBehaviour
{
    private Camera _cam;
    private float _originalSize;
    private float _targetSize;
    private int _aliveCount = 6;

    void Start()
    {
        _cam = GetComponent<Camera>();
        if (_cam != null && _cam.orthographic)
            _originalSize = _cam.orthographicSize;
        else if (_cam != null)
            _originalSize = _cam.fieldOfView;

        _targetSize = _originalSize;

        RoundManager.OnAliveCountChanged += HandleAliveCountChanged;
    }

    void OnDestroy()
    {
        RoundManager.OnAliveCountChanged -= HandleAliveCountChanged;
    }

    void Update()
    {
        if (_cam == null) return;

        // Smoothly lerp toward target
        if (_cam.orthographic)
            _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, _targetSize, Time.deltaTime * 2f);
        else
            _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, _targetSize, Time.deltaTime * 2f);
    }

    private void HandleAliveCountChanged(int count)
    {
        _aliveCount = count;

        // Zoom in as fewer players remain
        if (_aliveCount <= 2)
            _targetSize = _originalSize * 0.75f;
        else if (_aliveCount <= 3)
            _targetSize = _originalSize * 0.85f;
        else
            _targetSize = _originalSize;
    }
}
