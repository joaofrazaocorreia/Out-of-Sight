using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Interaction;
using UnityEngine;
using UnityEngine.AI;

public class MapEntrance : MonoBehaviour
{
    [SerializeField] private List<ItemType> objectiveItems;
    [SerializeField] private float playerDetectionDistance = 6f;
    [SerializeField] private float npcDetectionDistance = 4f;
    [SerializeField] private float minCivillianSpawnTime = 3f;
    [SerializeField] private float maxCivillianSpawnTime = 15f;
    [SerializeField] private float minWorkerSpawnTime = 2f;
    [SerializeField] private float maxWorkerSpawnTime = 10f;
    [SerializeField] private Transform enemiesGameObject;
    [SerializeField] private Transform QueuedCivilliansParent;
    [SerializeField] private Transform QueuedWorkersParent;

    public static Transform Transform;
    private UIManager uiManager;
    private Alarm alarm;
    private Transform player;
    private PlayerInventory playerInventory;
    public static List<Enemy> enemies;
    private int numOfCivilliansInQueue;
    private int numOfWorkersInQueue;
    private float civilliansSpawnTimer;
    private float workersSpawnTimer;
    private bool PlayerHasAllObjectives {get
    {
        foreach(ItemType i in objectiveItems)
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
        QueuedCivilliansParent.gameObject.SetActive(true);
        QueuedWorkersParent.gameObject.SetActive(true);
        numOfCivilliansInQueue = 0;
        numOfWorkersInQueue = 0;
        workersSpawnTimer = Random.Range(minWorkerSpawnTime, maxWorkerSpawnTime);
        civilliansSpawnTimer = Random.Range(minCivillianSpawnTime, maxCivillianSpawnTime);

        player = FindAnyObjectByType<Player>().transform;
        playerInventory = FindAnyObjectByType<PlayerInventory>();

        UpdateEnemies();
    }


    private void Update()
    {
        // If the alarm is off and there are workers outside of the map, they progressively respawn one by one
        if(numOfWorkersInQueue > 0 && !alarm.IsOn)
        {
            if(workersSpawnTimer > 0)
            {
                workersSpawnTimer -= Time.deltaTime;
            }

            else
            {
                // Respawns a worker in this exit's position
                GameObject newNPC = SpawnEnemy(Enemy.Type.Worker);
                enemies.Add(newNPC.GetComponent<EnemyWorker>());

                // Decreases the number of NPCs in the queue and restarts the spawn timer
                numOfWorkersInQueue--;
                workersSpawnTimer = Random.Range(minWorkerSpawnTime, maxWorkerSpawnTime);
            }
        }

        // If the alarm is off and there are civillians outside of the map, they progressively respawn one by one
        if(numOfCivilliansInQueue > 0 && !alarm.IsOn)
        {
            if(civilliansSpawnTimer > 0)
            {
                civilliansSpawnTimer -= Time.deltaTime;
            }

            else
            {
                // Spawns the civillian in this exit's position
                GameObject newNPC = SpawnEnemy(Enemy.Type.Civillian);
                enemies.Add(newNPC.GetComponent<EnemyCivillian>());

                // Decreases the number of NPCs in the queue and restarts the spawn timer
                numOfCivilliansInQueue--;
                civilliansSpawnTimer = Random.Range(minCivillianSpawnTime, maxCivillianSpawnTime);
            }
        }


        // If the player has the objective in their inventory and comes close to the exit, they complete the mission
        if((transform.position - player.position).magnitude <= playerDetectionDistance)
        {
            if(PlayerHasAllObjectives)
            {
                uiManager.Win();
            }
        }

        // If any civillian comes close to the exit and is running away, they "leave" the map (they despawn until the alarm ends)
        UpdateEnemies();
        foreach(Enemy e in enemies)
        {
            if((transform.position - e.transform.position).magnitude <= npcDetectionDistance && e.EnemyMovement &&
                (e.EnemyMovement.currentStatus == EnemyMovement.Status.Fleeing || e.EnemyMovement.LeavingMap))
            {
                // Tells the game that one more civillian needs to be spawned in and stores
                // the current civillian's movement targets for it
                if(e.GetComponent<EnemyWorker>())
                {
                    numOfWorkersInQueue++;
                }
                else if(e.GetComponent<EnemyCivillian>())
                {
                    numOfCivilliansInQueue++;
                }

                // Schedules the enemy's GameObject for destruction and removal from the list
                StartCoroutine(ScheduleGameObjectForQueue(e.gameObject, e.EnemyType));
            }
        }
    }
    
    /// <summary>
    /// Coroutine that instantly disables the given GameObject and safely stores it in the next frame.
    /// </summary>
    /// <param name="go">The GameObject to be deleted.</param>
    /// <returns></returns>
    private IEnumerator ScheduleGameObjectForQueue(GameObject go, Enemy.Type type)
    {
        go.SetActive(false);

        yield return new WaitForEndOfFrame();

        if(go.GetComponent<Enemy>() != null) 
            enemies.Remove(go.GetComponent<Enemy>());

        if(type == Enemy.Type.Civillian)
            go.transform.parent = QueuedCivilliansParent;

        else if(type == Enemy.Type.Worker)
            go.transform.parent = QueuedWorkersParent;
        
        else Destroy(go);
        
        UpdateEnemies();
    }

    /// <summary>
    /// Spawns the given prefab at the entrance of the map with the given movement targets.
    /// </summary>
    /// <param name="prefab">The prefab to be spawned.</param>
    /// <returns>The spawned GameObject.</returns>
    public GameObject SpawnEnemy(GameObject prefab, List<Transform> movementTargets)
    {
        // Calculates the position in the navmesh to spawn the NPC
        NavMesh.SamplePosition(transform.position, out NavMeshHit navHit,
            npcDetectionDistance, NavMesh.AllAreas);

        // Spawns the NPC in the navmesh position nearest to this exit's position
        GameObject newNPC = Instantiate(prefab, navHit.position, transform.rotation, parent:enemiesGameObject);
        enemies.Add(newNPC.GetComponent<Enemy>());

        newNPC.GetComponent<EnemyMovement>().SetMovementTargets(movementTargets);
        UpdateEnemies();

        return newNPC;
    }

    /// <summary>
    /// Respawns the given type of enemy at the entrance of the map.
    /// </summary>
    /// <param name="prefab">The prefab to be spawned.</param>
    /// <returns>The spawned GameObject.</returns>
    public GameObject SpawnEnemy(Enemy.Type type)
    {
        Transform QueuedEnemiesGameObject;
        if(type == Enemy.Type.Civillian)
            QueuedEnemiesGameObject = QueuedCivilliansParent;
        
        else
            QueuedEnemiesGameObject = QueuedWorkersParent;
        
        // Calculates the position in the navmesh to spawn the NPC
        NavMesh.SamplePosition(transform.position, out NavMeshHit navHit,
            npcDetectionDistance, NavMesh.AllAreas);

        // Respawns a random NPC of the given type in the navmesh position nearest to this exit's position
        int index = Random.Range(0, QueuedEnemiesGameObject.childCount);
        Enemy newNPC = Instantiate(QueuedEnemiesGameObject.GetChild(index), navHit.position, Quaternion.identity, enemiesGameObject).GetComponent<Enemy>();
        enemies.Add(newNPC);

        newNPC.transform.parent = enemiesGameObject;
        newNPC.transform.position = navHit.position;
        newNPC.ResetNPC();
        newNPC.EnemyMovement.ResetNPC(QueuedEnemiesGameObject.GetChild(index).GetComponent<EnemyMovement>().SpawnPos);
        newNPC.gameObject.SetActive(true);

        Destroy(QueuedEnemiesGameObject.GetChild(index).gameObject);

        return newNPC.gameObject;
    }

    /// <summary>
    /// Stores a list of all non-camera enemies to check whenever one wants to leave the map.
    /// </summary>
    public static void UpdateEnemies()
    {
        enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None).ToList();
        enemies = enemies.Where(e => !e.gameObject.GetComponent<EnemyCamera>()).ToList();
    }
}
