using UnityEngine;

public class EnemyCamera : Enemy
{
    [SerializeField] private EnemyGuard cameraOperator;

    private bool isOn;
    public bool IsOn {get => isOn;}


    protected override void Start()
    {
        base.Start();

        type = Type.Camera;


        if(cameraOperator != null)
            isOn = true;
        else
            isOn = false;
    }


    protected override void Update()
    {
        base.Update();

        if(isOn)
        {
            if(cameraOperator.IsKnockedOut)
                isOn = false;

            else if (detection.DetectionMeter >= detection.DetectionLimit)
            {
                cameraOperator.BecomeAlarmed();
            }
        }
    }

    public override void BecomeAlarmed()
    {
        detection.TrackPlayer();
        cameraOperator.BecomeAlarmed();
    }
}
