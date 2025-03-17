using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    [SerializeField] private PlayAudio detectionAudioPlayer;
    
    public static Vector3 lastPlayerPos;
    public static float globalDetectionMultiplier = 1.0f;

    private Player player;
    private PlayerInteraction playerInteraction;
    private Enemy selfEnemy;
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
    private bool isDetectionReset;


    private void Start()
    {
        player = FindAnyObjectByType<Player>();
        playerInteraction = FindAnyObjectByType<PlayerInteraction>();
        playerInteraction.OnSuspiciousAction += OnSuspiciousAction;
        selfEnemy = GetComponentInParent<Enemy>();
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
        // If this setting is toggled off, all detections are disabled
        if(player.detectable)
        { 
            // Checks if the enemy is either a conscious NPC or an enabled camera
            if((enemyMovement != null && enemyCamera == null &&
                enemyMovement.IsConscious) || (enemyCamera != null &&
                    enemyMovement == null && !enemyCamera.Jammed && enemyCamera.IsOn))
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


                // Checks all available bodies in the level that haven't been seen already
                UpdateAllBodies();
                foreach(BodyCarry b in allBodies)
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
                            RaycastHit[] hits = new RaycastHit[10];
                            Physics.RaycastNonAlloc(new Ray(transform.position, distance), hits, detectionRange);

                            // Validates the detection if the only raycast collision was the body
                            if (hits.Count() == 1)
                            {
                                // Checks if the raycast hit an available body that hasn't been seen yet
                                if (hits[0].transform.GetComponent<BodyCarry>() && b.enabled && !b.HasBeenDetected)
                                {
                                    seesBody = true;
                                    Debug.DrawRay(transform.position, distance * hits[0].distance, Color.red);
                                    break;
                                }

                                // If the body was already seen, draws a debug ray
                                else
                                {
                                    seesBody = false;
                                    Debug.DrawRay(transform.position, distance * hits[0].distance, Color.yellow);
                                }
                            }

                            // If the raycast hit obstacles other than the body
                            else if(hits.Count() != 0)
                            {
                                seesBody = false;
                                Debug.DrawRay(transform.position, distance * detectionRange, Color.yellow);
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

            UpdateSelfDetectionUI();
        }
    }

    /// <summary>
    /// Checks if the NPC sees the player or if it's too close to them, and raises/decreases detection accordingly.
    /// </summary>
    private void UpdateDetection()
    {
        // Multiple sources of detection stack with each other
        int sourceMultiplier = 0;

        // Detection increases per status if the enemy sees the player or is too close to them
        if(seesPlayer || tooCloseToPlayer)
        {
            TrackPlayer();

            if(tooCloseToPlayer)
                sourceMultiplier += 1;

            if(seesPlayer && player.status.Contains(Player.Status.Suspicious))
                sourceMultiplier += 2;

            if(seesPlayer && player.status.Contains(Player.Status.Trespassing))
                sourceMultiplier += 1;
            if(seesPlayer && player.status.Contains(Player.Status.CriticalTrespassing))
                detectionMeter = 10f;
        }

        // Seeing a body increases the multiplier thrice as much
        if(seesBody)
        {
            sourceMultiplier += 5;
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
        else if(DetectionMeter < DetectionLimit)
        {
            DetectionMeter -= Time.deltaTime;
        }

        // Plays audio only when the detection is raised from 0
        switch (DetectionMeter)
        {
            case > 0 when isDetectionReset:
                isDetectionReset = false;
                if(detectionAudioPlayer != null) detectionAudioPlayer.Play();
                break;
            case 0 when !isDetectionReset:
                isDetectionReset = true;
                break;
        }
    }

    /// <summary>
    /// Tracks the last seen player position across all enemies.
    /// </summary>
    public void TrackPlayer()
    {
        if(player.detectable && (enemyCamera == null || enemyCamera.IsOn))
            lastPlayerPos = player.transform.position;
    }

    /// <summary>
    /// Updates the list of bodies in the level.
    /// </summary>
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

    /// <summary>
    /// Enables and updates the detection UI specific to this enemy.
    /// </summary>
    private void UpdateSelfDetectionUI()
    {
        // The UI checks if the enemy is not knocked out and not a camera
        if(enemyMovement != null && enemyCamera == null &&
            enemyMovement.currentStatus != EnemyMovement.Status.KnockedOut)
        {
            // Enables the tased timer UI when tased
            if(enemyMovement.currentStatus == EnemyMovement.Status.Tased)
            {
                selfDetection.SetActive(true);
                tasedIcon.SetActive(true);
                alarmedIcon.SetActive(false);
                detectionIcon.SetActive(false);

                tasedFill.fillAmount = enemyMovement.TasedTimer / enemyMovement.TasedTime;
            }

            // Enables the alarmed icon when alarmed
            else if(enemyMovement != null && (alarm.IsOn || selfEnemy.IsAlarmed))
            {
                selfDetection.SetActive(true);
                tasedIcon.SetActive(false);
                alarmedIcon.SetActive(true);
                detectionIcon.SetActive(false);
            }

            // Enables the detection meter if it's detecting something
            else if(DetectionMeter > 0)
            {
                selfDetection.SetActive(true);
                tasedIcon.SetActive(false);
                alarmedIcon.SetActive(false);
                detectionIcon.SetActive(true);

                detectionFill.fillAmount = DetectionMeter / DetectionLimit;

                // The color becomes increasingly more red the closer it gets to the limit

                float colorDifference = 1 - (DetectionMeter / DetectionLimit);
                detectionFill.GetComponentInChildren<Image>().color = new Color(1, colorDifference, colorDifference, 1);
            }

            // If no info needs to be displayed, the UI turns off
            else
            {
                selfDetection.SetActive(false);
            }
        }

        // Checks if this enemy is a camera
        else if (enemyMovement == null && enemyCamera != null)
        {
            // Enables the jammed icon when being jammed
            if(enemyCamera.Jammed)
            {
                selfDetection.SetActive(true);
                tasedIcon.SetActive(true);
                alarmedIcon.SetActive(false);
                detectionIcon.SetActive(false);

                tasedFill.fillAmount = 1f;
            }

            // Enables the alarmed icon when the alarm is raised (and the camera is turned on)
            else if(alarm.IsOn && enemyCamera.IsOn)
            {
                selfDetection.SetActive(true);
                tasedIcon.SetActive(false);
                alarmedIcon.SetActive(true);
                detectionIcon.SetActive(false);
            }

            // Shows the detection UI if the camera is detecting something (and turned on)
            else if(DetectionMeter > 0 && enemyCamera.IsOn)
            {
                selfDetection.SetActive(true);
                tasedIcon.SetActive(false);
                alarmedIcon.SetActive(false);
                detectionIcon.SetActive(true);

                detectionFill.fillAmount = DetectionMeter / DetectionLimit;

                // The color becomes increasingly more red the closer it gets to the limit

                float colorDifference = 1 - (DetectionMeter / DetectionLimit);
                detectionFill.GetComponentInChildren<Image>().color = new Color(1, colorDifference, colorDifference, 1);
            }

            // Disables the UI if no info needs to be shown
            else
            {
                selfDetection.SetActive(false);
            }
        }

        // If the enemy is neither an active camera or conscious enemy, disables this UI
        else
        {
            selfDetection.SetActive(false);
        }
    }

    private void OnSuspiciousAction(object sender, EventArgs e)
    {
        if (seesPlayer) GetActionSuspicion(sender);
    }

    private void GetActionSuspicion(object sender)
    {
        var increase = ((PlayerInteraction)sender).ActiveInteractiveObject.SuspicionIncreaseOnInteraction;
        if (increase > 0f) detectionMeter += increase;
    }
}


