using UnityEngine;
using UnityEngine.UI;

public class EnemyGuard : Enemy
{
    /// <summary>
    /// How long this enemy stays at the last seen player position before searching around it
    /// </summary>
    [SerializeField] private float aggroTime = 5f;
    /// <summary>
    /// How close this enemy needs to be to the player while chasing to catch them
    /// </summary>
    [SerializeField] private float playerCatchDistance = 1.5f;
    /// <summary>
    /// If true, this enemy won't chase the player during alarms
    /// </summary>
    [SerializeField] private bool ignoresAlarm = false;
    [SerializeField] private GameObject radioIcon;
    [SerializeField] private Image radioFill;
    [SerializeField] private PlayAudio radioSoundPlayer;

    private float aggroTimer;
    private Vector3 prevPlayerPos;

    
    protected override void Start()
    {
        base.Start();

        type = Type.Guard;
    }


    protected override void Update()
    {
        CheckStatus();

        if(IsAlarmed && enemyMovement.IsConscious)
        {
            alarmedTimer -= Time.deltaTime;
            radioIcon.SetActive(true);
            radioFill.fillAmount = alarmedTimer / alarmedTime;

            if(alarmedTimer <= 0)
            {
                alarm.TriggerAlarm(!alarm.IsOn);
                radioIcon.SetActive(false);
            }
        }

        else
            radioIcon.SetActive(false);
    }

    // Checks this enemy's status and proceeds accordingly.
    private void CheckStatus()
    {
        // ------------ Normal ------------ 
        if(enemyMovement.currentStatus == EnemyMovement.Status.Normal)
        {
            // Becomes alarmed if the detection meter fills up
            if(detection.DetectionMeter >= detection.DetectionLimit)
            {
                BecomeAlarmed();
            }
        }

        // ------------ Chasing ------------ 
        else if(enemyMovement.currentStatus == EnemyMovement.Status.Chasing)
        {
            // Becomes alarmed if this enemy sees the player while chasing them
            if(detection.SeesPlayer)
            {
                BecomeAlarmed();

                // Resets the alarm timer if it's already on
                if(alarm.IsOn)
                    alarm.TriggerAlarm(!alarm.IsOn);
            }

            if(enemyMovement.IsAtDestination)
            {
                // Remains at the position if it's still aggro
                if(aggroTimer > 0)
                {
                    aggroTimer -= Time.deltaTime;
                }

                // Begins searching after aggro period ends
                else
                {
                    prevPlayerPos = Detection.lastPlayerPos;
                    enemyMovement.currentStatus = EnemyMovement.Status.Searching;
                }
            }

            // Keeps the enemy aggro until it reaches its target.
            else
            {
                aggroTimer = aggroTime;
            }

            // If this enemy comes close enough to the player to catch them, the player loses.

            float distanceToPlayer = (Vector3.Scale(transform.position, new Vector3(1f, 0f, 1f))
                - Vector3.Scale(player.transform.position, new Vector3(1f, 0f, 1f))).magnitude;

            if(distanceToPlayer < playerCatchDistance)
            {
                uiManager.Lose();
            }
        }

        // ------------ Searching ------------ 
        else if(enemyMovement.currentStatus == EnemyMovement.Status.Searching)
        {
            // Becomes alarmed if this enemy (or another) sees the player 
            if(detection.SeesPlayer || Detection.lastPlayerPos != prevPlayerPos)
            {
                prevPlayerPos = Detection.lastPlayerPos;
                BecomeAlarmed();
            }

            
            // Goes back to normal after the alarm ends
            else if (!alarm.IsOn)
            {
                enemyMovement.currentStatus = EnemyMovement.Status.Normal;
            }
            
        }

        // ------------ Tased && Knocked out ------------ 
        else if(enemyMovement.currentStatus == EnemyMovement.Status.Tased ||
            enemyMovement.currentStatus == EnemyMovement.Status.KnockedOut)
        {
            // Drops all items this enemy is carrying
            enemyItemInventory.DropAllItems();
        }

        // ----- If any other enemyMovement.currentStatus is detected, resets it to normal.
        else
            enemyMovement.currentStatus = EnemyMovement.Status.Normal;
    }
    

    /// <summary>
    /// Alarms this enemy if it's conscious and triggers its radio cooldown
    /// </summary>
    public override void BecomeAlarmed()
    {
        if(enemyMovement == null) Start();
        if(enemyMovement.IsConscious)
        {
            // On the first time being alarmed, plays the radio loop sound
            if (!alarm.IsOn && !IsAlarmed)
            {
                radioSoundPlayer.Play();
            }
            
            base.BecomeAlarmed();

            // Begins chasing the player unless this enemy specifically ignores the alarm
            if(!ignoresAlarm)
            {
                enemyMovement.currentStatus = EnemyMovement.Status.Chasing;
                aggroTimer = aggroTime;
            }
        }
    }
}
