using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class MapEntrance : MonoBehaviour
{
    [SerializeField] private List<Item> objectiveItems;
    [SerializeField] private float characterDetectionDistance = 4f;
    [SerializeField] private List<GameObject> civillianPrefabs;
    [SerializeField] private float minNPCSpawnTime = 3f;
    [SerializeField] private float maxNPCSpawnTime = 12f;
    [SerializeField] private Transform enemiesGameObject;

    public static Transform Transform;
    private UIManager uiManager;
    private Alarm alarm;
    private Transform player;
    private PlayerInventory playerInventory;
    public static List<Enemy> enemies;
    private Queue<List<Transform>> civilliansMovementTargets;
    private int numOfNPCsInQueue;
    private float NPCspawnTimer;
    private bool PlayerHasAllObjectives {get
    {
        foreach(Item i in objectiveItems)
        {
            if(!playerInventory.HasItem(i))
                return false;
        }

         return true;   
    }}


    private void Start()
    {
        Transform = transform;
        uiManager = FindAnyObjectByType<UIManager>();
        alarm = FindAnyObjectByType<Alarm>();
        numOfNPCsInQueue = 0;
        NPCspawnTimer = Random.Range(minNPCSpawnTime, maxNPCSpawnTime);

        player = FindAnyObjectByType<Player>().transform;
        playerInventory = FindAnyObjectByType<PlayerInventory>();

        // Stores a list of all non-camera enemies to check whenever one wants to leave the map
        enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None).ToList();
        enemies = enemies.Where(e => !e.gameObject.GetComponent<EnemyCamera>()).ToList();

        civilliansMovementTargets = new Queue<List<Transform>>();
    }


    private void Update()
    {
        // If the alarm is off and there are civillians outside of the map, they progressively respawn one by one
        if(numOfNPCsInQueue > 0 && !alarm.IsOn)
        {
            if(NPCspawnTimer > 0)
            {
                NPCspawnTimer -= Time.deltaTime;
            }

            else
            {
                // Picks a random civillian prefab to spawn
                int index = Random.Range(0, civillianPrefabs.Count);
                NavMesh.SamplePosition(transform.position, out NavMeshHit navHit,
                    characterDetectionDistance, NavMesh.AllAreas);

                // Spawns the civillian in the navmesh position nearest to this exit's position and
                // assigns the movement targets of a previous NPC to the new one
                GameObject newNPC = SpawnEnemy(civillianPrefabs[index], civilliansMovementTargets.Dequeue());
                enemies.Add(newNPC.GetComponent<EnemyCivillian>());

                // Decreases the number of NPCs in the queue and restarts the spawn timer
                numOfNPCsInQueue--;
                NPCspawnTimer = Random.Range(minNPCSpawnTime, maxNPCSpawnTime);
            }
        }


        // If the player has the objective in their inventory and comes close to the exit, they complete the mission
        if((transform.position - player.position).magnitude <= characterDetectionDistance)
        {
            if(PlayerHasAllObjectives)
            {
                uiManager.Win();
            }
        }

        // If any civillian comes close to the exit and is running away, they "leave" the map (they despawn until the alarm ends)
        foreach(Enemy e in enemies)
        {
            if((transform.position - e.transform.position).magnitude <= characterDetectionDistance && e.EnemyMovement &&
                (e.EnemyMovement.status == EnemyMovement.Status.Fleeing || e.EnemyMovement.LeavingMap))
            {
                // Tells the game that one more civillian needs to be spawned in and stores
                // the current civillian's movement targets for it
                numOfNPCsInQueue++;
                civilliansMovementTargets.Enqueue(e.EnemyMovement.MovementTargets);

                // Schedules the enemy's GameObject for destruction and removal from the list
                StartCoroutine(ScheduleGameObjectForDestruction(e.gameObject));
            }
        }
    }
    
    /// <summary>
    /// Coroutine that instantly disables the given GameObject and safely destroys it in the next frame.
    /// </summary>
    /// <param name="go">The GameObject to be deleted.</param>
    /// <returns></returns>
    private IEnumerator ScheduleGameObjectForDestruction(GameObject go)
    {
        go.SetActive(false);

        yield return new WaitForEndOfFrame();

        if(go.GetComponent<Enemy>() != null) 
            enemies.Remove(go.GetComponent<Enemy>());

        Destroy(go);
    }

    /// <summary>
    /// Spawns the given prefab at the entrance of the map.
    /// </summary>
    /// <param name="prefab">The prefab to be spawned.</param>
    /// <returns>The spawned GameObject.</returns>
    public GameObject SpawnEnemy(GameObject prefab, List<Transform> movementTargets)
    {
        // Calculates the position in the navmesh to spawn the NPC
        NavMesh.SamplePosition(transform.position, out NavMeshHit navHit,
            characterDetectionDistance, NavMesh.AllAreas);

        // Spawns the NPC in the navmesh position nearest to this exit's position
        GameObject newNPC = Instantiate(prefab, navHit.position, transform.rotation, parent:enemiesGameObject);
        enemies.Add(newNPC.GetComponent<Enemy>());

        newNPC.GetComponent<EnemyMovement>().SetMovementTargets(movementTargets);

        return newNPC;
    }
}
