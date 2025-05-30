using System;
using Unity.Mathematics;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private float maxRotationAngle;
    [SerializeField] private float minRotationAngle;
    
    private Transform _rotationPivot;
    private Transform _originalRotationPivot;

    private void Start()
    {
        _rotationPivot = transform;
        _originalRotationPivot = _rotationPivot;
    }

    public void RotateCamera(float  angle)
    {
        float currentAngle = transform.localEulerAngles.x;
        currentAngle += angle;
        while (currentAngle < -180.0f) currentAngle += 360.0f; 
        while (currentAngle > 180.0f) currentAngle -= 360.0f;
        
        currentAngle = math.clamp(currentAngle, minRotationAngle, maxRotationAngle);
        
        _rotationPivot.localRotation = Quaternion.Euler(currentAngle, 0.0f, 0.0f);
    }

    public void ExtendedCameraInUse(bool isInUse, Transform newRotationPivot)
    {
        _rotationPivot = isInUse ? newRotationPivot : _originalRotationPivot;
    }

}
