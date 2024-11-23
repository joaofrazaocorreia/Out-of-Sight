using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class MapEntrance : MonoBehaviour
{
    [SerializeField] private float characterDetectionDistance = 4f;
    [SerializeField] private List<GameObject> civillianPrefabs;
    [SerializeField] private float minNPCSpawnTime = 3f;
    [SerializeField] private float maxNPCSpawnTime = 12f;

    private Transform player;
    private List<EnemyCivillian> civillians;
    private Queue<List<Transform>> civilliansMovementTargets;
    private int numOfNPCsInQueue;
    private float NPCspawnTimer;


    private void Start()
    {
        numOfNPCsInQueue = 0;
        NPCspawnTimer = Random.Range(minNPCSpawnTime, maxNPCSpawnTime);

        player = FindAnyObjectByType<Player>().transform;

        civillians = new List<EnemyCivillian>();
        foreach(EnemyCivillian c in FindObjectsByType<EnemyCivillian>(FindObjectsSortMode.None))
        {
            civillians.Add(c);
        }

        civilliansMovementTargets = new Queue<List<Transform>>();
    }


    private void Update()
    {
        // If the alarm is off and there are civillians outside of the map, they progressively respawn one by one
        if(numOfNPCsInQueue > 0) // && !alarm.isOn)
        {
            if(NPCspawnTimer > 0)
            {
                NPCspawnTimer -= Time.deltaTime;
            }

            else
            {
                // Picks a random civillian prefab to spawn and calculates the position in the navmesh to spawn it
                int index = Random.Range(0, civillianPrefabs.Count);
                NavMesh.SamplePosition(transform.position, out NavMeshHit navHit,
                    characterDetectionDistance, NavMesh.AllAreas);

                // Spawns the civillian in the navmesh position nearest to this exit's position
                GameObject newNPC = Instantiate(civillianPrefabs[index], navHit.position, transform.rotation);
                civillians.Add(newNPC.GetComponent<EnemyCivillian>());

                // Assigns the movement targets of a previous NPC to the new one
                newNPC.GetComponent<EnemyMovement>().SetMovementTargets(civilliansMovementTargets.First());
                civilliansMovementTargets.Dequeue();


                // Decreases the number of NPCs in the queue and restarts the spawn timer
                numOfNPCsInQueue--;
                NPCspawnTimer = Random.Range(minNPCSpawnTime, maxNPCSpawnTime);
            }
        }


        // If the player has the objective in their inventory and comes close to the exit, they complete the mission
        if((transform.position - player.position).magnitude <= characterDetectionDistance)
        {
            // check if player has the Objective Item in inventory to trigger Game Victory screen
        }

        // If any civillian comes close to the exit and is running away, they "leave" the map (they despawn until the alarm ends)
        foreach(EnemyCivillian c in civillians)
        {
            if(c.EnemyMovement == null) Debug.Log("c.EnemyMovement is null");
            if((transform.position - c.transform.position).magnitude <= characterDetectionDistance && c.EnemyMovement &&
                (c.EnemyMovement.status == EnemyMovement.Status.Fleeing ||
                    c.EnemyMovement.status == EnemyMovement.Status.Scared))
            {

                // Tells the game that one more civillian needs to be spawned in and stores
                // the current civillian's movement targets for it
                numOfNPCsInQueue++;
                civilliansMovementTargets.Enqueue(c.EnemyMovement.MovementTargets);

                // Schedules the civillian's GameObject for destruction and removal from the list
                StartCoroutine(ScheduleGameObjectForDestruction(c.gameObject));
            }
        }
    }
    
    /// <summary>
    /// Coroutine that instantly disables the given GameObject and safely destroys it in the next frame;
    /// If the GameObject is a civillian, it is removed from the civillians list.
    /// </summary>
    /// <param name="go">The GameObject to be deleted.</param>
    /// <returns></returns>
    private IEnumerator ScheduleGameObjectForDestruction(GameObject go)
    {
        go.SetActive(false);

        yield return new WaitForEndOfFrame();

        if(go.GetComponent<EnemyCivillian>() != null) 
            civillians.Remove(go.GetComponent<EnemyCivillian>());
        Destroy(go);
    }
}
