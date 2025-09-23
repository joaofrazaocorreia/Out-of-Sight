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
    [SerializeField] private float detectionStallTime = 4.0f;
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
    private float detectionStallTimer;
    private LayerMask detectionLayers;

    public float DetectionMeter
    {
        get => detectionMeter;
        set => detectionMeter = Mathf.Clamp(value, 0f, detectionLimit);
    }
    public float DetectionLimit {get => detectionLimit;}
    private List<DetectableObject> seenDetectables;
    public DetectableObject ClosestSuspiciousObject
    {
        get
        {
            DetectableObject closest = null;
            if(tooCloseToPlayer) closest = player.GetComponent<DetectableObject>();

            foreach(DetectableObject d in seenDetectables)
            {
                if((d.DetectionMultiplier != 0) && (closest == null ||
                    (d.transform.position - transform.position).magnitude < (closest.transform.position
                        - transform.position).magnitude))
                {
                    closest = d;
                }
            }

            return closest;
        }
    }
    private DetectableObject playerDetectable;
    public bool SeesPlayer { get => seenDetectables.Contains(playerDetectable); }
    private static List<DetectableObject> allDetectables;
    private bool tooCloseToPlayer;
    public bool TooCloseToPlayer {get => tooCloseToPlayer;}
    private EnemyCamera enemyCamera;
    private EnemyMovement enemyMovement;
    private bool isDetectionReset;


    private void Awake()
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
        detectionStallTimer = 0;
        selfDetection.SetActive(false);
        seenDetectables = new List<DetectableObject>();
        tooCloseToPlayer = false;
        playerDetectable = player.GetComponentInChildren<DetectableObject>();
        enemyMovement = GetComponentInParent<EnemyMovement>();
        enemyCamera = GetComponentInParent<EnemyCamera>();
        detectionLayers = LayerMask.GetMask("Default", "Player", "Enemies", "Body");
    
        globalDetectionMultiplier = 1f;
        allDetectables ??= new List<DetectableObject>();
        
        foreach(Enemy e in FindObjectsByType<Enemy>(FindObjectsSortMode.None).ToList())
        {
            e.OnKnockout += OnEnemyKnockout;
        }
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
                CheckForDetectables(allDetectables.Where(d => d != null && d.enabled).ToList());

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
            // Calculates the raw and horizontal distances from this enemy towards the current detectableObject
            Vector3 distanceToDetectable = d.transform.position - transform.position;
            Vector3 distanceToDetectableHorizontal = new Vector3
                (d.transform.position.x, 0f, d.transform.position.z) - 
                    new Vector3(transform.position.x, 0f, transform.position.z);

            float range = Mathf.Min(distanceToDetectable.magnitude, detectionRange);

            // Checks if the current detectableObject is the player
            bool isPlayer = d.GetComponentInParent<Player>() != null;
            
            if (isPlayer)
            {
                // Checks if the player is too close to the NPC
                tooCloseToPlayer = distanceToDetectable.magnitude <= proximityDetectionRange;
            }
            
            // Checks if the detectableObject is within range to be detected by this NPC
            if(distanceToDetectable.magnitude <= detectionRange ||
                distanceToDetectableHorizontal.magnitude <= proximityDetectionRange)
            {
                // Checks if the NPC detects the object by proximity
                if(distanceToDetectableHorizontal.magnitude <= proximityDetectionRange)
                {
                    DetectObject(d, distanceToDetectable.normalized * range);
                }

                // If not detected by proximity, checks if the object is visible by this NPC
                else
                {
                    // Checks if the detectableObject is within this NPC's field of view
                    if (Vector3.Angle(transform.TransformDirection(Vector3.forward),
                        distanceToDetectableHorizontal) <= detectionMaxAngle &&
                            (enemyCamera == null || distanceToDetectableHorizontal.magnitude >= 2f))
                    {
                        // Sends a raycast towards the detectableObject
                        Physics.Raycast(transform.position, distanceToDetectable,
                            out RaycastHit hit, detectionRange, detectionLayers);

                        // Checks if the raycast hit the collider of the detectableObject
                        if(d.GetComponent<Collider>() == hit.collider ||
                            d.GetComponentsInChildren<Collider>().Contains(hit.collider) ||
                                (isPlayer && hit.collider == player.GetComponent<CharacterController>()))
                        {
                            DetectObject(d, distanceToDetectable.normalized * range);
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

            //if(tooCloseToPlayer)
                //sourceMultiplier += 0.0f;

            if (player.status.Contains(Player.Status.CriticalTrespassing))
                detectionMeter = 10f;

            else if (player.status.Contains(Player.Status.Doubtful) && SeesPlayer
                && DetectionMeter >= DetectionLimit / 3f)
            {
                detectionStallTimer = detectionStallTime * 0.5f;
            }
        }

        // Seeing detectables increases the detection multiplier for the detectables' respective individual multiplers
        foreach(DetectableObject d in seenDetectables)
        {
            if (d == playerDetectable && player.disguise != Enums.Disguise.Civillian)
                sourceMultiplier += d.DetectionMultiplier * 0.70f;

            else
                sourceMultiplier += d.DetectionMultiplier;
        }

        // Increases detection based on the number of sources that increase it
        if (sourceMultiplier != 0)
        {
            float amountToIncrease = Time.deltaTime * baseDetectionRate * globalDetectionMultiplier * sourceMultiplier;

            // If the enemy sees the player with the doubtful status, the detection increase is boosted by 25%
            if((SeesPlayer || tooCloseToPlayer) && player.status.Contains(Player.Status.Doubtful))
                amountToIncrease *= 1.25f;

            // Increases the detection meter with the calculated amount
            DetectionMeter += amountToIncrease;

            // Resets the timer that stops the enemy's detection from lowering temporarily
            if(detectionMeter >= detectionLimit / 5)
                detectionStallTimer = detectionStallTime;
        }

        // If there are no sources of detection, the meter is decreased instead
        else if(DetectionMeter < DetectionLimit)
        {
            if(detectionStallTimer > 0)
                detectionStallTimer -= Time.deltaTime;
            
            else
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
    /// Detects a given DetectableObject unless it's a body that's been detected already.
    /// </summary>
    /// <param name="detectableObject">The DetectableObject to detect.</param>
    /// <param name="raycastRangeDirection">The range and direction of the debug ray.</param>
    private void DetectObject(DetectableObject detectableObject, Vector3 raycastRangeDirection)
    {
        // Checks if the detectableObject is a body
        BodyCarry body = detectableObject.GetComponent<BodyCarry>();

        if (body != null)
        {
            // Detects the body if it hasn't been found
            if(!body.HasBeenDetected)
            {
                Detect(detectableObject);
                Debug.DrawRay(transform.position, raycastRangeDirection, Color.red);
            }

            // Doesn't detect the body if it was already found
            else
            {
                Undetect(detectableObject);
                Debug.DrawRay(transform.position, raycastRangeDirection, Color.green);
            }
        }

        // Detects the detectableObject if it's not a body
        else
        {
            Detect(detectableObject);
            Debug.DrawRay(transform.position, raycastRangeDirection, Color.red);
        }
    }

    /// <summary>
    /// Registers a new DetectableObject for all enemies.
    /// </summary>
    /// <param name="detectableObject"></param>
    public static void AddDetectable(DetectableObject detectableObject)
    {
        allDetectables ??= new List<DetectableObject>();

        if (!allDetectables.Contains(detectableObject))
            allDetectables.Add(detectableObject);
    }

    /// <summary>
    /// Unregisters a given DetectableObject for all enemies.
    /// </summary>
    /// <param name="detectableObject"></param>
    public static void RemoveDetectable(DetectableObject detectableObject)
    {
        if(allDetectables.Contains(detectableObject))
            allDetectables.Remove(detectableObject);
    }

    /// <summary>
    /// Enables and updates the detection UI specific to this enemy.
    /// </summary>
    private void UpdateSelfDetectionUI()
    {
        // The UI checks if the enemy is not knocked out and not a camera
        if (enemyMovement != null && enemyCamera == null &&
            selfEnemy.EnemyStatus != Enemy.Status.KnockedOut)
        {
            // Enables the alarmed icon when alarmed
            if (enemyMovement != null && (alarm.IsOn || selfEnemy.IsAlarmed))
            {
                selfDetection.SetActive(true);
                tasedIcon.SetActive(false);
                alarmedIcon.SetActive(true);
                detectionIcon.SetActive(false);
            }

            // Enables the detection meter if it's detecting something
            else if (DetectionMeter > 0)
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
            if (enemyCamera.Jammed)
            {
                selfDetection.SetActive(true);
                tasedIcon.SetActive(true);
                alarmedIcon.SetActive(false);
                detectionIcon.SetActive(false);

                tasedFill.fillAmount = 1f;
            }

            // Enables the alarmed icon when the alarm is raised (and the camera is turned on)
            else if (alarm.IsOn && enemyCamera.IsOn)
            {
                selfDetection.SetActive(true);
                tasedIcon.SetActive(false);
                alarmedIcon.SetActive(true);
                detectionIcon.SetActive(false);
            }

            // Shows the detection UI if the camera is detecting something (and turned on)
            else if (DetectionMeter > 0 && enemyCamera.IsOn)
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

    public bool Sees(DetectableObject detectableObject)
    {
        return seenDetectables.Contains(detectableObject);
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
        if (SeesPlayer && player.detectable && GetComponentInParent<Enemy>().IsConscious)
        {
            detectionMeter = detectionLimit * 2;
        }
    }

    private void OnEnemyKnockout(object sender, EventArgs e)
    {
        /*
        if (Sees(enemy.GetComponent<DetectableObject>) && player.detectable)
        {
            detectionMeter = detectionLimit * 2;
            Debug.Log("this npc saw another get knocked out");
        }*/
    }
}


