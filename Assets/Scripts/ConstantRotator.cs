using UnityEngine;

public class ConstantRotator : MonoBehaviour
{
    // Axis of rotation (e.g., (0,1,0) for Y-axis spin)
    public Vector3 rotationAxis = Vector3.up;

    // Speed in degrees per second
    public float rotationSpeed = 180f;

    void Update()
    {
        // Rotate around the axis in local space
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.Self);
    }
}