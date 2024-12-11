using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))] 
public class EnemyMovement : MonoBehaviour
{
    public enum Status {Normal, Fleeing, Searching, Chasing, Tased, KnockedOut};

    [SerializeField] private bool isStatic = false;
    [SerializeField] private bool looksAround = true;
    [SerializeField] private List<Transform> movementTargets;
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float minMovementTime = 5f;
    [SerializeField] private float maxMovementTime = 12f;
    [SerializeField] private float minTurnTime = 2f;
    [SerializeField] private float maxTurnTime = 5f;
    [SerializeField] private float aggroTurnTime = 1.5f;
    [SerializeField] private float minSearchTime = 4f;
    [SerializeField] private float maxSearchTime = 10f;
    [SerializeField] private float searchRadius = 20f;
    [SerializeField] private float stuckTime = 3f;
    [SerializeField] protected float tasedTime = 5f;

    private Status status;
    public Status currentStatus {get => status; set {if(status != Status.KnockedOut) status = value;}}
    public bool halted = false;
    public List<Transform> MovementTargets {get => movementTargets;}
    private List<Vector3> movementPosTargets;
    public List<Vector3> MovementPosTargets {get => movementPosTargets;}
    public bool IsStatic {get => isStatic;}

    private float moveTimer;
    private float turnTimer;
    private float searchTimer;
    private float tasedTimer;
    private Vector3 lastTarget;
    public Vector3 LastTarget {get => lastTarget;}
    private Vector3 spawnPos;
    public Vector3 SpawnPos {get=> spawnPos;}
    private Player player;
    private Body body;
    private Animator animator;
    private NavMeshAgent navMeshAgent;
    public bool IsAtDestination {get => (new Vector3(lastTarget.x, 0f, lastTarget.z) -
        new Vector3(transform.position.x, 0f, transform.position.z)).magnitude <= navMeshAgent.stoppingDistance * 3f;}
    private float stuckTimer;
    private Vector3 lastSelfPos;
    private List<MapEntrance> mapEntrances;
    private Vector3 chosenNearestExit;
    private bool leavingMap;
    public bool LeavingMap {get => leavingMap; set{leavingMap = value;}}

    private void Start()
    {
        moveTimer = 0;
        turnTimer = 0;
        searchTimer = 0;
        stuckTimer = 0;
        tasedTimer = tasedTime;
        lastSelfPos = transform.position;
        lastTarget = Vector3.zero;
        spawnPos = transform.position;
        player = FindAnyObjectByType<Player>();
        body = GetComponentInChildren<Body>();
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        mapEntrances = FindObjectsByType<MapEntrance>(FindObjectsSortMode.None).ToList();
        leavingMap = false;

        // Forcefully sets the NavMeshAgent to the NPC type if it isn't already one
        if(navMeshAgent.agentTypeID != -1372625422)
            navMeshAgent.agentTypeID = -1372625422;

        if(isStatic)
        {
            movementTargets = new List<Transform>() {transform};
            movementPosTargets = new List<Vector3>() {spawnPos};
        }

        else
        {
            movementPosTargets = new List<Vector3>();
            foreach(Transform t in movementTargets)
                movementPosTargets.Add(t.position);
        }
    }


    // Checks this enemy's status and moves accordingly.
    private void Update()
    {
        if(leavingMap)
        {
            ExitMap();
        }

        // ------------ Normal ------------ 
        else if(currentStatus == Status.Normal)
        {
            navMeshAgent.speed = walkSpeed;

            Patrol();
        }

        // ------------ Fleeing ------------ 
        else if (currentStatus == Status.Fleeing)
        {
            navMeshAgent.speed = runSpeed;

            ExitMap();
        }
        
        // ------------ Chasing ------------ 
        else if(currentStatus == Status.Chasing)
        {
            navMeshAgent.speed = runSpeed;

            if(GetComponent<EnemyPolice>())
                MoveTo(player.transform.position);

            else
                MoveTo(Detection.lastPlayerPos);

            if((transform.position - lastSelfPos).magnitude < navMeshAgent.speed * Time.deltaTime
                && currentStatus != Status.Tased && currentStatus != Status.KnockedOut)
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

            MoveTo(transform.position);

            if(tasedTimer > 0)
            {
                animator.SetBool("Tased", true);
                animator.applyRootMotion = false;
                tasedTimer -= Time.deltaTime;
            }

            else
            {
                animator.SetBool("Tased", false);
                animator.applyRootMotion = true;

                currentStatus = Status.Normal;
                GetComponent<Enemy>().BecomeAlarmed();
            }
        }

        // ------------ Knocked Out ------------ 
        else if (currentStatus == Status.KnockedOut)
        {
            if(!body.enabled && body.HasDisguise)
                body.enabled = true;
                
            else if(body.enabled && !body.HasDisguise)
                body.enabled = false;

            navMeshAgent.speed = 0f;

            movementPosTargets = new List<Vector3>{transform.position};

            animator.SetBool("KO", true);
            animator.applyRootMotion = false;
        }


        if(currentStatus != Status.Tased && currentStatus != Status.KnockedOut)
        {
            lastSelfPos = transform.position;
        }
    }

    private void Patrol()
    {
        if(halted)
        {
            // -turn towards Detection.lastPlayerPos

            MoveTo(transform.position);
        }

        // Progressively decreases the movement timer if the agent is at its target
        else if(IsAtDestination && moveTimer > 0)
        {
            moveTimer -= Time.deltaTime;

            if(looksAround)
            {
                CheckForTurning();
            }
        }

        if(moveTimer <= 0 && !halted && !isStatic)
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

            // Tells the NPC to move towards the chosen index position
            MoveTo(movementPosTargets[index]);
        }

        else if(isStatic)
        {
            MoveTo(spawnPos);
        }
    }

    public void MoveTo(Vector3 destination)
    {
        if(destination != null && destination != lastTarget && currentStatus != Status.KnockedOut && currentStatus != Status.Tased)
        {
            // Moves to the given destination and registers it as the last chosen
            navMeshAgent.SetDestination(destination);
            lastTarget = destination;

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
            // -turn to another rotation

            if(aggro)
            {
                turnTimer = aggroTurnTime;
            }
            else
            {
                turnTimer = Random.Range(minTurnTime, maxTurnTime);
            }
        }
    }

    public void CheckNearestExit()
    {
        float distanceToNearestExit = float.MaxValue;
        int chosenExitIndex = 0;

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
        if(!leavingMap && currentStatus != Status.KnockedOut && currentStatus != Status.Tased)
        {
            movementTargets = newMovementTargets;
            movementPosTargets = new List<Vector3>();

            foreach(Transform t in movementTargets)
                movementPosTargets.Add(t.position);
        }
    }

    public void ExitMap()
    {
        if(currentStatus != Status.KnockedOut && currentStatus != Status.Tased)
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
            leavingMap = false;
        }
    }

    public void ResetNPC(Vector3 oldSpawnPos)
    {
        if(currentStatus != Status.KnockedOut)
        {
            currentStatus = Status.Normal;
            moveTimer = 0;
            turnTimer = 0;
            searchTimer = 0;
            stuckTimer = 0;
            tasedTimer = tasedTime;
            spawnPos = oldSpawnPos;
            body = GetComponentInChildren<Body>();
            animator = GetComponent<Animator>();
            body.ResetNPC();
            body.enabled = false;
            navMeshAgent = GetComponent<NavMeshAgent>();
            mapEntrances = FindObjectsByType<MapEntrance>(FindObjectsSortMode.None).ToList();
            leavingMap = false;
        }
    }
}
