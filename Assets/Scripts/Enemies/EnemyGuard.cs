using UnityEngine;

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

    private float aggroTimer;
    private Vector3 prevPlayerPos;

    
    protected override void Start()
    {
        base.Start();

        type = Type.Guard;
    }


    // Checks this enemy's status and proceeds accordingly.
    protected override void Update()
    {
        // ------------ Normal ------------ 
        if(enemyMovement.currentStatus == EnemyMovement.Status.Normal)
        {
            if(detection.DetectionMeter >= detection.DetectionLimit)
            {
                BecomeAlarmed();
            }
        }

        // ------------ Chasing ------------ 
        else if(enemyMovement.currentStatus == EnemyMovement.Status.Chasing)
        {
            if(detection.SeesPlayer)
            {
                BecomeAlarmed();
            }

            if(enemyMovement.IsAtDestination)
            {
                // Remains at the position if it's still aggro, otherwise it begins searching.
                if(aggroTimer > 0)
                {
                    aggroTimer -= Time.deltaTime;
                }

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
            if(detection.SeesPlayer || Detection.lastPlayerPos != prevPlayerPos)
            {
                prevPlayerPos = Detection.lastPlayerPos;
                BecomeAlarmed();
            }

            
            else if (!alarm.IsOn)
            {
                enemyMovement.currentStatus = EnemyMovement.Status.Normal;
            }
            
        }

        // ------------ Tased && Knocked out ------------ 
        else if(enemyMovement.currentStatus == EnemyMovement.Status.Tased ||
            enemyMovement.currentStatus == EnemyMovement.Status.KnockedOut)
        {
            enemyItemInventory.DropAllItems();
        }

        // ----- If any other enemyMovement.currentStatus is detected, resets it to normal.
        else
            enemyMovement.currentStatus = EnemyMovement.Status.Normal;
    }
    

    public override void BecomeAlarmed()
    {
        if(enemyMovement == null) Start();
        if(!IsKnockedOut && !IsTased)
        {
            base.BecomeAlarmed();

            if(!ignoresAlarm)
            {
                enemyMovement.currentStatus = EnemyMovement.Status.Chasing;
                aggroTimer = aggroTime;
            }
        }
    }
}
