using Unity.Mathematics;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private float maxRotationAngle;
    [SerializeField] private float minRotationAngle;

    public void RotateCamera(float angle)
    {
        print(angle);
        angle = math.clamp(transform.eulerAngles.x - angle, minRotationAngle, maxRotationAngle);

        angle = (angle > 0) ? angle -transform.eulerAngles.x : angle + transform.eulerAngles.x;
        
        print(angle + transform.eulerAngles.x);
        
        transform.Rotate(Vector3.right, angle); 
    }
    
}
