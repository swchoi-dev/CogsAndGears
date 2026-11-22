using UnityEngine;

// Drone 모델(Animator가 있는 오브젝트)에 붙어 Animation Event를 DroneController로 전달한다.
public class DroneAnimationEvents : MonoBehaviour
{
    private DroneController _drone;

    private void Awake()
    {
        _drone = GetComponentInParent<DroneController>();
    }

    public void OnFire()
    {
        _drone.Fire();
    }
}
