using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum Type {Civillian, Worker, Guard, Police, Camera};

    protected Type type;
    //private Alarm alarm;
    protected Detection detection;
    public Detection Detection {get => detection;}
    protected EnemyMovement enemyMovement;
    public EnemyMovement EnemyMovement {get=> enemyMovement;}
    public bool IsKnockedOut {get => enemyMovement.status == EnemyMovement.Status.KnockedOut;}


    protected virtual void Start()
    {
        detection = GetComponent<Detection>();
        enemyMovement = GetComponent<EnemyMovement>();


        if(detection == null)
            detection= GetComponentInChildren<Detection>();

        if(enemyMovement == null)
            enemyMovement= GetComponentInChildren<EnemyMovement>();
    }

    protected virtual void Update()
    {
        if(detection.DetectionMeter >= detection.DetectionLimit)
        {
            if(type != Type.Camera)
            {
                if((transform.position - Detection.lastPlayerPos).magnitude <= 3f)
                    enemyMovement.status = EnemyMovement.Status.Scared;
                else
                    enemyMovement.status = EnemyMovement.Status.Fleeing;
            }
        }
    }

    public virtual void BecomeAlarmed()
    {
        detection.DetectionMeter = detection.DetectionLimit;
        // alarm.TriggerAlarm();
    }
}
