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
    private bool tooCloseToPlayer;
    private EnemyCamera enemyCamera;
    private EnemyMovement enemyMovement;


    private void Start()
    {
        player = FindAnyObjectByType<Player>();
        DetectionMeter = 0;
        seesPlayer = false;
        tooCloseToPlayer = false;
        enemyMovement = GetComponentInParent<EnemyMovement>();
        enemyCamera = GetComponentInParent<EnemyCamera>();
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
                // Checks if player is within this NPC's field of view
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


            CheckForPlayer();
        }
    }

    // Checks if the NPC sees the player or if it's too close to them, and raises/decreases detection accordingly
    private void CheckForPlayer()
    {
        // Multiple sources of detection stack with each other
        int sourceMultiplier = 0;

        if(seesPlayer || tooCloseToPlayer) // || seesSuspiciousObject);
        {
            TrackPlayer();

            if(tooCloseToPlayer)
                sourceMultiplier++;

            if(seesPlayer && player.status.Contains(Player.Status.Suspicious))
                sourceMultiplier++;

            if(seesPlayer && player.status.Contains(Player.Status.Trespassing))
                sourceMultiplier++;

            /*
            if(seesSuspiciousObject)
                sourceMultiplier++;
            */


            DetectionMeter += Time.deltaTime * baseDetectionRate * globalDetectionMultiplier * sourceMultiplier;
        }

        // If there are no sources of detection, it's decreased instead
        if(sourceMultiplier == 0)
        {
            DetectionMeter -= Time.deltaTime;
        }

        /*if(DetectionMeter != 0)
            Debug.Log(transform.parent.name + " - detection %: " + Mathf.Round(DetectionMeter/DetectionLimit * 100));*/
    }

    /// <summary>
    /// Tracks the last seen player position across all enemies.
    /// </summary>
    public void TrackPlayer()
    {
        if(enemyCamera == null || enemyCamera.IsOn)
            lastPlayerPos = player.transform.position;
    }
}


