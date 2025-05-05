using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;


[RequireComponent(typeof(NavMeshAgent))] 
public class EnemyMovement : MonoBehaviour
{

    [SerializeField] private bool isStatic = false;
    [SerializeField] private bool looksAround = true;
    [SerializeField] private bool chooseTargetsAsSequence = false;
    [SerializeField] private Transform movementTargetsParent;
    [SerializeField] private List<MovementTarget> movementTargets;
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
    [SerializeField] protected PlayAudio footstepPlayer;
    [SerializeField] [Range(0.1f, 2f)] private float footstepInterval = 0.45f;

    private Enemy enemySelf;
    private int movementTargetIndex;
    private bool movingToSetTarget;
    public bool MovingToSetTarget {get => movingToSetTarget; set => movingToSetTarget = value;}
    /*public bool IsConscious {get => currentStatus != Status.KnockedOut && currentStatus != Status.Tased;}*/

    /*public Status currentStatus
    { 
        get => status;
        set
        {
            if(status != Status.KnockedOut && value == Status.KnockedOut) knockoutPlayer.Play();
            if(status != Status.KnockedOut) status = value;
            footstepTimer = Time.time - footstepInterval;
        }
    }*/
    public float WalkSpeed {get=> walkSpeed;}
    public float RunSpeed {get=> runSpeed;}
    public bool halted = false;
    public bool Halted
    {
        get => halted;
        
        set
        {
            if(value) DeoccupyCurrentTarget();
            halted = value;
        }
    }
    public List<MovementTarget> MovementTargets {get => movementTargets;}
    private List<Vector3> movementPosTargets;
    public List<Vector3> MovementPosTargets {get => movementPosTargets;}
    public bool IsStatic {get => isStatic;}

    private float moveTimer;
    public float MoveTimer { get => moveTimer; set => moveTimer = value;}
    private float turnTimer;
    private float searchTimer;
    /*private float tasedTimer;
    public float TasedTimer {get => tasedTimer;}
    public float TasedTime {get => tasedTime;}*/
    private Vector3 lastTargetPos;
    public Vector3 LastTargetPos {get => lastTargetPos;}
    private float? lastTargetRot;
    public float? LastTargetRot {get => lastTargetRot;}
    private Vector3 spawnPos;
    public Vector3 SpawnPos {get=> spawnPos;}
    private float spawnRot;
    public float SpawnRot {get=> spawnRot;}
    private float buttonCheckTimer;
    private float exitCheckTimer;
    private BodyDisguise bodyDisguise;
    private BodyCarry bodyCarry;
    private DetectableObject detectableObject;
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
    private List<MapEntrance> mapEntrances;
    private bool leavingMap;
    private bool knockedOut;
    public bool LeavingMap {get => leavingMap; set{leavingMap = value;}}
    private float footstepTimer;

    private void Start()
    {
        if(spawnPos != Vector3.zero)
            ResetNPC(spawnPos, spawnRot);
            
        else ResetNPC();

        enemySelf.EnemyStatus = Enemy.Status.Normal;
    }

    // Default version to be called from the inspector
    public void ResetNPC()
    {
        ResetNPC(transform.position, transform.eulerAngles.y);
    }

    // (Re)Initializes all the variables in the NPC (used for respawned enemies)
    public void ResetNPC(Vector3 oldSpawnPos, float oldSpawnRot)
    {
        enemySelf = GetComponent<Enemy>();

        // Only works on living enemies
        if(enemySelf.EnemyStatus != Enemy.Status.KnockedOut)
        {
            // Sets up all the variables
            movementTargetIndex = startingTargetIndex - 1;
            moveTimer = 0;
            turnTimer = 0;
            searchTimer = 0;
            stuckTimer = 0;
            //tasedTimer = tasedTime;
            spawnPos = oldSpawnPos;
            spawnRot = oldSpawnRot;
            lastSelfPos = transform.position;
            lastTargetPos = transform.position;
            lastTargetRot = spawnRot;
            buttonCheckTimer = 0f;
            exitCheckTimer = 0f;
            bodyDisguise = GetComponentInChildren<BodyDisguise>();
            bodyCarry = GetComponentInChildren<BodyCarry>();
            detectableObject = GetComponentInChildren<DetectableObject>();
            animator = GetComponent<Animator>();
            bodyCarry.ResetNPC();
            bodyDisguise.ResetNPC();
            bodyCarry.enabled = false;
            bodyDisguise.enabled = false;
            detectableObject.DetectionMultiplier = bodyCarry.BodyDetectionMultiplier;
            detectableObject.enabled = false;
            navMeshAgent = GetComponent<NavMeshAgent>();
            navMeshAgent.enabled = true;
            alarm = FindAnyObjectByType<Alarm>();
            mapButtons = FindObjectsByType<PanicButton>(FindObjectsSortMode.None).ToList();
            mapEntrances = FindObjectsByType<MapEntrance>(FindObjectsSortMode.None).ToList();
            leavingMap = false;
            knockedOut = false;
            footstepTimer  = Time.time;

            // Forcefully sets the NavMeshAgent to the NPC type if it isn't already one
            if(navMeshAgent.agentTypeID != -1372625422)
                navMeshAgent.agentTypeID = -1372625422;

            // Static enemies use their position as their target
            if(isStatic)
            {
                MovementTarget selfTargetPos = MovementTarget.CreateMovementTarget
                    (transform.position, transform.rotation, movementTargetsParent);
                movementTargets = new List<MovementTarget>() {selfTargetPos};
                movementPosTargets = new List<Vector3>() {spawnPos};
            }

            // Converts the transform list into Vector3 positions
            else
            {
                movementPosTargets = new List<Vector3>();
                foreach(MovementTarget mt in movementTargets)
                    movementPosTargets.Add(mt.transform.position);
            }
        }
    }

    // Updates this enemy's previous position for StuckCheck(), and plays footsteps when walking
    private void Update()
    {
        // Updates its last position when conscious to check when it's stuck
        if(enemySelf.IsConscious)
        {
            lastSelfPos = transform.position;
        }
        
        // Plays footsteps when walking and running
        if(footstepTimer + footstepInterval * navMeshAgent.speed / walkSpeed <= Time.time && navMeshAgent.velocity.magnitude >= 1)
        {
            footstepPlayer.Play();
            footstepTimer = Time.time;
        }
    }

    /// <summary>
    /// Changes the movement speed of this enemy.
    /// </summary>
    /// <param name="newSpeed">The new speed value for this enemy.</param>
    public void SetMovementSpeed(float newSpeed)
    {
        if(navMeshAgent.speed != newSpeed)
            navMeshAgent.speed = newSpeed;
    }

    /// <summary>
    /// Makes this enemy move around the map unless its halted or static.
    /// </summary>
    public void Patrol()
    {
        bool remainOnTarget = false;

        // Stops this enemy when it's seeing something suspicious
        if(halted)
        {
            Halt();
        }

        else if(Time.timeScale != 0 && lastTargetRot != null && IsAtDestination
            && !IsFacingTarget && (moveTimer > 0 || isStatic))
        {
            // Calculates how much the enemy will rotate this frame and updates the rotation
            Vector3 difference = Vector3.up * Mathf.Clamp((float)lastTargetRot - transform.eulerAngles.y, -turnSpeed, turnSpeed);
            transform.rotation = Quaternion.Euler(transform.eulerAngles + difference);

            // Stops rotating once the rotation is reached (static enemies preserve their spawning rotation)
            if(IsFacingTarget && !isStatic)
                RotateTo(null);
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
                // Separates the available targets into a new list
                List<MovementTarget> availableMovementTargets = movementTargets.Where
                    (mt => (mt != null) && (mt.transform.position != lastTargetPos) &&
                        (!mt.Occupied)).ToList();

                // If there's available targets, loops until one is selected
                if(availableMovementTargets.Count() > 0)
                {
                    while(movementTargetIndex < 0 || !availableMovementTargets.Contains
                        (movementTargets[movementTargetIndex]))
                    {
                        // Rolls a random index within the number of available targets and
                        // loops until it gets an available value
                        movementTargetIndex = Random.Range(0, movementTargets.Count());
                    }
                }

                // If there's no available targets to move, remains on the current movement
                // target for half the duration
                else
                    remainOnTarget = true;
            }

            // The enemy is only forced to move once
            if(movingToSetTarget)
                movingToSetTarget = false;

            // Tells this NPC to move towards the movement target of the chosen index
            if(!remainOnTarget)
                movementTargets[movementTargetIndex].Occupy(this);
            else
                movementTargets[movementTargetIndex].Occupy(this, true, 0.5f);
        }

        // Static enemies that arent being forced to move will remain in their spawn position
        else if(isStatic && !movingToSetTarget)
        {
            MoveTo(spawnPos);
        }
    }

    /// <summary>
    /// Tells this enemy to move to a given position.
    /// </summary>
    /// <param name="destination">The position to move this enemy.</param>
    public void MoveTo(Vector3 destination, bool canChooseLastPos = false, float moveTimeMultiplier = 1f)
    {
        if(destination != null && (destination != lastTargetPos || canChooseLastPos) && enemySelf.IsConscious)
        {
            // Moves to the given destination and registers it as the last chosen
            navMeshAgent.SetDestination(destination);
            lastTargetPos = destination;

            // Stops any rotation coroutines
            StopAllCoroutines();

            // Resets the patrol movement cooldown
            moveTimer = Random.Range(minMovementTime, maxMovementTime) * moveTimeMultiplier;
        }
    }

    /// <summary>
    /// Tells this enemy to rotate to a given angle (if null, the enemy just faces forward).
    /// </summary>
    /// <param name="targetRotation">The angle to rotate this enemy towards. If null, the enemy won't rotate.</param>
    public void RotateTo(float? targetRotation)
    {
        if(targetRotation != lastTargetRot && enemySelf.IsConscious)
        {
            lastTargetRot = AdjustRotationAngle(targetRotation);
        }
    }

    /// <summary>
    /// Makes this NPC look at a given position.
    /// </summary>
    public void LookAt(Vector3 position)
    {
        Vector3 lookPos = new Vector3(position.x,
            transform.position.y, position.z);

        transform.LookAt(lookPos);
    }

    /// <summary>
    /// Converts a given angle into the range between 0-360 degrees.
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
    /// Makes this enemy stop in place.
    /// </summary>
    public void Halt()
    {
        MoveTo(transform.position);
        if(!isStatic) RotateTo(null);

        DeoccupyCurrentTarget();
    }

    /// <summary>
    /// Updates the current movement target to no longer be occupied.
    /// </summary>
    public void DeoccupyCurrentTarget()
    {
        if(movementTargetIndex >= 0)
            movementTargets[movementTargetIndex].Deoccupy(this);
    }

    /// <summary>
    /// Picks a random position within the NavMesh on a radius around the given position, and then moves to it.
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
    /// Makes this enemy wander around the area to look for the player.
    /// </summary>
    public void Search()
    {
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

    /// <summary>
    /// Checks if this enemy is stuck somewhere and makes it start searching after a while.
    /// </summary>
    public bool StuckCheck()
    {
        if((transform.position - lastSelfPos).magnitude < (navMeshAgent.speed * Time.deltaTime)
            && enemySelf.IsConscious)
        {
            stuckTimer += Time.deltaTime;

            if(stuckTimer >= stuckTime)
            {
                stuckTimer = 0f;
                return true;
            }

            else return false;
        }

        // Resets the stuck timer if the enemy is not stuck
        else
        {
            stuckTimer = 0f;
            return false;
        }
    }

    /// <summary>
    /// The enemy turns around faster and more often if it's looking for the player.
    /// </summary>
    public void LookAround(bool aggro = false)
    {
        if(IsAtDestination && IsFacingTarget)
        {
            CheckForTurning(aggro);
        }

        // Resets the turn timer with a faster cooldown
        else
        {
            turnTimer = aggroTurnTime;
        }
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
    public Transform CheckNearestPanicButton()
    {
        float distanceToNearestButton = float.MaxValue;
        int chosenButtonIndex = 0;
        UpdateButtonsList();

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

        return mapButtons[chosenButtonIndex].transform;
    }

    /// <summary>
    /// Updates the positions of the panic buttons on the map with a cooldown of 1 second.
    /// </summary>
    private void UpdateButtonsList()
    {
        if(buttonCheckTimer <= 0f)
        {
            mapButtons = FindObjectsByType<PanicButton>(FindObjectsSortMode.None).ToList();
            buttonCheckTimer = 1f;
        }

        else
            buttonCheckTimer -= Time.deltaTime;
    }

    /// <summary>
    /// Updates the list of exits and registers the nearest one.
    /// </summary>
    public Transform CheckNearestExit()
    {
        float distanceToNearestExit = float.MaxValue;
        int chosenExitIndex = 0;
        UpdateExitsList();

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

        return mapEntrances[chosenExitIndex].transform;
    }

    /// <summary>
    /// Updates the positions of the exits on the map with a cooldown of 1 second.
    /// </summary>
    private void UpdateExitsList()
    {
        if(exitCheckTimer <= 0f)
        {
            mapEntrances = FindObjectsByType<MapEntrance>(FindObjectsSortMode.None).ToList();
            exitCheckTimer = 1f;
        }

        else
            exitCheckTimer -= Time.deltaTime;
    }

    /// <summary>
    /// Sets this enemy's movement targets to a given list.
    /// </summary>
    /// <param name="newMovementTargets">The given list of new movement targets.</param>
    public void SetMovementTargets(List<MovementTarget> newMovementTargets)
    {
        if(enemySelf == null) Start();
        
        if(!leavingMap && enemySelf.IsConscious)
        {
            movementTargets = newMovementTargets;
            movementPosTargets = new List<Vector3>();

            if(movementTargets.Count() > 0)
            {
                foreach(MovementTarget mt in movementTargets)
                    movementPosTargets.Add(mt.transform.position);
            }
        }
    }

    /// <summary>
    /// Makes this enemy begin leaving the map if it's conscious.
    /// </summary>
    public void ExitMap()
    {
        if(enemySelf.IsConscious)
        {
            Transform chosenNearestExit = CheckNearestExit();
            leavingMap = true;
            MoveTo(chosenNearestExit.position);
            RotateTo(chosenNearestExit.eulerAngles.y);
        }
    }

    /// <summary>
    /// Enables this enemy's body and disguise interactibles.
    /// </summary>
    public void EnableBody()
    {
        if(!bodyDisguise.enabled && bodyDisguise.HasDisguise)
            bodyDisguise.enabled = true;
            
        else if(bodyDisguise.enabled && !bodyDisguise.HasDisguise)
            bodyDisguise.enabled = false;

        if(!bodyCarry.enabled)
            bodyCarry.enabled = true;
    }

    /// <summary>
    /// When this enemy is knocked out, this code runs only once.
    /// </summary>
    public void GetKnockedOut()
    {
        if(!knockedOut)
        {
            // Stops this enemy
            navMeshAgent.speed = 0f;
            navMeshAgent.enabled = false;
            MoveTo(transform.position);
            RotateTo(transform.eulerAngles.y);
            movementPosTargets = new List<Vector3>{transform.position};

            // Enables the body's detection multiplier
            detectableObject.enabled = true;

            // Stops animations and sounds
            animator.SetBool("KO", true);
            animator.applyRootMotion = false;
            
            // Invokes an event from the inspector when knocked out
            enemySelf.onKnockOut?.Invoke();
            knockedOut = true;
        }
    }
}
