using System.Linq;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))] 
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private bool isStatic;
    [SerializeField] private Transform[] movementTargets;
    [SerializeField] private float minMovementCooldown = 5f;
    [SerializeField] private float maxMovementCooldown = 12f;

    private float moveTimer;
    private int lastTarget;
    private NavMeshAgent navMeshAgent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveTimer = 0;
        lastTarget = 0;
        navMeshAgent = GetComponent<NavMeshAgent>();

        // Forcefully sets the NavMeshAgent to the NPC type if it isn't already one
        if(navMeshAgent.agentTypeID != -1372625422)
            navMeshAgent.agentTypeID = -1372625422;
    }

    // Update is called once per frame
    void Update()
    {
        // Progressively decreases the movement timer if the agent is at its target
        if((movementTargets[lastTarget].position - transform.position).magnitude < 0.2f && moveTimer > 0)
            moveTimer -= Time.deltaTime;

        if(moveTimer <= 0 && !isStatic)
        {
            // Rolls a random index within the number of available targets and
            // loops until it gets a value different than the last chosen index
            int index = Random.Range(0, movementTargets.Count());

            while(index == lastTarget)
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

            // Moves to the rolled index and registers it as the last chosen
            navMeshAgent.SetDestination(movementTargets[index].position);
            lastTarget = index;

            // Resets the movement cooldown
            moveTimer = Random.Range(minMovementCooldown, maxMovementCooldown);
        }
    }
}
