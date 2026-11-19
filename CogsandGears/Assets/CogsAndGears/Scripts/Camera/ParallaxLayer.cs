using System;
using UnityEngine;

[DefaultExecutionOrder(200)]
public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private Transform _targetCamera;
    [SerializeField, Range(0f, 1f)] private float _parallaxFactor = 0.5f;
    [SerializeField] private CameraFollow2D _cameraFollow2D;

    private Vector3 _originPosition;
    private Vector3 _initPosition;
    private float _initCameraX;

    private void Awake()
    {
        _originPosition = transform.position;
    }
    
    private void OnEnable()
    {
        _cameraFollow2D.UpdatedBounds += InitPosition;
    }

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
    
    private void OnDisable()
    {
        _cameraFollow2D.UpdatedBounds -= InitPosition;
    }
    
    private void InitPosition()
    {
        _initCameraX = _targetCamera.position.x;
        
        _initPosition = new Vector3(
            _initCameraX + _originPosition.x,
            _originPosition.y,
            _originPosition.z
        );

        transform.position = _initPosition;
    }
    
}
