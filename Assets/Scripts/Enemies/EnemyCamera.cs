using Interaction;
using UnityEngine;

public class EnemyCamera : Enemy, IJammable
{
    [Header("Camera Variables")]
    [SerializeField] private Enemy cameraOperator;
    [SerializeField] private Transform cameraBody;
    [SerializeField] private PlayAudio cameraSFXPlayer;
    [SerializeField] [Range(-90, 90)] private int maxRotationAngle = 45;
    [SerializeField] [Range(-90, 90)] private int minRotationAngle = -45;
    [SerializeField] private float rotationSpeed = 25f;
    [SerializeField] private float rotationIdleTime = 6f;
    [SerializeField] private bool invertedRotation = false;

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

        canRotate = maxRotationAngle != minRotationAngle;
        positiveRotationDirection = !invertedRotation;
        rotationIdleTimer = 0f;
        
        int startingRotation = positiveRotationDirection ? maxRotationAngle : minRotationAngle;
        cameraBody.localRotation = Quaternion.RotateTowards(cameraBody.localRotation,
            Quaternion.Euler(0f, startingRotation, 0f), 180f);

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
            else if ((!alarm.IsOn && detection.DetectionMeter >= detection.DetectionLimit) ||
                (detection.SeesPlayer && alarm.IsOn))
            {
                BecomeAlarmed();
            }


            // Checks if the camera rotates and rotates it
            if (canRotate)
            {
                float newRotationY;

                // If the camera's detection is above one third, it follows the player
                if (detection.SeesPlayer && detection.DetectionMeter > detection.DetectionLimit * 1 / 3)
                {
                    Quaternion targetRotation = cameraBody.rotation;
                    targetRotation = Quaternion.RotateTowards(targetRotation,
                        Quaternion.LookRotation(Detection.lastPlayerPos - cameraBody.position), 1f);

                    targetRotation = new Quaternion(0f, targetRotation.y, 0f, targetRotation.w);

                    cameraBody.rotation = targetRotation;
                }

                // Checks if the camera is ready to rotate
                else if (rotationIdleTimer <= 0)
                {
                    newRotationY = positiveRotationDirection ? cameraBody.localEulerAngles.y +
                        Time.deltaTime * rotationSpeed : cameraBody.localEulerAngles.y - (Time.deltaTime * rotationSpeed);

                    if (newRotationY >= 180f)
                        newRotationY -= 360f;

                    newRotationY = Mathf.Clamp(newRotationY, minRotationAngle, maxRotationAngle);

                    if ((positiveRotationDirection && newRotationY >= maxRotationAngle) ||
                        (!positiveRotationDirection && newRotationY <= minRotationAngle))
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

                    if (rotationIdleTimer <= 0)
                    {
                        cameraSFXPlayer.Play();
                        cameraSFXPlayer.StretchPitch((maxRotationAngle - minRotationAngle) / rotationSpeed);
                    }
                }
            }

            /*
            Vector3 distanceToPlayer = player.transform.position - detection.transform.position;
            if ((Physics.Raycast(transform.position, distanceToPlayer, out RaycastHit hit,
                 cameraSFXPlayer.AudioSource.maxDistance, LayerMask.GetMask("Default", "Player")) &&
                 hit.transform.CompareTag("Player")) || distanceToPlayer.magnitude < cameraSFXPlayer.AudioSource.maxDistance/2)
            {
                cameraSFXPlayer.AudioSource.volume = 1f;
                Debug.DrawRay(detection.transform.position, distanceToPlayer, Color.red);
            }

            else
            {
                cameraSFXPlayer.AudioSource.volume = 0.25f;
                Debug.DrawRay(detection.transform.position, distanceToPlayer, Color.yellow);
            }
            */
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

    public override void BecomeNormal(bool ignoreAlarm = false)
    {
        if (!jammed && (!IsAlarmed || ignoreAlarm) && !alarm.IsOn)
            detection.DetectionMeter = 0f;
    }

    /// <summary>
    /// Tracks the player's position and alarms the camera operator.
    /// </summary>
    public override void BecomeAlarmed()
    {
        if (detection.SeesPlayer)
            detection.TrackPlayer();

        if(!cameraOperator.IsAlarmed)
            cameraOperator.BecomeAlarmed();

        if(alarm.IsOn)
            alarm.TriggerAlarm(false);
    }

    /// <summary>
    /// Toggles this camera's jammed state.
    /// </summary>
    public void ToggleJammed()
    {
        jammed = !jammed;
    }
}
