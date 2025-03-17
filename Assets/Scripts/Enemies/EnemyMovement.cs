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
    [SerializeField] private bool chooseTargetsAsSequence = false;
    [SerializeField] private List<Transform> movementTargets;
    [SerializeField] private int startingTargetIndex;

    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float turnSpeed = 2.5f;
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
    private int movementTargetIndex;
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
    private Vector3 lastTargetPos;
    public Vector3 LastTargetPos {get => lastTargetPos;}
    private float? lastTargetRot;
    public float? LastTargetRot {get => lastTargetRot;}
    private Vector3 spawnPos;
    public Vector3 SpawnPos {get=> spawnPos;}
    private float spawnRot;
    public float SpawnRot {get=> spawnRot;}
    private Player player;
    private Detection detection;
    private BodyDisguise bodyDisguise;
    private BodyCarry bodyCarry;
    private Animator animator;
    private NavMeshAgent navMeshAgent;
    private Alarm alarm;
    private bool IsFacingTarget {get=> lastTargetRot != null &&
        Mathf.Abs((float)lastTargetRot - transform.eulerAngles.y) <= 1f;}
    public bool IsAtDestination {get => (new Vector3(lastTargetPos.x, 0f, lastTargetPos.z) -
        new Vector3(transform.position.x, 0f, transform.position.z)).magnitude <=
            (alarm.IsOn ? navMeshAgent.stoppingDistance * 3f : navMeshAgent.stoppingDistance);}
    private float stuckTimer;
    private Vector3 lastSelfPos;
    private List<PanicButton> mapButtons;
    private Transform chosenNearestButton;
    private List<MapEntrance> mapEntrances;
    private Transform chosenNearestExit;
    private bool leavingMap;
    private bool knockedOut;
    public bool LeavingMap {get => leavingMap; set{leavingMap = value;}}
    private float _footstepTimer;

    private void Start()
    {
        currentStatus = Status.Normal;

        if(spawnPos != Vector3.zero)
            ResetNPC(spawnPos, spawnRot);
            
        else ResetNPC();
    }

    // Default version to be called from the inspector
    public void ResetNPC()
    {
        ResetNPC(transform.position, transform.eulerAngles.y);
    }

    // (Re)Initializes all the variables in the NPC (used for respawned enemies)
    public void ResetNPC(Vector3 oldSpawnPos, float oldSpawnRot)
    {
        // Only works on living enemies
        if(currentStatus != Status.KnockedOut)
        {
            // Sets up all the variables
            currentStatus = Status.Normal;
            movementTargetIndex = startingTargetIndex - 1;
            moveTimer = 0;
            turnTimer = 0;
            searchTimer = 0;
            stuckTimer = 0;
            tasedTimer = tasedTime;
            spawnPos = oldSpawnPos;
            spawnRot = oldSpawnRot;
            lastSelfPos = transform.position;
            lastTargetPos = Vector3.zero;
            lastTargetRot = 0f;
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
            
            // Begins leaving the map during an alarm
            if(alarm.IsOn)
                ExitMap();
            
            // When alarm is off, attempts to raise it by either escaping or
            // pressing a panic button, whichever is closer
            else
            {
                CheckNearestExit();
                CheckNearestPanicButton();

                float distanceToButton = (chosenNearestButton.position - transform.position).magnitude;
                float distanceToExit = (chosenNearestExit.position - transform.position).magnitude;

                if(distanceToButton <= distanceToExit)
                {
                    MoveTo(chosenNearestButton.position);
                    RotateTo(chosenNearestButton.eulerAngles.y);
                }
                
                else
                {
                    MoveTo(chosenNearestExit.position);
                    RotateTo(chosenNearestExit.eulerAngles.y);
                }
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
            {
                MoveTo(Detection.lastPlayerPos);
                RotateTo(null);
            }

            // Checks if the enemy is stuck somewhere and makes it start searching after a while
            if((transform.position - lastSelfPos).magnitude < navMeshAgent.speed * Time.deltaTime
                && IsConscious)
            {
                stuckTimer += Time.deltaTime;

                if(stuckTimer >= stuckTime)
                {
                    MoveTo(transform.position);
                    RotateTo(null);
                    currentStatus = Status.Searching;
                    stuckTimer = 0f;
                }
            }

            // Resets the stuck timer if the enemy is not stuck
            else
            {
                stuckTimer = 0f;
            }


            // The enemy turns around faster and more often if it's looking for the player
            if(IsAtDestination && IsFacingTarget)
            {
                CheckForTurning(aggro:true);
            }

            // Resets the turn timer with a faster cooldown
            else
            {
                turnTimer = aggroTurnTime;
            }
        }

        // ------------ Searching ------------ 
        else if (currentStatus == Status.Searching)
        {
            navMeshAgent.speed = walkSpeed;

            // If this enemy is at its destination, decreases the movement timer and looks around
            if(IsAtDestination && IsFacingTarget)
            {
                if(searchTimer > 0)
                {
                    searchTimer -= Time.deltaTime;

                    CheckForTurning();
                }

                // Resets the timers with random values and picks a random position nearby
                // (outside of the movement targets)
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
            // Stops this enemy from moving when tased
            navMeshAgent.speed = 0f;
            navMeshAgent.enabled = false;
            leavingMap = false;
            MoveTo(transform.position);
            RotateTo(transform.eulerAngles.y);

            // Decreases the tased timer while tased
            if(tasedTimer > 0)
            {
                animator.SetBool("Tased", true);
                animator.applyRootMotion = false;
                tasedTimer -= Time.deltaTime;
            }

            // After the tased timer ends, the enemy becomes alarmed
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
            // Enables this enemy's body and disguise when knocked out
            if(!bodyDisguise.enabled && bodyDisguise.HasDisguise)
                bodyDisguise.enabled = true;
                
            else if(bodyDisguise.enabled && !bodyDisguise.HasDisguise)
                bodyDisguise.enabled = false;

            if(!bodyCarry.enabled)
                bodyCarry.enabled = true;

            // Runs this code on the first frame of being knocked out
            if(!knockedOut)
            {
                // Stops this enemy
                navMeshAgent.speed = 0f;
                navMeshAgent.enabled = false;
                leavingMap = false;
                MoveTo(transform.position);
                RotateTo(transform.eulerAngles.y);

                movementPosTargets = new List<Vector3>{transform.position};

                taserLoopPlayer.Stop();
                animator.SetBool("KO", true);
                animator.applyRootMotion = false;
                
                // Invokes an event from the inspector when knocked out
                onKnockOut?.Invoke();
                knockedOut = true;
            }
        }


        // Updates its last position when conscious to check when it's stuck
        if(IsConscious)
        {
            lastSelfPos = transform.position;
        }
        
        // Plays footsteps when walking and running
        if(_footstepTimer + footstepInterval * navMeshAgent.speed / walkSpeed <= Time.time && navMeshAgent.velocity.magnitude >= 1)
        {
            footstepPlayer.Play();
            _footstepTimer = Time.time;
        }
    }

    /// <summary>
    /// Makes this enemy move around the map unless its halted or static.
    /// </summary>
    private void Patrol()
    {
        // Stops this enemy when it's seeing something suspicious
        if(halted)
        {
            MoveTo(transform.position);
            RotateTo(null);
        }

        else if(Time.timeScale != 0 && lastTargetRot != null && IsAtDestination
            && !IsFacingTarget && moveTimer > 0 && !isStatic)
        {
            Vector3 difference = Vector3.up * Mathf.Clamp((float)lastTargetRot - transform.eulerAngles.y, -turnSpeed, turnSpeed);
            transform.rotation = Quaternion.Euler(transform.eulerAngles + difference);

            if(IsFacingTarget) lastTargetRot = null;
        }

        // Progressively decreases the movement timer if the agent is at its target
        else if((IsAtDestination && moveTimer > 0) || (isStatic && !movingToSetTarget && looksAround))
        {
            moveTimer -= Time.deltaTime;

            if(looksAround)
            {
                CheckForTurning();
            }
        }

        // Checks if this enemy can move or if it was ordered to move
        if(moveTimer <= 0 && ((!halted && !isStatic) || movingToSetTarget))
        {
            if(chooseTargetsAsSequence)
            {
                movementTargetIndex++;
                if(movementTargetIndex >= movementTargets.Count) movementTargetIndex = 0;
            }

            else
            {
                // Rolls a random index within the number of available targets and
                // loops until it gets a value different than the last chosen index
                movementTargetIndex = Random.Range(0, movementTargets.Count());

                if(movementTargets[movementTargetIndex] == null)
                {
                    Start();
                }

                
                int loop = 0;
                while(movementTargets[movementTargetIndex].position == lastTargetPos)
                {
                    movementTargetIndex = Random.Range(0, movementTargets.Count());

                    // If it loops for too long, breaks out of the loop
                    if (++loop >= 100)
                    {
                        movementTargetIndex = 0;
                        break;
                    }
                }
            }

            // The enemy is only forced to move once
            if(movingToSetTarget)
                movingToSetTarget = false;

            // Tells the NPC to move towards the chosen index position
            MoveTo(movementTargets[movementTargetIndex].position);
            RotateTo(movementTargets[movementTargetIndex].eulerAngles.y);
        }

        // Static enemies that arent being forced to move will remain in their spawn position
        else if(isStatic && !movingToSetTarget)
        {
            MoveTo(spawnPos);
            RotateTo(null);
        }
    }

    /// <summary>
    /// Tells this enemy to move to a given position.
    /// </summary>
    /// <param name="destination">The position to move this enemy.</param>
    public void MoveTo(Vector3 destination)
    {
        if(destination != null && destination != lastTargetPos && IsConscious)
        {
            // Moves to the given destination and registers it as the last chosen
            navMeshAgent.SetDestination(destination);
            lastTargetPos = destination;

            // Stops any rotation coroutines
            StopAllCoroutines();

            // Resets the patrol movement cooldown
            moveTimer = Random.Range(minMovementTime, maxMovementTime);
        }
    }

    /// <summary>
    /// Tells this enemy to rotate to a given angle (if null, the enemy just faces forward).
    /// </summary>
    /// <param name="targetRotation">The angle to rotate this enemy towards. If null, the enemy won't rotate.</param>
    public void RotateTo(float? targetRotation)
    {
        if(targetRotation != lastTargetRot && IsConscious)
        {
            lastTargetRot = AdjustRotationAngle(targetRotation);
        }
    }

    /// <summary>
    /// Converts a given angle into the range between 0-360 degrees
    /// </summary>
    private float? AdjustRotationAngle(float? rotation)
    {
        if(rotation != null)
        {
            while(rotation < 0) rotation += 360f;
            while(rotation >= 360) rotation -= 360f;

            return rotation;
        }

        else return null;
    }

    /// <summary>
    /// Picks a random position within the NavMesh on a radius around the given position, and then moves to it
    /// </summary>
    /// <param name="center">The center of the wandering area.</param>
    /// <param name="radius">The radius of the wandering area.</param>
    /// <param name="layermask">Which layers should be ignored when picking a position.</param>
    public void Wander(Vector3 center, float radius, int layermask = NavMesh.AllAreas)
    {
        Vector3 randomDirection = center + (Random.insideUnitSphere * radius);

        NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, radius, layermask);

        MoveTo(navHit.position);
        RotateTo(null);
    }

    /// <summary>
    /// Checks if this enemy can turn around and calculates how much it turns.
    /// </summary>
    /// <param name="aggro"></param>
    private void CheckForTurning(bool aggro = false)
    {
        // Decreases the turn timer if it's not ready to turn around.
        if(turnTimer > 0)
        {
            turnTimer -= Time.deltaTime;
        }

        // Calculates a random angle to rotate and turns around in place.
        else
        {
            float newRot = transform.eulerAngles.y;
            float turnAngle = Random.Range(-maxTurnAngle, maxTurnAngle);

            if(turnAngle < minTurnAngle)
                turnAngle += minTurnAngle;

            else if(turnAngle > -minTurnAngle)
                turnAngle -= minTurnAngle;

            newRot += turnAngle;

            // Begins rotating to the chosen angle
            RotateTo(AdjustRotationAngle(newRot));


            // Resets the timer to a faster one when chasing, otherwise uses a random timer.
            if(aggro)
                turnTimer = aggroTurnTime;
            else
                turnTimer = Random.Range(minTurnTime, maxTurnTime);
        }
    }

    /// <summary>
    /// Updates the list of panic buttons and registers the nearest one.
    /// </summary>
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

        chosenNearestButton = mapButtons[chosenButtonIndex].transform;
    }

    /// <summary>
    /// Updates the list of exits and registers the nearest one.
    /// </summary>
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

        chosenNearestExit = mapEntrances[chosenExitIndex].transform;
    }

    /// <summary>
    /// Sets this enemy's movement targets to a given list.
    /// </summary>
    /// <param name="newMovementTargets">The given list of new movement targets.</param>
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

    /// <summary>
    /// Makes this enemy begin leaving the map if it's conscious.
    /// </summary>
    public void ExitMap()
    {
        if(IsConscious)
        {
            leavingMap = true;
            CheckNearestExit();
            MoveTo(chosenNearestExit.position);
            RotateTo(chosenNearestExit.eulerAngles.y);
        }
    }

    /// <summary>
    /// Causes this enemy to be tased and stop moving.
    /// </summary>
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
