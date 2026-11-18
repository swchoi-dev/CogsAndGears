using System;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform _target;

    private Camera _camera;
    private BoxCollider2D _cameraBounds;

    public event Action UpdatedBounds;
    
    private void Awake() => CacheComponents();

    private void LateUpdate()
    {
        UpdateCameraPosition();
    }
    
    public void SetCameraBounds(BoxCollider2D cameraBounds)
    {
        _cameraBounds = cameraBounds;
        UpdateCameraPosition();
        
        Debug.Log($"Camera Bounds Updated: {transform.position.x}");
        UpdatedBounds?.Invoke();
    }

    private void UpdateCameraPosition()
    {
        if (_camera == null || _cameraBounds == null) return;

        Bounds bounds = _cameraBounds.bounds;

        float halfWidth = _camera.orthographicSize * _camera.aspect;

        float minX = bounds.min.x + halfWidth;
        float maxX = bounds.max.x - halfWidth;

        float targetX = minX <= maxX
            ? Mathf.Clamp(_target.position.x, minX, maxX)
            : bounds.center.x;

        Vector3 position = transform.position;
        position.x = targetX;
        position.y = bounds.center.y;
        transform.position = position;
    }
    
    private void CacheComponents()
    {
        _camera = GetComponentInChildren<Camera>();
    }
}
