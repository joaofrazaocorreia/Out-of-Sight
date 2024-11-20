using UnityEngine;

public class EnemyGuard : Enemy
{
    /// <summary>
    /// How long this enemy stays at the last seen player position before searching around it.
    /// </summary>
    [SerializeField] private float aggroTime = 3f;

    private float aggroTimer;
    private Vector3 prevPlayerPos;

    
    protected override void Start()
    {
        base.Start();

        type = Type.Guard;
        enemyMovement.status = EnemyMovement.Status.Normal;
    }


    // Checks this enemy's status and proceeds accordingly.
    protected override void Update()
    {
        // ------------ Normal ------------ 
        if(enemyMovement.status == EnemyMovement.Status.Normal)
        {
            if(detection.DetectionMeter >= detection.DetectionLimit)
            {
                BecomeAlarmed();
            }


            if(detection.DetectionMeter >= detection.DetectionLimit/3 &&
                enemyMovement.status == EnemyMovement.Status.Normal)
            {
                enemyMovement.halted = true;
            }

            else
                enemyMovement.halted = false;

            
        }

        // ------------ Chasing ------------ 
        else if(enemyMovement.status == EnemyMovement.Status.Chasing)
        {
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
                    enemyMovement.status = EnemyMovement.Status.Searching;
                }
            }

            // Keeps the enemy aggro until it reaches its target.
            else
            {
                aggroTimer = aggroTime;
            }
        }

        // ------------ Searching ------------ 
        else if(enemyMovement.status == EnemyMovement.Status.Searching)
        {
            if(detection.SeesPlayer || Detection.lastPlayerPos != prevPlayerPos)
            {
                prevPlayerPos = Detection.lastPlayerPos;
                BecomeAlarmed();
            }

            /*
            else if (!alarm.IsOn)
            {
                enemyMovement.status = EnemyMovement.Status.Normal;
            }
            */
        }

        // ------------ Knocked Out ------------ 
        else if(enemyMovement.status == EnemyMovement.Status.KnockedOut)
        {
            // dead
        }

        // If any other enemyMovement.status is detected, resets it to normal.
        else
            enemyMovement.status = EnemyMovement.Status.Normal;
    }
    

    public override void BecomeAlarmed()
    {
        base.BecomeAlarmed();

        enemyMovement.status = EnemyMovement.Status.Chasing;
        aggroTimer = aggroTime;
    }
}
