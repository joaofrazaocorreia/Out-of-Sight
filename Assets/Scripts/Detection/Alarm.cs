using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Alarm : MonoBehaviour
{
    [SerializeField] private float baseDuration = 30f;
    [SerializeField] private float durationIncreasePerTier = 30f;
    [SerializeField] private float maxDuration = 300f;
    [SerializeField] private int maxTier = 3;
    [SerializeField] private int extraGuardsPerTier = 3;
    [SerializeField] private int maxGuardsRemainPermanent = 2;
    [SerializeField] private List<MovementTarget> movementTargets;
    [SerializeField] private GameObject guardPrefab;
    [SerializeField] private GameObject policePrefab;
    [SerializeField] private float policeSpawnTime = 10f;
    [SerializeField] private int maxPoliceNPCS = 6;
    [SerializeField] private GameObject paramedicPrefab;
    [SerializeField] private float paramedicSpawnTime = 2.5f;
    [SerializeField] private PlayAudio alarmLoopPlayer;
    [SerializeField] private PlayAudio alarmEndPlayer;
    [SerializeField] private MusicPlayer musicPlayer;

    private Transform player;
    private MovementTarget playerTargetPos;
    private int currentTier;
    private bool isOn;
    public bool IsOn {get => isOn; set => isOn = value;}
    public bool forceDisable;
    private float alarmTimer;
    private float AlarmTime {get => baseDuration + (durationIncreasePerTier * Mathf.Min(currentTier - 1, maxTier));}
    public float AlarmTimer {get => alarmTimer; set => alarmTimer = value;}
    private float alarmTimeLimit;
    private float policeSpawnTimer;
    private float paramedicSpawnTimer;
    private List<Enemy> allEnemies;
    private List<Enemy> nonCameraEnemies;
    private List<Enemy> extraGuards;
    private List<Enemy> policeGuards;
    private List<Enemy> paramedics;
    private List<BodyCarry> seenBodies;

    private void Start()
    {
        currentTier = 0;
        isOn = false;
        forceDisable = false;
        alarmTimer = AlarmTime;
        alarmTimeLimit = maxDuration;
        policeSpawnTimer = 0f;
        paramedicSpawnTimer = 0f;
        allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None).ToList();
        nonCameraEnemies = allEnemies.Where(e => !e.GetComponent<EnemyCamera>()).ToList();
        extraGuards = new List<Enemy>();
        policeGuards = new List<Enemy>();
        paramedics = new List<Enemy>();
        seenBodies = new List<BodyCarry>();

        player = FindAnyObjectByType<Player>().transform;
        playerTargetPos = MovementTarget.CreateMovementTarget(
            Vector3.zero, Quaternion.Euler(Vector3.zero), player.transform);
        playerTargetPos.enabled = false;
    }

    private void Update()
    {
        if(isOn)
        {
            if(alarmTimer > 0 && !forceDisable)
            {
                // Decreases the alarm timer if it's not the maximum alarm tier.
                if(currentTier < maxTier)
                {
                    alarmTimer -= Time.deltaTime;
                    alarmTimeLimit -= Time.deltaTime;

                    // If the alarm is still active after reaching its time limit, the alarm tier is maxxed instantly
                    if(alarmTimeLimit <= 0)
                    {
                        Debug.Log("alarm time limit reached - maxxed out the alarm tier");
                        currentTier = maxTier;
                    }
                }

                // If it's the maximum tier, starts generating police NPCs procedurally until the maximum amount is hit.
                else if(policeGuards.Count < maxPoliceNPCS)
                {
                    if(policeSpawnTimer <= 0)
                    {
                        policeSpawnTimer = policeSpawnTime;
                        EnemySpawner spawner = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None)
                            .Where(s => s.EnemyType == Enemy.Type.Police).First();
                        Enemy newPolice = spawner.SpawnEnemy(policePrefab, new List<MovementTarget>() {playerTargetPos});

                        policeGuards.Add(newPolice);
                    }

                    else
                    {
                        policeSpawnTimer -= Time.deltaTime;
                    }
                }
            }

            // When the duration ends, the alarm turns off and any extra spawned enemies leave the map.
            // Some enemies can remain on the map depending on the maximum amount set.
            else
            {
                Debug.Log("Alarm turned off!");
                isOn = false;
                forceDisable = false;
                player.GetComponent<NavMeshObstacle>().enabled = true;

                foreach(Enemy e in allEnemies)
                {
                    e.Detection.DetectionMeter = 0f;
                    e.BecomeNormal(true);
                    e.BecomeCurious();
                }
      
                int enemyCount = 0;
                List<Enemy> enemiesToRemove = new List<Enemy>();
                foreach(Enemy g in extraGuards)
                {
                    if(++enemyCount > maxGuardsRemainPermanent)
                    {
                        enemiesToRemove.Add(g);

                        g.EnemyMovement.ExitMap();
                    }
                }

                // Safely removes the guards from the extra guards list after the previous foreach loop ends
                // (to prevent altering the list while it's being checked by the foreach loop)
                foreach (Enemy g in enemiesToRemove)
                {
                    extraGuards.Remove(g);
                }
                
                alarmLoopPlayer.Stop();
                //alarmEndPlayer.Play();
                musicPlayer.SwitchTrack();
            }
        }

        // If the alarm is off, spawns a paramedic for each body seen.
        else if(seenBodies.Count() > 0)
        {
            if(paramedicSpawnTimer <= 0)
            {
                int index = Random.Range(0, seenBodies.Count());
                paramedicSpawnTimer = paramedicSpawnTime;

                MovementTarget bodyTarget = MovementTarget.CreateMovementTarget
                    (seenBodies[index].transform.position, seenBodies[index].transform.rotation
                        * Quaternion.Euler(0f, 180f, 0f), seenBodies[index].transform);

                EnemySpawner spawner = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None)
                    .Where(s => s.EnemyType == Enemy.Type.Paramedic).First();
                    
                Enemy newParamedic = spawner.SpawnEnemy(paramedicPrefab, new
                    List<MovementTarget>(){bodyTarget});

                paramedics.Add(newParamedic);
                (newParamedic as EnemyParamedic).BodyTarget = seenBodies[index];
                seenBodies.Remove(seenBodies[index]);
            }

            else
            {
                paramedicSpawnTimer -= Time.deltaTime;
            }
        }
    }

    /// <summary>
    /// Public method to trigger the alarm globally and begin/restart its timer.
    /// </summary>
    public void TriggerAlarm(bool alertEnemies)
    {
        // Whenever the alarm is raised, its tier increases and other effects apply depending on it, and
        // all NPCS become alarmed.
        if(!isOn)
        {
            Debug.Log("Alarm triggered!");

            isOn = true;
            ++currentTier;
            alarmTimeLimit = maxDuration;
            player.GetComponent<NavMeshObstacle>().enabled = false;

            // Every time the alarm is raised, global detection is raised by 10%
            if(currentTier >= 1)
            {
                Detection.globalDetectionMultiplier += 0.1f;
            }

            // Whenever the alarm is raised starting at the second time, extra guards spawn at the entrance of the level
            if(currentTier >= 1)
            {
                for(int i = 0; i < (extraGuardsPerTier * currentTier); i++)
                {
                    EnemySpawner spawner = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None)
                        .Where(s => s.EnemyType == Enemy.Type.Guard).First();
                    Enemy newGuard = spawner.SpawnEnemy(guardPrefab, movementTargets);
                    extraGuards.Add(newGuard);

                    newGuard.GetComponent<Enemy>().EnemyStatus = Enemy.Status.Chasing;
                }
            }

            if(currentTier == maxTier) playerTargetPos.enabled = true;
            
            alarmLoopPlayer.Play();
            musicPlayer.SwitchTrack();
        }

        allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None).ToList();
        nonCameraEnemies = allEnemies.Where(e => !e.GetComponent<EnemyCamera>()).ToList();
        
        // NPCs become aware of the bodies in the level so they don't raise the alarm again
        foreach(Enemy e in nonCameraEnemies)
        {
            if(e.GetComponent<Enemy>().EnemyStatus == Enemy.Status.KnockedOut)
            {
                BodyCarry body = e.GetComponentInChildren<BodyCarry>();

                if(body != null && !body.HasBeenDetected)
                {
                    body.HasBeenDetected = true;
                    seenBodies.Add(body);
                }
            }
        }

        // Alerts all enemies in the level
        if(alertEnemies)
        {
            foreach(Enemy e in nonCameraEnemies)
            {
                e.BecomeAlarmed();
            }
        }

        alarmTimer = AlarmTime;
    }

    public void RegisterEnemy(Enemy enemy)
    {
        if(!allEnemies.Contains(enemy))
            allEnemies.Add(enemy);
    }
    public void UnregisterEnemy(Enemy enemy)
    {
        if(allEnemies.Contains(enemy))
            allEnemies.Remove(enemy);
    }
}
