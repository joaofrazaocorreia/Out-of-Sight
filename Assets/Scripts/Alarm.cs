using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Alarm : MonoBehaviour
{
    [SerializeField] private float baseDuration = 30f;
    [SerializeField] private float durationIncreasePerTier = 30f;
    [SerializeField] private int maxTier = 3;
    [SerializeField] private int extraGuardsPerTier = 3;
    [SerializeField] private int maxGuardsRemainPermanent = 2;
    [SerializeField] private MapEntrance mapEntrance;
    [SerializeField] private List<Transform> movementTargets;
    [SerializeField] private GameObject guardPrefab;
    //[SerializeField] private GameObject policePrefab;
    //[SerializeField] private float policeSpawnTime = 10f;
    //[SerializeField] private int maxPoliceNPCS = 6;

    private Transform player;
    private int currentTier;
    private bool isOn;
    public bool IsOn {get => isOn; set => isOn = value;}
    private float alarmTimer;
    private float AlarmTime {get => baseDuration + (durationIncreasePerTier * Mathf.Min(currentTier - 1, maxTier));}
    public float AlarmTimer {get => alarmTimer; set => alarmTimer = value;}
    //private float policeSpawnTimer;
    private List<Enemy> enemies;
    private List<EnemyGuard> nonStaticGuards;
    private List<EnemyGuard> extraGuards;
    //private List<EnemyPolice> policeGuards;

    private void Start()
    {
        currentTier = 0;
        isOn = false;
        alarmTimer = AlarmTime;
        //policeSpawnTimer = 0f;
        enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None).ToList();
        enemies = enemies.Where(e => !e.GetComponent<EnemyCamera>()).ToList();
        extraGuards = new List<EnemyGuard>();
        nonStaticGuards = FindObjectsByType<EnemyGuard>(FindObjectsSortMode.None).ToList();
        nonStaticGuards = nonStaticGuards.Where(enemy => !enemy.GetComponent<EnemyMovement>().IsStatic).ToList();

        player = FindAnyObjectByType<Player>().transform;
    }

    private void Update()
    {
        if(isOn)
        {
            if(alarmTimer > 0)
            {
                // Decreases the alarm timer if it's not the maximum alarm tier.
                if(currentTier < maxTier)
                {
                    alarmTimer -= Time.deltaTime;
                }

                /*
                // If it's the last tier, starts generating police NPCs procedurally until the maximum amount is hit.
                else if(policeGuards.Count < maxPoliceNPCS)
                {
                    if(policeSpawnTimer <= 0)
                    {
                        policeSpawnTimer = policeSpawnTime;
                        GameObject newPolice = mapEntrance.SpawnEnemy(policePrefab, player);

                        policeGuards.Add(newPolice.GetComponent<EnemyPolice>());
                    }

                    else
                    {
                        policeSpawnTimer -= Time.deltaTime;
                    }
                }
                */
            }

            // When the duration ends, the alarm turns off and any extra spawned enemies leave the map.
            // Some enemies can remain on the map depending on the maximum amount set.
            else
            {
                Debug.Log("Alarm turned off!");
                isOn = false;
      
                int enemyCount = 0;
                List<EnemyGuard> enemiesToRemove = new List<EnemyGuard>();
                foreach(EnemyGuard g in extraGuards)
                {
                    if(++enemyCount > maxGuardsRemainPermanent)
                    {
                        enemiesToRemove.Add(g);

                        g.EnemyMovement.ExitMap();
                    }
                }

                // Safely removes the guards from the extra guards list after the previous foreach loop ends
                // (to prevent altering the list while it's being checked by the foreach loop)
                foreach (EnemyGuard g in enemiesToRemove)
                {
                    extraGuards.Remove(g);
                }
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

            // Every time the alarm is raised, global detection is raised by 10%
            if(currentTier > 0)
            {
                Detection.globalDetectionMultiplier += 0.1f;
            }

            // Whenever the alarm is raised after the second time, extra guards spawn at the entrance of the level
            if(currentTier > 1)
            {
                for(int i = 0; i < (extraGuardsPerTier * currentTier - 1); i++)
                {
                    GameObject newGuard = mapEntrance.SpawnEnemy(guardPrefab, movementTargets);
                    extraGuards.Add(newGuard.GetComponent<EnemyGuard>());

                    newGuard.GetComponent<EnemyMovement>().status = EnemyMovement.Status.Chasing;
                }
            }
        }

        enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None).ToList();
        enemies = enemies.Where(e => !e.GetComponent<EnemyCamera>()).ToList();
        
        // NPCs become aware of the bodies in the level so they don't raise the alarm right after it ends
        foreach(Enemy e in enemies)
        {
            if(e.GetComponent<EnemyMovement>().status == EnemyMovement.Status.KnockedOut)
                e.GetComponentInChildren<Body>().HasBeenDetected = true;
        }

        // Alerts all enemies in the level;
        if(alertEnemies)
        {
            foreach(Enemy e in enemies)
            {
                e.BecomeAlarmed();
            }
        }

        alarmTimer = AlarmTime;
    }
}
