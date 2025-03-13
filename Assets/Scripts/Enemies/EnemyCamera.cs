using Interaction;
using UnityEngine;

public class EnemyCamera : Enemy, IJammable
{
    [SerializeField] private EnemyGuard cameraOperator;
    [SerializeField] private Transform cameraBody;
    [SerializeField] [Range(0, 90)] private int rightRotationAngle = 45;
    [SerializeField] [Range(0, 90)] private int leftRotationAngle = 45;
    [SerializeField] private float rotationSpeed = 25f;
    [SerializeField] private float rotationIdleTime = 6f;
    [SerializeField] private bool startsRotatingToTheLeft = false;

    private bool isOn;
    public bool IsOn {get => isOn;}

    private bool jammed;
    public bool Jammed {get => jammed;}

    private bool canRotate;
    private bool positiveRotationDirection;
    private float rotationIdleTimer;
    private static Vector3 cameraOperatorStartPos;


    protected override void Start()
    {
        base.Start();

        if(detection == null)
            detection= GetComponentInChildren<Detection>();

        type = Type.Camera;


        if(cameraOperator != null)
            isOn = true;
        else
            isOn = false;

        canRotate = rightRotationAngle != 0 || leftRotationAngle != 0;
        positiveRotationDirection = !startsRotatingToTheLeft;
        rotationIdleTimer = 1f;

        UpdateCamOpStartPos();
    }


    protected override void Update()
    {
        // Removes all detection from this camera if it's disabled
        if(!isOn || jammed)
        {
            detection.DetectionMeter = 0;
        }
        
        // This code only runs if the camera is enabled
        if(isOn && !jammed)
        {
            // Disables the camera if the camera operator is KO or too far away
            // from its position
            if(cameraOperator.IsKnockedOut || 
                (cameraOperator.transform.position - cameraOperatorStartPos).magnitude > 2f)
            {
                isOn = false;
            }

            // Becomes alarmed at max detection and detects the player during alarms
            else if (detection.DetectionMeter >= detection.DetectionLimit || (detection.SeesPlayer && alarm.IsOn))
            {
                BecomeAlarmed();
            }


            // Checks if the camera rotates and rotates it
            if(canRotate)
            {
                float newRotationY;

                // If the camera's detection is above one third, it follows the player
                if(detection.SeesPlayer && detection.DetectionMeter > detection.DetectionLimit * 1 / 3)
                {
                    Quaternion targetRotation = cameraBody.rotation;
                    targetRotation = Quaternion.RotateTowards(targetRotation,
                        Quaternion.LookRotation(Detection.lastPlayerPos - cameraBody.position), 1f);

                    targetRotation = new Quaternion(0f, targetRotation.y, 0f, targetRotation.w);

                    cameraBody.rotation = targetRotation;
                }

                // Checks if the camera is ready to rotate
                else if(rotationIdleTimer <= 0)
                {
                    newRotationY = positiveRotationDirection ? cameraBody.localEulerAngles.y +
                        Time.deltaTime * rotationSpeed : cameraBody.localEulerAngles.y - (Time.deltaTime * rotationSpeed);

                    if(newRotationY >= 180f)
                        newRotationY -= 360f;

                    newRotationY = Mathf.Clamp(newRotationY, -leftRotationAngle , rightRotationAngle);

                    if((positiveRotationDirection && newRotationY >= rightRotationAngle) ||
                        (!positiveRotationDirection && newRotationY <= -leftRotationAngle))
                    {
                        rotationIdleTimer = rotationIdleTime;
                        positiveRotationDirection = !positiveRotationDirection;
                    }

                    // Updates the rotation
                    cameraBody.localEulerAngles = new Vector3(
                        cameraBody.rotation.eulerAngles.x, newRotationY, cameraBody.rotation.eulerAngles.z);
                }

                // If the camera is not ready to rotate, decreases the timer
                else
                {
                    rotationIdleTimer -= Time.deltaTime;
                }
            }
        }

        // Reenables the camera if the camera operator returns to its position
        else if(!isOn && !cameraOperator.IsKnockedOut && 
            (cameraOperator.transform.position - cameraOperatorStartPos).magnitude <= 2f)
        {
            isOn = true;
        }
    }

    /// <summary>
    /// Updates the position for the camera operator to use the cameras.
    /// </summary>
    /// <param name="pos">The new position.</param>
    public void UpdateCamOpStartPos(Transform pos = null)
    {
        if(pos == null)
            cameraOperatorStartPos = cameraOperator.transform.position;
        else
            cameraOperatorStartPos = pos.position;
    }

    /// <summary>
    /// Tracks the player's position and alarms the camera operator.
    /// </summary>
    public override void BecomeAlarmed()
    {
        detection.TrackPlayer();
        cameraOperator.BecomeAlarmed();
    }

    /// <summary>
    /// Toggles this camera's jammed state.
    /// </summary>
    public void ToggleJammed()
    {
        jammed = !jammed;
    }
}
