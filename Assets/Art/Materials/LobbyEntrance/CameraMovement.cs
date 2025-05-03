using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float fastMoveSpeed = 10f;
    public float rotationSpeed = 60f;

    [Header("Axis Controls")]
    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backwardKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode upKey = KeyCode.E;
    public KeyCode downKey = KeyCode.Q;

    private Transform cameraTransform;
    private bool isFastMode = false;

    void Start()
    {
        cameraTransform = transform;
    }

    void Update()
    {
        // Check for sprint mode
        isFastMode = Input.GetKey(KeyCode.LeftShift);
        float currentMoveSpeed = isFastMode ? fastMoveSpeed : moveSpeed;

        // Handle movement input
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(forwardKey))
            moveDirection += cameraTransform.forward;
        
        if (Input.GetKey(backwardKey))
            moveDirection -= cameraTransform.forward;
        
        if (Input.GetKey(rightKey))
            moveDirection += cameraTransform.right;
        
        if (Input.GetKey(leftKey))
            moveDirection -= cameraTransform.right;
        
        if (Input.GetKey(upKey))
            moveDirection += Vector3.up;
        
        if (Input.GetKey(downKey))
            moveDirection += Vector3.down;

        // Apply movement
        if (moveDirection != Vector3.zero)
        {
            transform.position += moveDirection.normalized * currentMoveSpeed * Time.deltaTime;
        }

        // Handle rotation with mouse
        if (Input.GetMouseButton(1)) // Right mouse button
        {
            float horizontalRotation = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            float verticalRotation = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

            transform.Rotate(Vector3.up, horizontalRotation, Space.World);
            transform.Rotate(Vector3.left, verticalRotation, Space.Self);
        }
    }
}