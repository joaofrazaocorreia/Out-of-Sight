using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Detection : MonoBehaviour
{
    [SerializeField] [Range(10f, 180f)] private float detectionMaxAngle = 75.0f;
    [SerializeField] private float detectionRange = 12f;
    [SerializeField] private float proximityDetectionRange = 0f;
    [SerializeField] [Range(0f, 20f)] private float baseDetectionRate = 1.0f;
    [SerializeField] private float detectionLimit = 5.0f;

    public static Vector3 lastPlayerPos;
    public static float globalDetectionMultiplier = 1.0f;

    private Player player;
    private float detectionMeter;
    public float DetectionMeter
    {
        get => detectionMeter;
        set => detectionMeter = Mathf.Clamp(value, 0f, detectionLimit);
    }
    public float DetectionLimit {get => detectionLimit;}
    private bool seesPlayer;
    public bool SeesPlayer {get => seesPlayer;}
    private bool seesBody;
    public bool SeesBody {get => seesBody;}
    private List<Body> allBodies;
    private bool tooCloseToPlayer;
    private EnemyCamera enemyCamera;
    private EnemyMovement enemyMovement;


    private void Start()
    {
        player = FindAnyObjectByType<Player>();
        DetectionMeter = 0;
        seesPlayer = false;
        seesBody = false;
        tooCloseToPlayer = false;
        enemyMovement = GetComponentInParent<EnemyMovement>();
        enemyCamera = GetComponentInParent<EnemyCamera>();

        allBodies = new List<Body>();
    }

    private void FixedUpdate()
    {
        if((enemyMovement != null && enemyCamera == null &&
            enemyMovement.status != EnemyMovement.Status.Tased &&
                enemyMovement.status != EnemyMovement.Status.KnockedOut) ||
                    (enemyCamera != null && enemyMovement == null))
        {
            Vector3 distanceToPlayer = player.transform.position - transform.position;

            // Checks if the player is too close to the NPC
            tooCloseToPlayer = distanceToPlayer.magnitude <= proximityDetectionRange;

            // Checks if the player is within range of this NPC's detection range
            if(distanceToPlayer.magnitude <= detectionRange)
            {
                // Checks if the player is within this NPC's field of view
                if(Vector3.Angle(transform.TransformDirection(Vector3.forward), distanceToPlayer) <= detectionMaxAngle)
                {
                    // Sends a raycast towards the player and checks if it hits anything
                    if (Physics.Raycast(transform.position, distanceToPlayer, out RaycastHit hit, detectionRange))
                    {
                        // Checks if the raycast hit the player
                        if (hit.transform.tag == "Player")
                        {
                            seesPlayer = true;
                            Debug.DrawRay(transform.position, distanceToPlayer * hit.distance, Color.red);
                            TrackPlayer();
                        }

                        // If the raycast detects an obstacle between the NPC and the player:
                        else
                        {
                            seesPlayer = false;
                            Debug.DrawRay(transform.position, distanceToPlayer * hit.distance, Color.yellow);
                        }
                    }

                    // If the raycast doesn't reach the player:
                    else
                    {
                        seesPlayer = false;
                        Debug.DrawRay(transform.position, transform.forward * detectionRange, Color.white);
                    }
                }
                
                // If the player is not within the enemy's field of view:
                else
                {
                    seesPlayer = false;
                    Debug.DrawRay(transform.position, transform.forward * detectionRange, Color.white);
                }
            }

            // If the player is too far away to be detected:
            else
            {
                seesPlayer = false;
                Debug.DrawRay(transform.position, transform.forward * detectionRange, Color.white);
            }


            UpdateAllBodies();

            foreach(Body b in allBodies)
            {
                if (!b.enabled || b.HasBeenDetected)
                    continue;
                
                Vector3 distance = b.transform.position - transform.position;

                // Checks if the body is within range of this NPC's detection range
                if(distance.magnitude <= detectionRange)
                {
                    // Checks if the body is within this NPC's field of view
                    if(Vector3.Angle(transform.TransformDirection(Vector3.forward), distance) <= detectionMaxAngle)
                    {
                        // Sends a raycast towards the body and checks if it hits anything
                        if (Physics.Raycast(transform.position, distance, out RaycastHit hit, detectionRange))
                        {
                            // Checks if the raycast hit an body
                            if (b.enabled && !b.HasBeenDetected)
                            {
                                seesBody = true;
                                Debug.DrawRay(transform.position, distance * hit.distance, Color.red);
                                break;
                            }

                            // If the raycast detects an obstacle between the NPC and the body:
                            else
                            {
                                seesBody = false;
                                Debug.DrawRay(transform.position, distance * hit.distance, Color.yellow);
                            }
                        }

                        // If the raycast doesn't reach the body:
                        else
                        {
                            seesBody = false;
                            Debug.DrawRay(transform.position, transform.forward * detectionRange, Color.white);
                        }
                    }
                    
                    // If the body is not within the enemy's field of view:
                    else
                    {
                        seesBody = false;
                        Debug.DrawRay(transform.position, transform.forward * detectionRange, Color.white);
                    }
                }

                // If the body is too far away to be detected:
                else
                {
                    seesBody = false;
                    Debug.DrawRay(transform.position, transform.forward * detectionRange, Color.white);
                }
            }

            UpdateDetection();
        }
    }

    // Checks if the NPC sees the player or if it's too close to them, and raises/decreases detection accordingly
    private void UpdateDetection()
    {
        // Multiple sources of detection stack with each other
        int sourceMultiplier = 0;

        if(seesPlayer || tooCloseToPlayer)
        {
            TrackPlayer();

            if(tooCloseToPlayer)
                sourceMultiplier++;

            if(seesPlayer && player.status.Contains(Player.Status.Suspicious))
                sourceMultiplier++;

            if(seesPlayer && player.status.Contains(Player.Status.Trespassing))
                sourceMultiplier++;
        }

        // Seeing a body increases the multiplier thrice as much
        if(seesBody)
        {
            sourceMultiplier += 3;
        }

        // Increases detection based on the number of sources that increase it
        if (sourceMultiplier != 0)
        {
            DetectionMeter += Time.deltaTime * baseDetectionRate * globalDetectionMultiplier * sourceMultiplier;
        }

        // If there are no sources of detection, it's decreased instead
        else
        {
            DetectionMeter -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Tracks the last seen player position across all enemies.
    /// </summary>
    public void TrackPlayer()
    {
        if(enemyCamera == null || enemyCamera.IsOn)
            lastPlayerPos = player.transform.position;
    }

    public void UpdateAllBodies()
    {
        Body[] bodies = FindObjectsByType<Body>(FindObjectsSortMode.None);

        if(allBodies.Count != bodies.Count())
        {
            allBodies = new List<Body>();

            foreach(Body b in bodies)
            {
                allBodies.Add(b);
            }
        }
    }
}


