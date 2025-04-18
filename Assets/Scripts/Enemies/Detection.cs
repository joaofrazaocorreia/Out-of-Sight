using System;
using System.Collections.Generic;
using System.Linq;
using Interaction.Equipments;
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
    private PlayerMelee playerMelee;
    private Taser taser;
    private Enemy selfEnemy;
    private Alarm alarm;
    private float detectionMeter;
    private LayerMask detectionLayers;

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
        playerMelee = FindAnyObjectByType<PlayerMelee>();
        playerMelee.OnKnockout += OnPlayerAttack;
        taser = FindAnyObjectByType<Taser>();
        taser.OnTaserShot += OnPlayerAttack;
        selfEnemy = GetComponentInParent<Enemy>();
        alarm = FindAnyObjectByType<Alarm>();
        DetectionMeter = 0;
        selfDetection.SetActive(false);
        seesPlayer = false;
        seesBody = false;
        tooCloseToPlayer = false;
        enemyMovement = GetComponentInParent<EnemyMovement>();
        enemyCamera = GetComponentInParent<EnemyCamera>();
        detectionLayers = LayerMask.GetMask("Default", "Player", "Interactables", "Enemies");

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
                float range = Mathf.Min(distanceToPlayer.magnitude, detectionRange);

                // Checks if the player is too close to the NPC
                tooCloseToPlayer = distanceToPlayer.magnitude <= proximityDetectionRange;

                // Checks if the player is within range of this NPC's detection range
                if(distanceToPlayer.magnitude <= detectionRange)
                {
                    // Checks if the player is within this NPC's field of view
                    if(Vector3.Angle(transform.TransformDirection(Vector3.forward), distanceToPlayer) <= detectionMaxAngle)
                    {
                        // Sends a raycast towards the player and checks if it hits anything
                        if (Physics.Raycast(transform.position, distanceToPlayer, out RaycastHit hit, range, detectionLayers))
                        {
                            // Checks if the raycast hit the player
                            if (hit.transform.tag == "Player")
                            {
                                seesPlayer = true;
                                Debug.DrawRay(transform.position, distanceToPlayer.normalized * range, Color.red);
                                TrackPlayer();
                            }

                            // If the raycast detects an obstacle between the NPC and the player:
                            else
                            {
                                seesPlayer = false;
                                Debug.DrawRay(transform.position, distanceToPlayer.normalized * range, Color.yellow);
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
                    if (b != null && (!b.enabled || b.HasBeenDetected))
                        continue;
                    
                    Vector3 distanceToBody = b.transform.position - transform.position;
                    Vector3 distanceToBodyHorizontal = new Vector3(b.transform.position.x, 0f, b.transform.position.z) - new Vector3(transform.position.x, 0f, transform.position.z);
                    range = Mathf.Min(distanceToBody.magnitude, detectionRange);

                    // Checks if the body is within range of this NPC's detection range
                    if(distanceToBody.magnitude <= detectionRange)
                    {
                        // Checks if the body is within this NPC's field of view
                        if(Vector3.Angle(transform.TransformDirection(Vector3.forward), distanceToBodyHorizontal) <= detectionMaxAngle)
                        {
                            // Sends a raycast towards the body
                            Physics.Raycast(transform.position, distanceToBody, out RaycastHit hit, detectionRange, detectionLayers);

                            // Checks if the raycast hit the body
                            if(hit.collider == b.GetComponent<Collider>())
                            {
                                // Checks if the body is enabled and hasn't been seen yet
                                if(b.enabled && !b.HasBeenDetected)
                                {
                                    seesBody = true;
                                    Debug.DrawRay(transform.position, distanceToBody.normalized * range, Color.red);
                                    break;
                                }

                                // If the body is disabled or was already seen:
                                else
                                {
                                    seesBody = false;
                                    Debug.DrawRay(transform.position, distanceToBody.normalized * range, Color.green);
                                }
                            }

                            // If the raycast didn't hit the body, draws different debug rays
                            else
                            {
                                seesBody = false;

                                // If the raycast hit any obstacle other than the body, draws a yellow ray towards the hit
                                if(hit.transform)
                                    Debug.DrawRay(transform.position, distanceToBody.normalized * range, Color.yellow);

                                // If the raycast didn't reach the body, draws a white ray forward
                                else
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

        seesBody = false;
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
        if (seesPlayer && player.detectable) GetActionSuspicion(sender);
    }

    private void GetActionSuspicion(object sender)
    {
        var increase = ((PlayerInteraction)sender).ActiveInteractiveObject.SuspicionIncreaseOnInteraction;
        if (increase > 0f) detectionMeter += increase;
    }

    private void OnPlayerAttack(object sender, EventArgs e)
    {
        if (seesPlayer && player.detectable) detectionMeter = detectionLimit * 2;
    }
}


