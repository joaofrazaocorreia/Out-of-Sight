using UnityEngine;

public class UIFaceCamera : MonoBehaviour
{
    private static Transform activePlayerCamera;

    private void Start()
    {
        if(activePlayerCamera == null)
            UpdateActiveCamera();
    }


    private void Update()
    {
        // Makes the attached UI always face the currently active camera
        
        Vector3 toTarget = activePlayerCamera.position - transform.position;
        Vector3 rotation = Quaternion.LookRotation(toTarget).eulerAngles;

        transform.rotation = Quaternion.Euler(rotation);
    }

    /// <summary>
    /// Updates the camera to rotate towards.
    /// </summary>
    public static void UpdateActiveCamera()
    {
        activePlayerCamera = FindAnyObjectByType<Camera>().transform;
    }
}
