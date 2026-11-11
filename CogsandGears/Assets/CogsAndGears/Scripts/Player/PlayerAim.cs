using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    private const float MIN_AIM_DISTANCE = 0.0001f;
    
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _aimPoint;
    [SerializeField] private Transform _visualRoot;
    [SerializeField] private float _facingThreshold = 0.1f;
    
    public Vector2 AimDirection { get; private set; } = Vector2.right;
    public float FacingDirection { get; private set; } = 1f;

    private void Update()
    {
        UpdateAim();
        UpdateFacing();
    }

    private void UpdateAim()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();

        Vector2 mouseWorld = _camera.ScreenToWorldPoint(mouseScreen);

        Vector2 direction = mouseWorld - (Vector2)_aimPoint.position;

        if (direction.sqrMagnitude < MIN_AIM_DISTANCE)
            return;

        AimDirection = direction.normalized;
    }

    private void UpdateFacing()
    {
        if (Mathf.Abs(AimDirection.x) < _facingThreshold)
            return;

        FacingDirection = Mathf.Sign(AimDirection.x);

        _visualRoot.localRotation = Quaternion.Euler(
            0f,
            FacingDirection > 0f ? 90f : -90f,
            0f
        );
    }
}
