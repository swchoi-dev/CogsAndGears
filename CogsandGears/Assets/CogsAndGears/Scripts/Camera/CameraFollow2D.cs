using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private BoxCollider2D _cameraBounds;

    private Camera _camera;

    private void Awake() => CacheComponents();

    private void LateUpdate()
    {
        Bounds bounds = _cameraBounds.bounds;

        float halfWidth = _camera.orthographicSize * _camera.aspect;

        float minX = bounds.min.x + halfWidth;
        float maxX = bounds.max.x - halfWidth;

        float targetX = minX <= maxX
            ? Mathf.Clamp(_target.position.x, minX, maxX)
            : bounds.center.x;

        Vector3 position = transform.position;
        position.x = targetX;
        transform.position = position;
    }
    
    private void CacheComponents()
    {
        _camera = GetComponent<Camera>();
    }


}
