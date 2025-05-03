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
    private List<DetectableObject> seenDetectables;
    public bool SeesPlayer {get => seenDetectables.Contains(player.GetComponent<DetectableObject>());}
    public bool SeesDetectable {get => seenDetectables.Count() > 0;}
    private List<DetectableObject> allDetectables;
    private bool tooCloseToPlayer;
    private EnemyCamera enemyCamera;
    private EnemyMovement enemyMovement;
    private bool isDetectionReset;
    private float detectablesCount;


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
        seenDetectables = new List<DetectableObject>();
        tooCloseToPlayer = false;
        enemyMovement = GetComponentInParent<EnemyMovement>();
        enemyCamera = GetComponentInParent<EnemyCamera>();
        detectionLayers = LayerMask.GetMask("Default", "Player", "Enemies");
    
        globalDetectionMultiplier = 1f;
        allDetectables = new List<DetectableObject>();
        UpdateAllDetectables();
    }

    private void FixedUpdate()
    {
        // If this setting is toggled off, all detections are disabled
        if(player.detectable)
        {
            // Checks if this enemy is either a conscious NPC or an enabled camera
            if((enemyMovement != null && selfEnemy.IsConscious) || (enemyCamera != null &&
                    enemyMovement == null && !enemyCamera.Jammed && enemyCamera.IsOn))
            {
                // Checks if it can see any available detectables in the level
                UpdateAllDetectables();
                CheckForDetectables(allDetectables);

                // Updates this enemy's detection meter according to what it is detecting
                UpdateDetection();
            }

            // Updates the UI to show visual feedback of the detection meter changes
            UpdateSelfDetectionUI();
        }
    }

    /// <summary>
    /// Checks if this enemy can see any detectables a given list, and Detects and Undetects them accordingly.
    /// </summary>
    /// <param name="detectablesList">The list of detectables to check.</param>
    private void CheckForDetectables(List<DetectableObject> detectablesList)
    {
        foreach(DetectableObject d in detectablesList)
        {
            Vector3 distanceToDetectable = d.transform.position - transform.position;
            Vector3 distanceToDetectableHorizontal = new Vector3
                (d.transform.position.x, 0f, d.transform.position.z) - 
                    new Vector3(transform.position.x, 0f, transform.position.z);

            float range = Mathf.Min(distanceToDetectable.magnitude, detectionRange);

            // Checks if the current detectableObject is the player
            Player player = d.GetComponent<Player>();
            if(player != null)
            {
                // Checks if the player is too close to the NPC
                tooCloseToPlayer = distanceToDetectable.magnitude <= proximityDetectionRange;
            }
            
            // Checks if the detectableObject is within range of this NPC's detection range
            if(distanceToDetectable.magnitude <= detectionRange)
            {
                // Checks if the detectableObject is within this NPC's field of view
                if(Vector3.Angle(transform.TransformDirection(Vector3.forward), 
                    distanceToDetectableHorizontal) <= detectionMaxAngle)
                {
                    // Sends a raycast towards the detectableObject
                    Physics.Raycast(transform.position, distanceToDetectable,
                        out RaycastHit hit, detectionRange, detectionLayers);

                    // Checks if the raycast hit the detectableObject
                    if(hit.collider == d.GetComponent<Collider>())
                    {
                        // Checks if the detectableObject is a body
                        BodyCarry body = d.GetComponent<BodyCarry>();

                        if (body != null)
                        {
                            // Detects the body if it hasn't been found
                            if(!body.HasBeenDetected)
                            {
                                Detect(d);
                                Debug.DrawRay(transform.position, distanceToDetectable.normalized * range, Color.red);
                            }

                            // Doesn't detect the body if it was already found
                            else
                            {
                                Undetect(d);
                                Debug.DrawRay(transform.position, distanceToDetectable.normalized * range, Color.green);
                            }
                        }

                        // Detects the detectableObject if it's not a body
                        else
                        {
                            Detect(d);
                            Debug.DrawRay(transform.position, distanceToDetectable.normalized * range, Color.red);
                        }
                    }

                    // Doesn't detect the detectableObject if the raycast didn't reach it or hit something else
                    else
                    {
                        Undetect(d);
                        Debug.DrawRay(transform.position, distanceToDetectable.normalized * range, Color.yellow);
                    }
                }
                
                // Doesn't detect the detectableObject if it's not within the enemy's field of view
                else
                {
                    Undetect(d);
                    Debug.DrawRay(transform.position, transform.forward * detectionRange, Color.white);
                }
            }

            // Doesn't detect the detectableObject if it's too far away to be detected
            else
            {
                Undetect(d);
                Debug.DrawRay(transform.position, transform.forward * detectionRange, Color.white);
            }
        }
    }

    /// <summary>
    /// Adds a given detectableObject object to the list of detectables currently seen by this NPC.
    /// </summary>
    /// <param name="detectableObject">The given detectableObject object to add to the list.</param>
    private void Detect(DetectableObject detectableObject)
    {
        if(!seenDetectables.Contains(detectableObject))
        {
            seenDetectables.Add(detectableObject);
        }
    }

    /// <summary>
    /// Removes a given detectableObject object from the list of detectables currently seen by this NPC.
    /// </summary>
    /// <param name="detectableObject">The given detectableObject object to remove from the list.</param>
    private void Undetect(DetectableObject detectableObject)
    {
        if(seenDetectables.Contains(detectableObject))
        {
            seenDetectables.Remove(detectableObject);
        }
    }

    /// <summary>
    /// Checks if the NPC sees the player or if it's too close to them, and raises/decreases detection accordingly.
    /// </summary>
    private void UpdateDetection()
    {
        // Multiple sources of detection stack with each other
        float sourceMultiplier = 0;

        // Detection increases per status if the enemy sees the player or is too close to them
        if(SeesPlayer || tooCloseToPlayer)
        {
            TrackPlayer();

            if(tooCloseToPlayer)
                sourceMultiplier += 0.5f;

            if(player.status.Contains(Player.Status.CriticalTrespassing))
                detectionMeter = 10f;
        }

        // Seeing detectables increases the detection multiplier for the detectables' respective individual multiplers
        foreach(DetectableObject d in seenDetectables)
        {
            sourceMultiplier += d.DetectionMultiplier;
        }

        // Increases detection based on the number of sources that increase it
        if (sourceMultiplier != 0)
        {
            float amountToIncrease = Time.deltaTime * baseDetectionRate * globalDetectionMultiplier * sourceMultiplier;

            // If the enemy sees the player with the doubtful status, the detection increase is boosted by 25%
            if((SeesPlayer || tooCloseToPlayer) && player.status.Contains(Player.Status.Doubtful))
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
    /// Updates the list of available detectables in the level.
    /// </summary>
    public void UpdateAllDetectables()
    {
        allDetectables = FindObjectsByType<DetectableObject>
            (FindObjectsSortMode.None).ToList();
        allDetectables = allDetectables.Where(a => a != null && a.enabled).ToList();

        seenDetectables = new List<DetectableObject>();
    }

    /// <summary>
    /// Enables and updates the detection UI specific to this enemy.
    /// </summary>
    private void UpdateSelfDetectionUI()
    {
        // The UI checks if the enemy is not knocked out and not a camera
        if(enemyMovement != null && enemyCamera == null &&
            selfEnemy.EnemyStatus != Enemy.Status.KnockedOut)
        {
            /*
            // Enables the tased timer UI when tased
            if(enemyMovement.EnemyStatus == Enemy.Status.Tased)
            {
                selfDetection.SetActive(true);
                tasedIcon.SetActive(true);
                alarmedIcon.SetActive(false);
                detectionIcon.SetActive(false);

                tasedFill.fillAmount = enemyMovement.TasedTimer / enemyMovement.TasedTime;
            }*/

            // Enables the alarmed icon when alarmed
            if(enemyMovement != null && (alarm.IsOn || selfEnemy.IsAlarmed))
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
        if (SeesPlayer && player.detectable) GetActionSuspicion(sender);
    }

    private void GetActionSuspicion(object sender)
    {
        var increase = ((PlayerInteraction)sender).ActiveInteractiveObject.SuspicionIncreaseOnInteraction;
        if (increase > 0f) detectionMeter += increase;
    }

    private void OnPlayerAttack(object sender, EventArgs e)
    {
        if (SeesPlayer && player.detectable) detectionMeter = detectionLimit * 2;
    }
}


