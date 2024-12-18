using Interaction;
using UnityEngine;

public class EnemyCamera : Enemy, IJammable
{
    [SerializeField] private EnemyGuard cameraOperator;
    [SerializeField] private Transform cameraBody;
    [SerializeField] [Range(0, 90)] private int rotationAngles = 45;
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

        if(!isOn || jammed)
        {
            detection.gameObject.SetActive(false);
            detection.DetectionMeter = 0;
        }
        
        else if(!detection.gameObject.activeSelf)
            detection.gameObject.SetActive(true);
        
        if(isOn && !jammed)
        {
           
            if(cameraOperator.IsKnockedOut || 
                (cameraOperator.transform.position - cameraOperatorStartPos).magnitude > 2f)
            {
                isOn = false;
            }

            else if (detection.DetectionMeter >= detection.DetectionLimit)
            {
                cameraOperator.BecomeAlarmed();
            }


            if(canRotate)
            {
                float newRotationY;

                if(detection.SeesPlayer && detection.DetectionMeter > detection.DetectionLimit * 1 / 3)
                {
                    Quaternion targetRotation = cameraBody.rotation;
                    targetRotation = Quaternion.RotateTowards(targetRotation,
                        Quaternion.LookRotation(Detection.lastPlayerPos - cameraBody.position), 1f);

                    targetRotation = new Quaternion(0f, targetRotation.y, 0f, targetRotation.w);

                    cameraBody.rotation = targetRotation;
                }

                else if(rotationIdleTimer <= 0)
                {
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

        else if(!isOn && !cameraOperator.IsKnockedOut && 
            (cameraOperator.transform.position - cameraOperatorStartPos).magnitude <= 2f)
        {
            isOn = true;
        }
    }

    public override void BecomeAlarmed()
    {
        detection.TrackPlayer();
        cameraOperator.BecomeAlarmed();
    }

    public void ToggleJammed()
    {
        jammed = !jammed;
    }
}
