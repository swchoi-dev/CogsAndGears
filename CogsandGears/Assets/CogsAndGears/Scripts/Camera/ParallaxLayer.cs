using UnityEngine;

[DefaultExecutionOrder(200)]
public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private Transform _targetCamera;
    [SerializeField, Range(0f, 1f)] private float _parallaxFactor = 0.5f;

    private Vector3 _initPosition;
    private float _initCameraX;
    
    private void Start() => InitPosition();

    private void LateUpdate()
    {
        float cameraDeltaX = _targetCamera.position.x - _initCameraX;
        
        float offsetX = cameraDeltaX * _parallaxFactor;

        transform.position = new Vector3(
            _initPosition.x + offsetX,
            _initPosition.y,
            _initPosition.z
        );
    }
    
    private void InitPosition()
    {
        _initPosition = transform.position;
        _initCameraX = _targetCamera.position.x;
    }
    
}
