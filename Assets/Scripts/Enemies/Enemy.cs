using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private PlayAudio alarmAudioPlayer;
    [SerializeField] protected float alarmedTime; 
    public enum Type {Civillian, Worker, Guard, Police, Camera};

    protected Type type;
    public Type EnemyType {get=> type;}
    protected UIManager uiManager;
    protected Alarm alarm;
    protected Detection detection;
    public Detection Detection {get => detection;}
    protected EnemyMovement enemyMovement;
    public EnemyMovement EnemyMovement {get=> enemyMovement;}
    protected Player player;
    protected float alarmedTimer;
    public float AlarmedTimer {get => alarmedTimer; set => alarmedTimer = value;}
    public bool IsAlarmed {get => alarmedTimer > 0;}
    public bool IsKnockedOut {get => enemyMovement.currentStatus == EnemyMovement.Status.KnockedOut;}
    public bool IsTased {get => enemyMovement.currentStatus == EnemyMovement.Status.Tased;}


    protected virtual void Start()
    {
        alarm = FindAnyObjectByType<Alarm>();
        uiManager = FindAnyObjectByType<UIManager>();
        detection = GetComponent<Detection>();
        enemyMovement = GetComponent<EnemyMovement>();
        player = FindAnyObjectByType<Player>();

        alarmedTimer = 0f;

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
                BecomeAlarmed();
                enemyMovement.currentStatus = EnemyMovement.Status.Fleeing;
            }
        }
    }

    public virtual void BecomeAlarmed()
    {
        if(detection == null)
            Start();

        if(!alarm.IsOn)
        {
            Debug.Log($"{name} was alarmed!");
            alarmedTimer = alarmedTime;
        }

        alarm.TriggerAlarm(!alarm.IsOn);
        if(alarmAudioPlayer != null) alarmAudioPlayer.Play();
    }

    public void ResetNPC()
    {
        Start();
    }
}
