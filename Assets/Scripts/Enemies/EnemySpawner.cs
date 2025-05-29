using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Enemy.Type enemyType;
    [SerializeField] private float npcDetectionDistance = 4f;
    [SerializeField] private bool respawnsEnemies = true;
    [SerializeField] private float minSpawnTime = 3f;
    [SerializeField] private float maxSpawnTime = 15f;
    [SerializeField] private Transform enemiesGameObject;
    [SerializeField] private Transform queuedNPCsParent;

    public Enemy.Type EnemyType {get => enemyType;}
    private Alarm alarm;
    public static List<Enemy> enemies;
    private int numOfSpawnsInQueue;
    private float spawnTimer;


    private void Start()
    {
        alarm = FindAnyObjectByType<Alarm>();
        queuedNPCsParent.gameObject.SetActive(true);
        numOfSpawnsInQueue = 0;
        spawnTimer = Random.Range(minSpawnTime, maxSpawnTime);

        UpdateEnemies();
    }


    private void Update()
    {
        // If the alarm is off and there are workers outside of the map, they progressively respawn one by one
        if(respawnsEnemies && numOfSpawnsInQueue > 0 && !alarm.IsOn)
        {
            if(spawnTimer > 0)
            {
                spawnTimer -= Time.deltaTime;
            }

            else
            {
                // Respawns an NPC in this exit's position
                Enemy newNPC = SpawnEnemy();
                Debug.Log($"Respawned {enemyType} ({newNPC.name})");
            }
        }

        // If any NPC of the chosen type comes close to the exit and is leaving, they get despawned
        UpdateEnemies();
        foreach(Enemy e in enemies)
        {
            if((transform.position - e.transform.position).magnitude <= npcDetectionDistance && 
                e.EnemyMovement != null && (e.EnemyStatus == Enemy.Status.Fleeing ||
                    e.EnemyMovement.LeavingMap) && e.EnemyType == EnemyType)
            {
                // Only schedules a respawn if set in the inspector
                if(respawnsEnemies)
                {
                    // Tells the game that one more NPC needs to be spawned in later
                    numOfSpawnsInQueue++;

                    // Schedules the enemy's GameObject for destruction and removal from the list
                    StartCoroutine(ScheduleEnemyForQueue(e));
                }
                
                // If this class doesn't respawn the enemies of the given type, deletes them instead
                else
                {
                    alarm.UnregisterEnemy(e);
                    Destroy(e.gameObject);
                }
            }
        }
    }

    /// <summary>
    /// Coroutine that instantly despawns the given enemy, then safely stores it in the next frame.
    /// </summary>
    /// <param name="enemy">The enemy to despawn.</param>
    /// <returns></returns>
    private IEnumerator ScheduleEnemyForQueue(Enemy enemy)
    {
        alarm.UnregisterEnemy(enemy);
        enemy.gameObject.SetActive(false);

        yield return new WaitForEndOfFrame();

        enemy.transform.parent = queuedNPCsParent;
        enemies.Remove(enemy);
            
        if(enemy.IsAlarmed && !alarm.IsOn)
            alarm.TriggerAlarm(alarm.IsOn);

        UpdateEnemies();
    }

    /// <summary>
    /// Spawns a given prefab at the entrance of the map with the given movement targets.
    /// </summary>
    /// <param name="prefab">The prefab to be spawned.</param>
    /// <returns>The spawned GameObject.</returns>
    public Enemy SpawnEnemy(GameObject prefab, List<MovementTarget> movementTargets)
    {
        Debug.Log($"Spawning {prefab.name} by Prefab...");

        // Calculates the navHit position in the navmesh to spawn the NPC
        NavMesh.SamplePosition(transform.position, out NavMeshHit navHit,
            npcDetectionDistance, NavMesh.AllAreas);

        // Spawns the NPC in the navmesh position nearest to this exit's position
        Enemy newNPC = Instantiate(prefab, navHit.position, transform.rotation, parent:enemiesGameObject).GetComponent<Enemy>();
        enemies.Add(newNPC);
        alarm.RegisterEnemy(newNPC);

        numOfSpawnsInQueue--;
        spawnTimer = Random.Range(minSpawnTime, maxSpawnTime);

        newNPC.GetComponent<EnemyMovement>().SetMovementTargets(movementTargets);
        UpdateEnemies();

        if(alarm.IsOn) newNPC.BecomeAlarmed();
        return newNPC;
    }

    /// <summary>
    /// Respawns this class's chosen type of enemy at the entrance of the map.
    /// </summary>
    /// <returns>The spawned GameObject.</returns>
    public Enemy SpawnEnemy()
    {
        Debug.Log($"Spawning {enemyType} by Type...");
        
        // Chooses a random queued NPC to spawn
        int index = Random.Range(0, queuedNPCsParent.childCount);

        // Calculates the navHit position in the navmesh to spawn the NPC
        NavMesh.SamplePosition(transform.position, out NavMeshHit navHit,
            npcDetectionDistance, NavMesh.AllAreas);

        // Respawns the NPC in the navmesh position nearest to this spawner's position
        Enemy newNPC = Instantiate(queuedNPCsParent.GetChild(index), navHit.position,
            Quaternion.identity, enemiesGameObject).GetComponent<Enemy>();

        // Registers and sets up the enemy 
        enemies.Add(newNPC);
        alarm.RegisterEnemy(newNPC);
        newNPC.transform.parent = enemiesGameObject;
        numOfSpawnsInQueue--;
        spawnTimer = Random.Range(minSpawnTime, maxSpawnTime);

        newNPC.name = queuedNPCsParent.GetChild(index).name;
        newNPC.transform.position = navHit.position;
        newNPC.ResetNPC();
        newNPC.EnemyMovement.ResetNPC(queuedNPCsParent.GetChild(index).GetComponent<EnemyMovement>().SpawnPos,
            queuedNPCsParent.GetChild(index).GetComponent<EnemyMovement>().SpawnRot);
        newNPC.gameObject.SetActive(true);

        Destroy(queuedNPCsParent.GetChild(index).gameObject);

        if(alarm.IsOn) newNPC.BecomeAlarmed();
        return newNPC;
    }

    /// <summary>
    /// Stores a list of all non-camera enemies to check whenever one wants to leave the map.
    /// </summary>
    public static void UpdateEnemies()
    {
        enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None).Where
            (e => !e.gameObject.GetComponent<EnemyCamera>()).ToList();
    }
}