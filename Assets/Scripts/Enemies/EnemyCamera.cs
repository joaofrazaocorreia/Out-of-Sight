using UnityEngine;

public class EnemyCamera : Enemy
{
    [SerializeField] private EnemyGuard cameraOperator;
    [SerializeField] private Transform cameraBody;
    [SerializeField] [Range(0, 90)] private int rotationAngles = 45;
    [SerializeField] private float rotationSpeed = 25f;
    [SerializeField] private float rotationIdleTime = 6f;
    [SerializeField] private bool startsRotatingToTheLeft = false;

    private bool isOn;
    public bool IsOn {get => isOn;}
    private bool canRotate;
    private bool positiveRotationDirection;
    private float rotationIdleTimer;
    private Vector3 cameraOperatorStartPos;


    protected override void Start()
    {
        detection = GetComponent<Detection>();

        if(detection == null)
            detection= GetComponentInChildren<Detection>();

        type = Type.Camera;


        if(cameraOperator != null)
            isOn = true;
        else
            isOn = false;

        canRotate = rotationAngles != 0;
        positiveRotationDirection = !startsRotatingToTheLeft;
        rotationIdleTimer = 1f;

        cameraOperatorStartPos = cameraOperator.transform.position;
    }


    protected override void Update()
    {
        base.Update();

        if(isOn)
        {
            if(cameraOperator.IsKnockedOut || 
                (cameraOperator.transform.position - cameraOperatorStartPos).magnitude > 2f)
            {
                isOn = false;
            }

            else if(!isOn && !cameraOperator.IsKnockedOut && 
                (cameraOperator.transform.position - cameraOperatorStartPos).magnitude <= 2f)
            {
                isOn = true;
            }

            else if (detection.DetectionMeter >= detection.DetectionLimit)
            {
                cameraOperator.BecomeAlarmed();
            }


            if(canRotate && rotationIdleTimer <= 0)
            {
                float newRotationY;

                if(positiveRotationDirection)
                {
                    newRotationY = cameraBody.localEulerAngles.y + (Time.deltaTime * rotationSpeed);
                    if(newRotationY >= 180f)
                        newRotationY -= 360f;

                    newRotationY = Mathf.Clamp(newRotationY, -rotationAngles , rotationAngles);

                    if(newRotationY >= rotationAngles)
                    {
                        rotationIdleTimer = rotationIdleTime;
                        positiveRotationDirection = !positiveRotationDirection;
                    }
                }

                else
                {
                    newRotationY = cameraBody.localEulerAngles.y - (Time.deltaTime * rotationSpeed);
                    if(newRotationY >= 180f)
                        newRotationY -= 360f;

                    newRotationY = Mathf.Clamp(newRotationY, -rotationAngles , rotationAngles);

                    if(newRotationY <= -rotationAngles)
                    {
                        rotationIdleTimer = rotationIdleTime;
                        positiveRotationDirection = !positiveRotationDirection;
                    }
                }

                cameraBody.localEulerAngles = new Vector3(
                    cameraBody.rotation.eulerAngles.x, newRotationY, cameraBody.rotation.eulerAngles.z);
            }

            else
            {
                rotationIdleTimer -= Time.deltaTime;
            }
        }
    }

    public override void BecomeAlarmed()
    {
        detection.TrackPlayer();
        cameraOperator.BecomeAlarmed();
    }
}
