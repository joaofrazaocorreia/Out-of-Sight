using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    public enum Type {Civillian, Worker, Guard, Police, Camera, Paramedic};
    public enum Status {Normal, Curious, Suspectful, Fleeing, Searching, Chasing, KnockedOut};
    public enum Gender {Male, Female, AttackHelicopter};

    [Header("General Enemy Variables")]
    [SerializeField] protected Transform model;
    [SerializeField] protected Transform audioPlayers;
    [SerializeField] protected Transform audioSources;
    [SerializeField] protected List<GameObject> maleModels;
    [SerializeField] protected List<GameObject> femaleModels;
    [SerializeField] protected PlayAudio alarmAudioPlayer;
    [SerializeField] [Min(1)] protected float curiousTime = 1f;
    [SerializeField] [Min(1)] protected float suspectfulTime = 5f;
    [SerializeField] protected float suspectfulInspectRange = 9f;
    [SerializeField] [Min(1)] protected float alarmedTime = 5f;
    [SerializeField] protected bool ignoresAlarm = false; // If true, this enemy won't change behaviour during alarms.
    [SerializeField] protected Type type;
    [SerializeField] public UnityEvent onBecomeAlarmed;
    [SerializeField] public UnityEvent onKnockOut;
    [SerializeField] protected PlayAudio bodyFallPlayer;
    [SerializeField] protected PlayAudio knockoutPlayer;
    [SerializeField] protected PlayAudio tasedPlayer;
    [SerializeField] protected List<MovementTarget> bathroomTargetsMale;
    [SerializeField] protected List<MovementTarget> bathroomTargetsFemale;
    [SerializeField] [Min(0)] protected float startBathroomTimer = 15f;
    [SerializeField] [Min(0)] protected float minBathroomTimer = 60f;
    [SerializeField] [Min(0)] protected float maxBathroomTimer = 300f;
    public event EventHandler OnBecomeAlarmed;
    public event EventHandler OnKnockout;

    public Type EnemyType {get=> type;}
    protected Status status;
    public Status EnemyStatus
    {
        get => status;
        set
        {
            if (status != Status.KnockedOut && value == Status.KnockedOut)
            {
                bodyFallPlayer.Play();
                knockoutPlayer.Play(3.5f);
            }
            
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
    protected Player player;
    protected Dictionary<(Action, float, float), float> necessities;
    protected float curiousTimer;
    public float CuriousTimer {get => curiousTimer; set => curiousTimer = value;}
    protected float suspectfulTimer;
    public float SuspectfulTimer {get => suspectfulTimer; set => suspectfulTimer = value;}
    protected Vector3 SuspicionLookPos
    {
        get
        {
            if(detection.ClosestSuspiciousObject != null)
            {
                return new Vector3(detection.ClosestSuspiciousObject.transform.position.x,
                    transform.position.y, detection.ClosestSuspiciousObject.transform.position.z);
            }
        
            else 
                return Vector3.Scale(Vector3.forward * 2, transform.position);
        }
    }
    protected float alarmedTimer;
    public float AlarmedTimer {get => alarmedTimer; set => alarmedTimer = value;}
    public bool IgnoresAlarm {get => ignoresAlarm; set => ignoresAlarm = value;}
    public bool IsAlarmed {get => alarmedTimer > 0;}
    public bool IsKnockedOut {get => status == Status.KnockedOut;}
    protected Gender gender;
    protected Rigidbody bodyHipsRB;
    public Rigidbody BodyHipsRB { get => bodyHipsRB; }
    protected bool knockedOut;


    protected virtual void Start()
    {
        alarm = FindAnyObjectByType<Alarm>();
        uiManager = FindAnyObjectByType<UIManager>();
        detection = GetComponent<Detection>();
        enemyMovement = GetComponent<EnemyMovement>();
        player = FindAnyObjectByType<Player>();
        necessities = new Dictionary<(Action, float, float), float>();

        curiousTimer = 0f;
        suspectfulTimer = 0f;
        alarmedTimer = 0f;
        knockedOut = false;

        if (detection == null)
            detection = GetComponentInChildren<Detection>();

        if (enemyMovement == null)
            enemyMovement = GetComponentInChildren<EnemyMovement>();


        if (GetComponent<EnemyCamera>() == null)
        {
            List<GameObject> modelsToUse = new List<GameObject>();

            switch (UnityEngine.Random.Range(0, 2))
            {
                case 0:
                    gender = Gender.Male;
                    modelsToUse = maleModels;
                    break;

                case 1:
                    gender = Gender.Female;
                    modelsToUse = femaleModels;
                    break;

                default:
                    gender = Gender.AttackHelicopter;
                    modelsToUse = maleModels;
                    break;
            }

            if (maleModels.Count == 0)
            {
                gender = Gender.Female;
                modelsToUse = femaleModels;
            }

            if (femaleModels.Count == 0)
            {
                gender = Gender.Male;
                modelsToUse = maleModels;
            }


            if (modelsToUse.Count > 0)
            {
                Queue<Transform> destroyQueue = new Queue<Transform>();

                for (int i = 0; i < model.childCount; i++)
                {
                    destroyQueue.Enqueue(model.GetChild(i));
                    model.GetChild(i).gameObject.SetActive(false);
                }

                while (destroyQueue.Count > 0)
                    Destroy(destroyQueue.Dequeue().gameObject);


                Instantiate(modelsToUse[UnityEngine.Random.Range(0, modelsToUse.Count)], model);
            }

            for (int i = 0; i < model.GetChild(0).childCount; i++)
            {
                if(model.GetChild(0).GetChild(i).CompareTag("BodyHips"))
                {
                    bodyHipsRB = model.GetChild(0).GetChild(i).GetComponent<Rigidbody>();
                    audioPlayers.parent = model.GetChild(0).GetChild(i);
                    audioSources.parent = model.GetChild(0).GetChild(i);
                    break;
                }
            }

            enemyMovement.ResetNPC(enemyMovement.SpawnPos, enemyMovement.SpawnRot);
            GetComponent<EnemyDrops>().SetItemDrops();


            AddNecessity(() =>
            {
                List<MovementTarget> bathroomTargets = new List<MovementTarget>();
                
                switch (gender)
                {
                    case Gender.Male: bathroomTargets = bathroomTargetsMale; break;
                    case Gender.Female: bathroomTargets = bathroomTargetsFemale; break;
                    case Gender.AttackHelicopter: bathroomTargets = bathroomTargetsMale; break;
                }

                if (bathroomTargets.Count > 0 && !ignoresAlarm)
                {
                    //Debug.Log($"{name} is going to the {gender} bathroom!");
                    enemyMovement.PickTarget(bathroomTargets, true, true);
                }
            },
                startBathroomTimer, minBathroomTimer, maxBathroomTimer);
        }
    }

    protected virtual void Update()
    {
        if(enemyMovement.LeavingMap && IsConscious)
            EnemyMovement.ExitMap();
        
        else
        {
            TickNecessities();
            CheckDetection();

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
    /// Makes this enemy start acting normal.
    /// </summary>
    public virtual void BecomeNormal(bool ignoreAlarm = false)
    {
        if(IsConscious && (!IsAlarmed || ignoreAlarm) && !alarm.IsOn)
        {
            if(EnemyStatus != Status.Normal)
                Debug.Log($"{name} is now acting normal!");

            curiousTimer = 0f;
            suspectfulTimer = 0f;
            alarmedTimer = 0f;
            enemyMovement.MoveTimer *= 0.5f;
            enemyMovement.MoveTimer += 2f;
            EnemyStatus = Status.Normal;
        }
    }

    /// <summary>
    /// Makes this enemy start acting curious.
    /// </summary>
    public virtual void BecomeCurious()
    {
        if(IsConscious && !IsAlarmed && !alarm.IsOn)
        {
            if(EnemyStatus != Status.Curious)
                Debug.Log($"{name} is now acting curious!");

            curiousTimer = curiousTime;
            EnemyStatus = Status.Curious;
        }
    }

    /// <summary>
    /// Makes this enemy start acting suspectful.
    /// </summary>
    public virtual void BecomeSuspectful()
    {
        if(IsConscious && !IsAlarmed && !alarm.IsOn)
        {
            if(EnemyStatus != Status.Suspectful)
                Debug.Log($"{name} is now acting suspectful!");

            suspectfulTimer = suspectfulTime;
            EnemyStatus = Status.Suspectful;
        }
    }

    /// <summary>
    /// Alarms this enemy, initiating its alarmed behaviour.
    /// </summary>
    public virtual void BecomeAlarmed()
    {
        if (IsConscious)
        {
            if (!IsAlarmed)
            {
                Debug.Log($"{name} was alarmed!");
                if (alarmAudioPlayer != null) alarmAudioPlayer.Play();
                onBecomeAlarmed?.Invoke();
            }

            alarmedTimer = alarmedTime;
            detection.DetectionMeter = detection.DetectionLimit;
            enemyMovement.Halted = false;
        }
    }

    /// <summary>
    /// Resets this enemy's variables
    /// (This is used when respawning enemies at the entrance).
    /// </summary>
    public void ResetNPC()
    {
        Start();
    }

    /// <summary>
    /// Adds a new Necessity for this NPC to execute over time.
    /// </summary>
    /// <param name="action">The code to execute when the necessity is called.</param>
    /// <param name="minTimer">The minimum time needed to recall this necessity.</param>
    /// <param name="maxTimer">The maximum time needed to recall this necessity.</param>
    public void AddNecessity(Action action, float startTimer, float minTimer, float maxTimer)
    {
        float timer = UnityEngine.Random.Range(startTimer, maxTimer - startTimer);

        necessities.Add((action, minTimer, maxTimer), timer);
    }

    /// <summary>
    /// Ticks down the Necessity timers and executes their respective code when their timer hit 0, then resets it.
    /// </summary>
    protected void TickNecessities()
    {
        // Creates a separate new dictionary to prevent modifying the one read by the foreach loop
        Dictionary<(Action, float, float), float> updatedNecessities = new Dictionary<(Action, float, float), float>();

        foreach (KeyValuePair<(Action, float, float), float> kv in necessities)
        {
            // Ticks down the timer
            float timer = kv.Value;
            timer -= Time.deltaTime;

            // Calls the code when the timer hits 0, and resets the timer
            if (timer <= 0f)
            {
                kv.Key.Item1.Invoke();
                timer = UnityEngine.Random.Range(kv.Key.Item2, kv.Key.Item3);
            }

            // Updates the timer
            updatedNecessities.Add(kv.Key, timer);
        }

        // Updates the list of necessities
        necessities = updatedNecessities;
    }

    /// <summary>
    /// Changes the enemy's status if it's detection meter reaches certain tresholds.
    /// </summary>
    protected virtual void CheckDetection()
    {
        if (IsConscious && alarm.IsOn && (EnemyStatus == Status.Normal ||
            EnemyStatus == Status.Curious || EnemyStatus == Status.Suspectful))
        {
            BecomeAlarmed();
        }

        else if (IsConscious && !IsAlarmed)
        {
            // At 2 thirds of detection, becomes suspectful of the nearest suspicious object it sees
            if (detection.DetectionMeter >= detection.DetectionLimit * 2 / 3)
            {
                BecomeSuspectful();
            }

            // At 1 third of detection, becomes curious of the nearest suspicious object it sees
            else if ((detection.DetectionMeter >= detection.DetectionLimit / 5) || (detection.TooCloseToPlayer && player.IsMoving))
            {
                if (EnemyStatus == Status.Normal || EnemyStatus == Status.Curious)
                    BecomeCurious();

                else
                    BecomeSuspectful();
            }
        }
    }

    /// <summary>
    /// How this NPC behaves while its status is Normal.
    /// </summary>
    protected virtual void NormalBehavior()
    {
        enemyMovement.SetMovementSpeed(enemyMovement.WalkSpeed);
        enemyMovement.Halted = false;

        enemyMovement.Patrol();
    }

    /// <summary>
    /// How this NPC behaves while its status is Curious.
    /// </summary>
    protected virtual void CuriousBehavior()
    {
        enemyMovement.SetMovementSpeed(enemyMovement.WalkSpeed);

        // Stops and looks at the nearest suspicious object while curious
        if(curiousTimer > 0f)
        {
            enemyMovement.Halt();
            enemyMovement.Halted = true;

            if(detection.ClosestSuspiciousObject != null)
                enemyMovement.LookAt(detection.ClosestSuspiciousObject.transform.position);

            else if ((player.transform.position - transform.position)
                .magnitude <= suspectfulInspectRange * 2)
            {
                enemyMovement.LookAt(player.transform.position);
            }

            TickBehaviorTimers();
        }

        // Otherwise, goes back to normal
        else
        {
            BecomeNormal();
        }
    }

    /// <summary>
    /// How this NPC behaves while its status Suspectful.
    /// </summary>
    protected virtual void SuspectfulBehavior()
    {
        enemyMovement.SetMovementSpeed(enemyMovement.WalkSpeed);

        if(suspectfulTimer > 0)
        {
            if (detection.ClosestSuspiciousObject != null)
            {
                // Follows the nearest suspicious object until it's within the minimum range
                if ((detection.ClosestSuspiciousObject.transform.position -
                    transform.position).magnitude > suspectfulInspectRange)
                {
                    enemyMovement.Halted = false;

                    enemyMovement.MoveTo(detection.ClosestSuspiciousObject.transform.position);
                    enemyMovement.RotateTo(null);
                }

                // Stops moving and looks at the suspicious object
                else
                {
                    enemyMovement.Halt();
                    enemyMovement.Halted = true;

                    enemyMovement.LookAt(detection.ClosestSuspiciousObject.transform.position);
                }
            }

            else if ((player.transform.position - transform.position)
                .magnitude <= suspectfulInspectRange * 2)
            {
                enemyMovement.LookAt(player.transform.position);
            }

            else
            {
                enemyMovement.Halted = false;
            }

            TickBehaviorTimers();
        }

        else
        {
            BecomeNormal();
        }
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
            Debug.Log("applying stuck behavior to " + name);
            enemyMovement.Wander(transform.position, 20f);
            EnemyStatus = Status.Searching;
        }

        enemyMovement.LookAround(aggro:true);
    }

    /// <summary>
    /// How this NPC behaves while its status is Knocked Out.
    /// </summary>
    protected virtual void KnockedOutBehavior()
    {
        EnemyMovement.EnableBody();
        
        if(!knockedOut)
        {
            enemyMovement.SetMovementSpeed(0f);

            if(enemyMovement.LeavingMap)
                enemyMovement.LeavingMap = false;


            EnemyMovement.GetKnockedOut();

            bodyHipsRB.AddForce(Camera.main.transform.forward * 20f, ForceMode.Impulse);

            knockedOut = true;
        }
    }

    /// <summary>
    /// Ticks down any behavior timers that are positive.
    /// </summary>
    protected void TickBehaviorTimers()
    {
        if(curiousTimer > 0)
            curiousTimer -= Time.deltaTime;

        if(suspectfulTimer > 0)
            suspectfulTimer -= Time.deltaTime;

        if(alarmedTimer > 0)
            alarmedTimer -= Time.deltaTime;
    }

    /// <summary>
    /// Causes this enemy to be knocked out and stop moving.
    /// </summary>
    public void GetKnockedOut()
    {
        if (EnemyStatus != Status.KnockedOut)
        {
            EnemyStatus = Status.KnockedOut;
            detection.DetectionMeter = 0;

            // Drops all items this enemy is carrying
            GetComponentInChildren<EnemyItemInventory>().DropAllItems();

            OnKnockout?.Invoke(this, EventArgs.Empty);
        }
    }
    
    public void GetTased()
    {
        tasedPlayer.Play();
        
        GetKnockedOut();
    }
}
