using UnityEngine;

public static class FacingUtility
{
    public static void RotateTowardsX(
        Transform modelRoot,
        float directionX,
        Quaternion rightRotation,
        Quaternion leftRotation,
        float turnSpeed,
        bool localSpace = true)
    {
        if (modelRoot == null || Mathf.Abs(directionX) < 0.01f)
            return;

        Quaternion targetRotation =
            directionX > 0f ? rightRotation : leftRotation;

        if (localSpace)
        {
            modelRoot.localRotation = Quaternion.RotateTowards(
                modelRoot.localRotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
        }
        else
        {
            modelRoot.rotation = Quaternion.RotateTowards(
                modelRoot.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
        }
    }
}