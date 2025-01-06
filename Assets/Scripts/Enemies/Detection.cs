using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Detection : MonoBehaviour
{
    [SerializeField] private GameObject selfDetection;
    [SerializeField] private GameObject detectionIcon;
    [SerializeField] private Image detectionFill;
    [SerializeField] private GameObject alarmedIcon;
    [SerializeField] private GameObject tasedIcon;
    [SerializeField] private Image tasedFill;
    [SerializeField] [Range(10f, 180f)] private float detectionMaxAngle = 75.0f;
    [SerializeField] private float detectionRange = 12f;
    [SerializeField] private float proximityDetectionRange = 0f;
    [SerializeField] [Range(0f, 20f)] private float baseDetectionRate = 1.0f;
    [SerializeField] private float detectionLimit = 5.0f;

    public static Vector3 lastPlayerPos;
    public static float globalDetectionMultiplier = 1.0f;

    private Player player;
    private Alarm alarm;
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
    private List<BodyCarry> allBodies;
    private bool tooCloseToPlayer;
    private EnemyCamera enemyCamera;
    private EnemyMovement enemyMovement;


    private void Start()
    {
        player = FindAnyObjectByType<Player>();
        alarm = FindAnyObjectByType<Alarm>();
        DetectionMeter = 0;
        selfDetection.SetActive(false);
        seesPlayer = false;
        seesBody = false;
        tooCloseToPlayer = false;
        enemyMovement = GetComponentInParent<EnemyMovement>();
        enemyCamera = GetComponentInParent<EnemyCamera>();

        allBodies = new List<BodyCarry>();
    }

    private void FixedUpdate()
    {
        if((enemyMovement != null && enemyCamera == null &&
            enemyMovement.currentStatus != EnemyMovement.Status.Tased &&
                enemyMovement.currentStatus != EnemyMovement.Status.KnockedOut) ||
                    (enemyCamera != null && enemyMovement == null && !enemyCamera.Jammed && enemyCamera.IsOn))
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

            foreach(BodyCarry b in allBodies)
            {
                if (!b.enabled || b.HasBeenDetected)
                    continue;
                
                Vector3 distance = b.transform.position - transform.position;

                // Checks if the bodyCarry is within range of this NPC's detection range
                if(distance.magnitude <= detectionRange)
                {
                    // Checks if the bodyCarry is within this NPC's field of view
                    if(Vector3.Angle(transform.TransformDirection(Vector3.forward), distance) <= detectionMaxAngle)
                    {
                        // Sends a raycast towards the bodyCarry and checks if it hits anything
                        if (Physics.Raycast(transform.position, distance, out RaycastHit hit, detectionRange))
                        {
                            // Checks if the raycast hit an bodyCarry
                            if (b.enabled && !b.HasBeenDetected)
                            {
                                seesBody = true;
                                Debug.DrawRay(transform.position, distance * hit.distance, Color.red);
                                break;
                            }

                            // If the raycast detects an obstacle between the NPC and the bodyCarry:
                            else
                            {
                                seesBody = false;
                                Debug.DrawRay(transform.position, distance * hit.distance, Color.yellow);
                            }
                        }

                        // If the raycast doesn't reach the bodyCarry:
                        else
                        {
                            seesBody = false;
                            Debug.DrawRay(transform.position, transform.forward * detectionRange, Color.white);
                        }
                    }
                    
                    // If the bodyCarry is not within the enemy's field of view:
                    else
                    {
                        seesBody = false;
                        Debug.DrawRay(transform.position, transform.forward * detectionRange, Color.white);
                    }
                }

                // If the bodyCarry is too far away to be detected:
                else
                {
                    seesBody = false;
                    Debug.DrawRay(transform.position, transform.forward * detectionRange, Color.white);
                }
            }

            UpdateDetection();
        }

        UpdateSelfDetectionUI();
    }

    // Checks if the NPC sees the player or if it's too close to them, and raises/decreases detection accordingly
    private void UpdateDetection()
    {
        // Multiple sources of detection stack with each other
        int sourceMultiplier = 0;

        // Detection increases per status if the enemy sees the player or is too close to them
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
            float amountToIncrease = Time.deltaTime * baseDetectionRate * globalDetectionMultiplier * sourceMultiplier;

            // If the enemy sees the player with the doubtful status, the detection increase is boosted by 25%
            if((seesPlayer || tooCloseToPlayer) && player.status.Contains(Player.Status.Doubtful))
                amountToIncrease *= 1.25f;

            DetectionMeter += amountToIncrease;
        }

        // If there are no sources of detection, the meter is decreased instead
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
        BodyCarry[] bodies = FindObjectsByType<BodyCarry>(FindObjectsSortMode.None);

        if(allBodies.Count != bodies.Count())
        {
            allBodies = new List<BodyCarry>();

            foreach(BodyCarry b in bodies)
            {
                allBodies.Add(b);
            }
        }
    }

    private void UpdateSelfDetectionUI()
    {
        if(enemyMovement != null && enemyCamera == null)
        {
            if(enemyMovement.currentStatus == EnemyMovement.Status.Tased)
            {
                selfDetection.SetActive(true);
                tasedIcon.SetActive(true);
                alarmedIcon.SetActive(false);
                detectionIcon.SetActive(false);

                tasedFill.fillAmount = enemyMovement.TasedTimer / enemyMovement.TasedTime;
            }

            else if(enemyMovement != null && alarm.IsOn)
            {
                selfDetection.SetActive(true);
                tasedIcon.SetActive(false);
                alarmedIcon.SetActive(true);
                detectionIcon.SetActive(false);
            }

            else if(DetectionMeter > 0)
            {
                selfDetection.SetActive(true);
                tasedIcon.SetActive(false);
                alarmedIcon.SetActive(false);
                detectionIcon.SetActive(true);

                detectionFill.fillAmount = DetectionMeter / DetectionLimit;
            }

            else
            {
                selfDetection.SetActive(false);
            }
        }

        else if (enemyMovement == null && enemyCamera != null)
        {
            if(enemyCamera.Jammed)
            {
                selfDetection.SetActive(true);
                tasedIcon.SetActive(true);
                alarmedIcon.SetActive(false);
                detectionIcon.SetActive(false);

                tasedFill.fillAmount = 1f;
            }

            else if(DetectionMeter > 0 && enemyCamera.IsOn)
            {
                selfDetection.SetActive(true);
                tasedIcon.SetActive(false);
                alarmedIcon.SetActive(false);
                detectionIcon.SetActive(true);

                detectionFill.fillAmount = DetectionMeter / DetectionLimit;
            }

            else
            {
                selfDetection.SetActive(false);
            }
        }
    }
}


