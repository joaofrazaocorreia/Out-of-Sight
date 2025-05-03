using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    public enum Type {Civillian, Worker, Guard, Police, Camera};
    public enum Status {Normal, Curious, Suspectful, Fleeing, Searching, Chasing, KnockedOut};

    [SerializeField] private PlayAudio alarmAudioPlayer;
    [SerializeField] [Min(1)] protected float alarmedTime = 5f;
    [SerializeField] protected bool ignoresAlarm = false; // If true, this enemy won't change behaviour during alarms.
    [SerializeField] protected Type type;
    [SerializeField] public UnityEvent onKnockOut;
    [SerializeField] protected PlayAudio knockoutPlayer;

    public Type EnemyType {get=> type;}
    protected Status status;
    public Status EnemyStatus
    {
        get => status;
        set
        {
            if(status != Status.KnockedOut && value == Status.KnockedOut) knockoutPlayer.Play();
            if(status != Status.KnockedOut) status = value;
        }
    }
    public bool IsConscious {get => EnemyStatus != Status.KnockedOut;}
    protected UIManager uiManager;
    protected Alarm alarm;
    protected Detection detection;
    public Detection Detection {get => detection;}
    protected EnemyMovement enemyMovement;
    public EnemyMovement EnemyMovement {get=> enemyMovement;}
    protected EnemyItemInventory enemyItemInventory;
    public EnemyItemInventory EnemyItemInventory {get=> enemyItemInventory;}
    protected Player player;
    protected float alarmedTimer;
    public float AlarmedTimer {get => alarmedTimer; set => alarmedTimer = value;}
    public bool IgnoresAlarm {get => ignoresAlarm; set => ignoresAlarm = value;}
    public bool IsAlarmed {get => alarmedTimer > 0;}
    public bool IsKnockedOut {get => status == Status.KnockedOut;}


    protected virtual void Start()
    {
        alarm = FindAnyObjectByType<Alarm>();
        uiManager = FindAnyObjectByType<UIManager>();
        detection = GetComponent<Detection>();
        enemyMovement = GetComponent<EnemyMovement>();
        enemyItemInventory = GetComponent<EnemyItemInventory>();
        player = FindAnyObjectByType<Player>();

        alarmedTimer = 0f;

        if(detection == null)
            detection= GetComponentInChildren<Detection>();

        if(enemyMovement == null)
            enemyMovement= GetComponentInChildren<EnemyMovement>();
    }

    protected virtual void Update()
    {
        if(enemyMovement.LeavingMap && IsConscious)
            EnemyMovement.ExitMap();
        
        else
        {
            switch(EnemyStatus)
            {
                case Status.Normal:
                    NormalBehavior();
                    break;

                case Status.Curious:
                    CuriousBehavior();
                    break;

                case Status.Suspectful:
                    SuspectfulBehavior();
                    break;

                case Status.Fleeing:
                    FleeingBehavior();
                    break;

                case Status.Searching:
                    SearchingBehavior();
                    break;

                case Status.Chasing:
                    ChasingBehavior();
                    break;

                case Status.KnockedOut:
                    KnockedOutBehavior();
                    break;
            }
        }
    }

    /// <summary>
    /// Alarms this enemy, initiating its alarmed behaviour
    /// </summary>
    public virtual void BecomeAlarmed()
    {
        if(detection == null)
            Start();

        if(IsConscious && !IsAlarmed)
        {
            Debug.Log($"{name} was alarmed!");
            if(alarmAudioPlayer != null) alarmAudioPlayer.Play();
            alarmedTimer = alarmedTime;
            detection.DetectionMeter = detection.DetectionLimit;
        }
    }

    /// <summary>
    /// Resets this enemy's variables
    /// (this is used when respawning enemies at the entrance)
    /// </summary>
    public void ResetNPC()
    {
        Start();
    }

    /// <summary>
    /// How this NPC behaves while its status is Normal.
    /// </summary>
    protected virtual void NormalBehavior()
    {
        /*
        // Calculates the direction towards the current most suspicious object
        Vector3 suspicionLookPos = new Vector3(Detection.suspiciousObjPos.x,
        transform.position.y, Detection.suspiciousObjPos.z);
        */

        // At 2 thirds of detection, moves towards the suspicious object to see it better
        if(detection.DetectionMeter >= detection.DetectionLimit * 2 / 3 &&
            EnemyStatus == Status.Normal)
        {
            enemyMovement.Halted = true;
            //enemyMovement.Halted = false;
            //transform.LookAt(suspicionLookPos);
            //MoveTo(Detection.lastPlayerPos);
        }

        // At 1 third of detection, stops in place and looks at the suspicious object
        else if(detection.DetectionMeter >= detection.DetectionLimit * 1 / 3 &&
            EnemyStatus == Status.Normal)
        {
            enemyMovement.Halted = true;
            //transform.LookAt(suspicionLookPos);
        }

        else
            enemyMovement.Halted = false;
        
        enemyMovement.SetMovementSpeed(enemyMovement.WalkSpeed);
        enemyMovement.Patrol();
    }

    /// <summary>
    /// How this NPC behaves while its status is Curious.
    /// </summary>
    protected virtual void CuriousBehavior()
    {

    }

    /// <summary>
    /// How this NPC behaves while its status Suspectful.
    /// </summary>
    protected virtual void SuspectfulBehavior()
    {
        
    }

    /// <summary>
    /// How this NPC behaves while its status is Fleeing.
    /// </summary>
    protected virtual void FleeingBehavior()
    {
        enemyMovement.SetMovementSpeed(enemyMovement.RunSpeed);
        
        // Begins leaving the map during an alarm
        if(alarm.IsOn)
            enemyMovement.ExitMap();
        
        // When alarm is off, attempts to raise it by either escaping or
        // pressing a panic button, whichever is closer
        else
        {
            Transform chosenNearestExit = enemyMovement.CheckNearestExit();
            Transform chosenNearestButton = enemyMovement.CheckNearestPanicButton();

            float distanceToButton = (chosenNearestButton.position - transform.position).magnitude;
            float distanceToExit = (chosenNearestExit.position - transform.position).magnitude;

            if(distanceToButton <= distanceToExit)
            {
                enemyMovement.MoveTo(chosenNearestButton.position);
                enemyMovement.RotateTo(chosenNearestButton.eulerAngles.y);
            }
            
            else
            {
                enemyMovement.MoveTo(chosenNearestExit.position);
                enemyMovement.RotateTo(chosenNearestExit.eulerAngles.y);
            }
        }
    }

    /// <summary>
    /// How this NPC behaves while its status is Searching.
    /// </summary>
    protected virtual void SearchingBehavior()
    {
        enemyMovement.SetMovementSpeed(enemyMovement.WalkSpeed);

        // Makes this enemy wander around the area to look for the player
        enemyMovement.Search();
    }

    /// <summary>
    /// How this NPC behaves while its status is Chasing.
    /// </summary>
    protected virtual void ChasingBehavior()
    {
        enemyMovement.SetMovementSpeed(enemyMovement.RunSpeed);

        // Police always know where the player is
        if(type == Type.Police)
            enemyMovement.MoveTo(player.transform.position);

        // Others will check on the last place the player was seen
        else
        {
            enemyMovement.MoveTo(Detection.lastPlayerPos);
            enemyMovement.RotateTo(null);
        }

        // Checks if the enemy is stuck somewhere and makes it start searching after a while
        if(enemyMovement.StuckCheck())
        {
            enemyMovement.MoveTo(transform.position);
            enemyMovement.RotateTo(null);
            EnemyStatus = Status.Searching;
        }

        enemyMovement.LookAround(aggro:true);
    }

    /// <summary>
    /// How this NPC behaves while its status is Knocked Out.
    /// </summary>
    protected virtual void KnockedOutBehavior()
    {
        if(enemyMovement.LeavingMap)
            enemyMovement.LeavingMap = false;

        EnemyMovement.EnableBody();
        EnemyMovement.GetKnockedOut();

        // Drops all items this enemy is carrying
        enemyItemInventory.DropAllItems();
    }

    /// <summary>
    /// Causes this enemy to be knocked out and stop moving.
    /// </summary>
    public void GetKnockedOut()
    {
        if(EnemyStatus != Status.KnockedOut)
        {
            EnemyStatus = Status.KnockedOut;
            detection.DetectionMeter = 0;
            foreach(Collider c in GetComponentsInChildren<Collider>())
            {
                c.isTrigger = true;
            }
        }
    }
}
