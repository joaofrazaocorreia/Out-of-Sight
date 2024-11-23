using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))] 
public class EnemyMovement : MonoBehaviour
{
    public enum Status {Normal, Scared, Fleeing, Searching, Chasing, Tased, KnockedOut};

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

    public Status status;
    public bool halted = false;
    public List<Transform> MovementTargets {get => movementTargets;}

    private float moveTimer;
    private float turnTimer;
    private float searchTimer;
    private Vector3 lastTarget;
    public Vector3 LastTarget {get => lastTarget;}
    private Vector3 spawnPos;
    private NavMeshAgent navMeshAgent;
    public bool IsAtDestination {get => (new Vector3(lastTarget.x, 0f, lastTarget.z) -
        new Vector3(transform.position.x, 0f, transform.position.z)).magnitude <= navMeshAgent.stoppingDistance;}
    private float stuckTimer;
    private Vector3 lastSelfPos;
    private List<MapEntrance> mapEntrances;
    private Vector3 chosenNearestExit;

    private void Start()
    {
        moveTimer = 0;
        turnTimer = 0;
        searchTimer = 0;
        stuckTimer = 0;
        lastSelfPos = transform.position;
        lastTarget = Vector3.zero;
        spawnPos = transform.position;
        navMeshAgent = GetComponent<NavMeshAgent>();
        mapEntrances = FindObjectsByType<MapEntrance>(FindObjectsSortMode.None).ToList();

        // Forcefully sets the NavMeshAgent to the NPC type if it isn't already one
        if(navMeshAgent.agentTypeID != -1372625422)
            navMeshAgent.agentTypeID = -1372625422;
    }


    // Checks this enemy's status and moves accordingly.
    private void Update()
    {

        // ------------ Normal ------------ 
        if(status == Status.Normal)
        {
            navMeshAgent.speed = walkSpeed;

            Patrol();
        }

        // ------------ Scared ------------ 
        else if (status == Status.Scared)
        {
            navMeshAgent.speed = runSpeed;

            Vector3 direction = transform.position - Detection.lastPlayerPos;

            if (NavMesh.SamplePosition(transform.position + direction, out NavMeshHit navHit, direction.magnitude, NavMesh.AllAreas))
                MoveTo(navHit.position);
        }

        // ------------ Fleeing ------------ 
        else if (status == Status.Fleeing)
        {
            navMeshAgent.speed = runSpeed;

            MoveTo(chosenNearestExit);
        }
        
        // ------------ Chasing ------------ 
        else if(status == Status.Chasing)
        {
            navMeshAgent.speed = runSpeed;

            MoveTo(Detection.lastPlayerPos);

            if((transform.position - lastSelfPos).magnitude < navMeshAgent.speed * Time.deltaTime
                && status != Status.Tased && status != Status.KnockedOut)
            {
                stuckTimer += Time.deltaTime;

                if(stuckTimer >= stuckTime)
                {
                    MoveTo(transform.position);
                    status = Status.Searching;
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
        else if (status == Status.Searching)
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
        else if (status == Status.Tased)
        {
            navMeshAgent.speed = 0f;

            movementTargets = new List<Transform>{transform};

            // -animator.SetBool("Ragdoll", true);
        }

        // ------------ Knocked Out ------------ 
        else if (status == Status.KnockedOut)
        {
            navMeshAgent.speed = 0f;

            movementTargets = new List<Transform>{transform};

            // -animator.SetBool("Ragdoll", true);
        }


        if(status != Status.Tased && status != Status.KnockedOut)
        {
            lastSelfPos = transform.position;
        }
    }

    private void Patrol()
    {
        // Progressively decreases the movement timer if the agent is at its target
        if(IsAtDestination && moveTimer > 0)
        {
            moveTimer -= Time.deltaTime;

            if(looksAround)
            {
                CheckForTurning();
            }
        }

        else if(halted)
        {
            // -turn towards Detection.lastPlayerPos

            MoveTo(transform.position);
        }

        if(moveTimer <= 0 && !halted && !isStatic)
        {
            // Rolls a random index within the number of available targets and
            // loops until it gets a value different than the last chosen index
            int index = Random.Range(0, movementTargets.Count());

            while(movementTargets[index].position == lastTarget)
            {
                int loop = 0;
                index = Random.Range(0, movementTargets.Count());

                // If it loops for too long, breaks out of the loop
                if (loop == 100)
                {
                    index = 0;
                    break;
                }
            }

            // Tells the NPC to move towards the chosen index position
            MoveTo(movementTargets[index].position);
        }

        else if(isStatic)
        {
            MoveTo(spawnPos);
        }
    }

    public void MoveTo(Vector3 destination)
    {
        if(destination != null && destination != lastTarget)
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
        movementTargets = newMovementTargets;
    }
}
