using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;


[RequireComponent(typeof(NavMeshAgent))] 
public class EnemyMovement : MonoBehaviour
{
    public enum Status {Normal, Fleeing, Searching, Chasing, Tased, KnockedOut};

    [SerializeField] private bool isStatic = false;
    [SerializeField] private bool looksAround = true;
    [SerializeField] private List<Transform> movementTargets;
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private float minMovementTime = 5f;
    [SerializeField] private float maxMovementTime = 12f;
    [SerializeField] private float minTurnTime = 2f;
    [SerializeField] private float maxTurnTime = 5f;
    [SerializeField] private float aggroTurnTime = 1.5f;
    [SerializeField] private float minTurnAngle = 35f;
    [SerializeField] private float maxTurnAngle = 90f;
    [SerializeField] private float minSearchTime = 4f;
    [SerializeField] private float maxSearchTime = 10f;
    [SerializeField] private float searchRadius = 20f;
    [SerializeField] private float stuckTime = 3f;
    [SerializeField] protected float tasedTime = 5f;
    [SerializeField] protected UnityEvent onKnockOut;
    [SerializeField] protected PlayAudio taserLoopPlayer;
    [SerializeField] protected PlayAudio knockoutPlayer;
    [SerializeField] protected PlayAudio footstepPlayer;
    [SerializeField] [Range(0.1f, 2f)] private float footstepInterval = 0.45f;

    private Status status;
    private bool movingToSetTarget;
    public bool MovingToSetTarget {get => movingToSetTarget; set => movingToSetTarget = value;}
    public bool IsConscious {get => currentStatus != Status.KnockedOut && currentStatus != Status.Tased;}

    public Status currentStatus
    { 
        get => status;
        set
        {
            if(status != Status.KnockedOut && value == Status.KnockedOut) knockoutPlayer.Play();
            if(status != Status.KnockedOut) status = value;
            _footstepTimer = Time.time - footstepInterval;
        }
    }
    public bool halted = false;
    public List<Transform> MovementTargets {get => movementTargets;}
    private List<Vector3> movementPosTargets;
    public List<Vector3> MovementPosTargets {get => movementPosTargets;}
    public bool IsStatic {get => isStatic;}

    private float moveTimer;
    private float turnTimer;
    private float searchTimer;
    private float tasedTimer;
    public float TasedTimer {get => tasedTimer;}
    public float TasedTime {get => tasedTime;}
    private Vector3 lastTarget;
    public Vector3 LastTarget {get => lastTarget;}
    private Vector3 spawnPos;
    public Vector3 SpawnPos {get=> spawnPos;}
    private Player player;
    private Detection detection;
    private BodyDisguise bodyDisguise;
    private BodyCarry bodyCarry;
    private Animator animator;
    private NavMeshAgent navMeshAgent;
    private Alarm alarm;
    public bool IsAtDestination {get => (new Vector3(lastTarget.x, 0f, lastTarget.z) -
        new Vector3(transform.position.x, 0f, transform.position.z)).magnitude <= navMeshAgent.stoppingDistance * 3f;}
    private float stuckTimer;
    private Vector3 lastSelfPos;
    private List<PanicButton> mapButtons;
    private Vector3 chosenNearestButton;
    private List<MapEntrance> mapEntrances;
    private Vector3 chosenNearestExit;
    private bool leavingMap;
    private bool knockedOut;
    public bool LeavingMap {get => leavingMap; set{leavingMap = value;}}
    private float _footstepTimer;

    private void Start()
    {
        currentStatus = Status.Normal;

        if(spawnPos != Vector3.zero)
            ResetNPC(spawnPos);
            
        else ResetNPC();
    }

    // Default version to be called from the inspector
    public void ResetNPC()
    {
        ResetNPC(transform.position);
    }

    // (Re)Initializes all the variables in the NPC (used for respawned enemies)
    public void ResetNPC(Vector3 oldSpawnPos)
    {
        // Only works on living enemies
        if(currentStatus != Status.KnockedOut)
        {
            // Sets up all the variables
            currentStatus = Status.Normal;
            moveTimer = 0;
            turnTimer = 0;
            searchTimer = 0;
            stuckTimer = 0;
            tasedTimer = tasedTime;
            spawnPos = oldSpawnPos;
            lastSelfPos = transform.position;
            lastTarget = Vector3.zero;
            player = FindAnyObjectByType<Player>();
            detection = GetComponentInChildren<Detection>();
            bodyDisguise = GetComponentInChildren<BodyDisguise>();
            bodyCarry = GetComponentInChildren<BodyCarry>();
            animator = GetComponent<Animator>();
            bodyCarry.ResetNPC();
            bodyDisguise.ResetNPC();
            bodyCarry.enabled = false;
            bodyDisguise.enabled = false;
            navMeshAgent = GetComponent<NavMeshAgent>();
            navMeshAgent.enabled = true;
            alarm = FindAnyObjectByType<Alarm>();
            mapButtons = FindObjectsByType<PanicButton>(FindObjectsSortMode.None).ToList();
            mapEntrances = FindObjectsByType<MapEntrance>(FindObjectsSortMode.None).ToList();
            leavingMap = false;
            knockedOut = false;
            taserLoopPlayer.Stop();
            _footstepTimer  = Time.time;

            // Forcefully sets the NavMeshAgent to the NPC type if it isn't already one
            if(navMeshAgent.agentTypeID != -1372625422)
                navMeshAgent.agentTypeID = -1372625422;

            // Static enemies use their position as their target
            if(isStatic)
            {
                movementTargets = new List<Transform>() {transform};
                movementPosTargets = new List<Vector3>() {spawnPos};
            }

            // Converts the transform list into Vector3 positions
            else
            {
                movementPosTargets = new List<Vector3>();
                foreach(Transform t in movementTargets)
                    movementPosTargets.Add(t.position);
            }
        }
    }

    // Checks this enemy's status and moves accordingly.
    private void Update()
    {
        if(leavingMap && IsConscious)
        {
            ExitMap();
        }

        // ------------ Normal ------------ 
        else if(currentStatus == Status.Normal)
        {
            /*
            // Calculates the direction towards the current most suspicious object
            Vector3 suspicionLookPos = new Vector3(Detection.suspiciousObjPos.x,
            transform.position.y, Detection.suspiciousObjPos.z);
            */

            // At 2 thirds of detection, moves towards the suspicious object to see it better
            if(detection.DetectionMeter >= detection.DetectionLimit * 2 / 3 &&
                currentStatus == Status.Normal)
            {
                halted = true;
                //halted = false;
                //transform.LookAt(suspicionLookPos);
                //MoveTo(Detection.lastPlayerPos);
            }

            // At 1 third of detection, stops in place and looks at the suspicious object
            else if(detection.DetectionMeter >= detection.DetectionLimit * 1 / 3 &&
                currentStatus == Status.Normal)
            {
                halted = true;
                //transform.LookAt(suspicionLookPos);
            }

            else
                halted = false;
            
            navMeshAgent.speed = walkSpeed;
            Patrol();
        }

        // ------------ Fleeing ------------ 
        else if (currentStatus == Status.Fleeing)
        {
            navMeshAgent.speed = runSpeed;
            
            if(alarm.IsOn)
                ExitMap();
            
            else
            {
                CheckNearestExit();
                CheckNearestPanicButton();

                float distanceToButton = (chosenNearestButton - transform.position).magnitude;
                float distanceToExit = (chosenNearestExit - transform.position).magnitude;

                if(distanceToButton <= distanceToExit)
                    MoveTo(chosenNearestButton);
                
                else
                    MoveTo(chosenNearestExit);
            }
        }
        
        // ------------ Chasing ------------ 
        else if(currentStatus == Status.Chasing)
        {
            navMeshAgent.speed = runSpeed;

            // Police always know where the player is
            if(GetComponent<EnemyPolice>())
                MoveTo(player.transform.position);

            // Others will check on the last place the player was seen
            else
                MoveTo(Detection.lastPlayerPos);

            if((transform.position - lastSelfPos).magnitude < navMeshAgent.speed * Time.deltaTime
                && IsConscious)
            {
                stuckTimer += Time.deltaTime;

                if(stuckTimer >= stuckTime)
                {
                    MoveTo(transform.position);
                    currentStatus = Status.Searching;
                    stuckTimer = 0f;
                }
            }

            else
            {
                stuckTimer = 0f;
            }


            if(IsAtDestination)
            {
                CheckForTurning(aggro:true);
            }

            else
            {
                turnTimer = aggroTurnTime;
            }
        }

        // ------------ Searching ------------ 
        else if (currentStatus == Status.Searching)
        {
            navMeshAgent.speed = walkSpeed;

            if(IsAtDestination)
            {
                if(searchTimer > 0)
                {
                    searchTimer -= Time.deltaTime;

                    CheckForTurning();
                }

                else
                {
                    searchTimer = Random.Range(minSearchTime, maxSearchTime);
                    turnTimer = Random.Range(minTurnTime, maxTurnTime);

                    Wander(transform.position, searchRadius);
                }
            }
        }

        // ------------ Tased ------------ 
        else if (currentStatus == Status.Tased)
        {
            navMeshAgent.speed = 0f;
            navMeshAgent.enabled = false;
            leavingMap = false;
            MoveTo(transform.position);

            if(tasedTimer > 0)
            {
                animator.SetBool("Tased", true);
                animator.applyRootMotion = false;
                tasedTimer -= Time.deltaTime;
            }

            else
            {
                taserLoopPlayer.Play();
                animator.SetBool("Tased", false);
                animator.applyRootMotion = true;

                currentStatus = Status.Normal;
                GetComponent<Enemy>().BecomeAlarmed();
            }
        }

        // ------------ Knocked Out ------------ 
        else if (currentStatus == Status.KnockedOut)
        {
            if(!bodyDisguise.enabled && bodyDisguise.HasDisguise)
                bodyDisguise.enabled = true;
                
            else if(bodyDisguise.enabled && !bodyDisguise.HasDisguise)
                bodyDisguise.enabled = false;

            if(!bodyCarry.enabled)
                bodyCarry.enabled = true;

            if(!knockedOut)
            {
                navMeshAgent.speed = 0f;
                navMeshAgent.enabled = false;
                leavingMap = false;
                MoveTo(transform.position);

                movementPosTargets = new List<Vector3>{transform.position};

                taserLoopPlayer.Stop();
                animator.SetBool("KO", true);
                animator.applyRootMotion = false;
                
                onKnockOut?.Invoke();
                knockedOut = true;
            }
        }


        if(IsConscious)
        {
            lastSelfPos = transform.position;
        }
        
        if(_footstepTimer + footstepInterval * navMeshAgent.speed / walkSpeed <= Time.time && navMeshAgent.velocity.magnitude >= 1)
        {
            footstepPlayer.Play();
            _footstepTimer = Time.time;
        }
    }

    private void Patrol()
    {
        if(halted)
        {
            MoveTo(transform.position);
        }

        // Progressively decreases the movement timer if the agent is at its target
        else if((IsAtDestination && moveTimer > 0) || (isStatic && !movingToSetTarget))
        {
            moveTimer -= Time.deltaTime;

            if(looksAround)
            {
                CheckForTurning();
            }
        }

        if(moveTimer <= 0 && ((!halted && !isStatic) || movingToSetTarget))
        {
            // Rolls a random index within the number of available targets and
            // loops until it gets a value different than the last chosen index
            int index = Random.Range(0, movementPosTargets.Count());

            if(movementPosTargets[index] == null)
            {
                Start();
            }

            
            int loop = 0;
            while(movementPosTargets[index] == lastTarget)
            {
                index = Random.Range(0, movementPosTargets.Count());

                // If it loops for too long, breaks out of the loop
                if (++loop >= 100)
                {
                    index = 0;
                    break;
                }
            }

            if(movingToSetTarget)
                movingToSetTarget = false;

            // Tells the NPC to move towards the chosen index position
            MoveTo(movementPosTargets[index]);
        }

        else if(isStatic && !movingToSetTarget)
        {
            MoveTo(spawnPos);
        }
    }

    public void MoveTo(Vector3 destination)
    {
        if(destination != null && destination != lastTarget && IsConscious)
        {
            // Moves to the given destination and registers it as the last chosen
            navMeshAgent.SetDestination(destination);
            lastTarget = destination;

            // Stops any rotation coroutines
            StopAllCoroutines();

            // Resets the patrol movement cooldown
            moveTimer = Random.Range(minMovementTime, maxMovementTime);
        }
    }

    // Picks a random position within the NavMesh on a radius around the given position, and then moves to it
    public void Wander(Vector3 center, float radius, int layermask = NavMesh.AllAreas)
    {
        Vector3 randomDirection = center + (Random.insideUnitSphere * radius);

        NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, radius, layermask);

        MoveTo(navHit.position);
    }

    private void CheckForTurning(bool aggro = false)
    {
        if(turnTimer > 0)
        {
            turnTimer -= Time.deltaTime;
        }

        else
        {
            float turnAngle = Random.Range(-maxTurnAngle, maxTurnAngle);

            if(turnAngle < minTurnAngle)
                turnAngle += minTurnAngle;

            else if(turnAngle > -minTurnAngle)
                turnAngle -= minTurnAngle;

            StopAllCoroutines();
            StartCoroutine(TurnTo(transform.eulerAngles.y + turnAngle));


            if(aggro)
                turnTimer = aggroTurnTime;
            else
                turnTimer = Random.Range(minTurnTime, maxTurnTime);
        }
    }

    private IEnumerator TurnTo(float targetRotation)
    {
        while(transform.rotation.y != targetRotation)
        {
            float rotation = transform.eulerAngles.y;
            float increase = Time.deltaTime * turnSpeed;
            float limit = targetRotation - transform.eulerAngles.y;

            if(targetRotation > rotation)
                rotation += Mathf.Min(increase, limit);

            else if (targetRotation < rotation)
                rotation += Mathf.Max(-increase, limit);

            transform.eulerAngles = new Vector3(0f, rotation, 0f);

            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
    }

    public void CheckNearestPanicButton()
    {
        float distanceToNearestButton = float.MaxValue;
        int chosenButtonIndex = 0;
        mapButtons = FindObjectsByType<PanicButton>(FindObjectsSortMode.None).ToList();

        for(int i = 0; i < mapButtons.Count; i++)
        {
            int index = i;
            float distanceToButton = (mapButtons[i].transform.position - transform.position).magnitude;

            if(distanceToButton < distanceToNearestButton)
            {
                distanceToNearestButton = distanceToButton;
                chosenButtonIndex = index;
            }
        }

        chosenNearestButton = mapButtons[chosenButtonIndex].transform.position;
    }

    public void CheckNearestExit()
    {
        float distanceToNearestExit = float.MaxValue;
        int chosenExitIndex = 0;
        mapEntrances = FindObjectsByType<MapEntrance>(FindObjectsSortMode.None).ToList();

        for(int i = 0; i < mapEntrances.Count; i++)
        {
            int index = i;
            float distanceToExit = (mapEntrances[i].transform.position - transform.position).magnitude;

            if(distanceToExit < distanceToNearestExit)
            {
                distanceToNearestExit = distanceToExit;
                chosenExitIndex = index;
            }
        }

        chosenNearestExit = mapEntrances[chosenExitIndex].transform.position;
    }

    public void SetMovementTargets(List<Transform> newMovementTargets)
    {
        if(!leavingMap && IsConscious)
        {
            movementTargets = newMovementTargets;
            movementPosTargets = new List<Vector3>();

            foreach(Transform t in movementTargets)
                movementPosTargets.Add(t.position);
        }
    }

    public void ExitMap()
    {
        if(IsConscious)
        {
            leavingMap = true;
            CheckNearestExit();
            MoveTo(chosenNearestExit);
        }
    }

    public void GetTased()
    {
        if(currentStatus != Status.KnockedOut)
        {
            currentStatus = Status.Tased;
            tasedTimer = tasedTime;
            detection.DetectionMeter = 0;
            leavingMap = false;
            taserLoopPlayer.Play();
        }
    }
}
