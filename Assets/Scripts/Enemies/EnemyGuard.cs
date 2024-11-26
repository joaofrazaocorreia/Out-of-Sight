using UnityEngine;

public class EnemyGuard : Enemy
{
    /// <summary>
    /// How long this enemy stays at the last seen player position before searching around it.
    /// </summary>
    [SerializeField] private float aggroTime = 5f;
    /// <summary>
    /// How close this enemy needs to be to the player while chasing to catch them
    /// </summary>
    [SerializeField] private float playerCatchDistance = 1.5f;

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
        if(enemyMovement.status == EnemyMovement.Status.Normal)
        {
            if(detection.DetectionMeter >= detection.DetectionLimit)
            {
                BecomeAlarmed();
            }


            if(detection.DetectionMeter >= detection.DetectionLimit * 2 / 3 &&
                enemyMovement.status == EnemyMovement.Status.Normal)
            {
                enemyMovement.halted = true;
                //transform.LookAt(Detection.lastPlayerPos);
                enemyMovement.MoveTo(Detection.lastPlayerPos);
            }

            else if(detection.DetectionMeter >= detection.DetectionLimit * 1 / 3 &&
                enemyMovement.status == EnemyMovement.Status.Normal)
            {
                enemyMovement.halted = true;
                //transform.LookAt(Detection.lastPlayerPos);
            }

            else
                enemyMovement.halted = false;

            
        }

        // ------------ Chasing ------------ 
        else if(enemyMovement.status == EnemyMovement.Status.Chasing)
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
                    enemyMovement.status = EnemyMovement.Status.Searching;
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
        else if(enemyMovement.status == EnemyMovement.Status.Searching)
        {
            if(detection.SeesPlayer || Detection.lastPlayerPos != prevPlayerPos)
            {
                prevPlayerPos = Detection.lastPlayerPos;
                BecomeAlarmed();
            }

            
            else if (!alarm.IsOn)
            {
                enemyMovement.status = EnemyMovement.Status.Normal;
            }
            
        }

        // ------------ Tased ------------ 
        else if(enemyMovement.status == EnemyMovement.Status.Tased)
        {
            if(tasedTimer > 0)
            {
                animator.SetBool("Tased", true);
                animator.applyRootMotion = false;
                tasedTimer -= Time.deltaTime;
            }

            else
            {
                animator.SetBool("Tased", false);
                animator.applyRootMotion = true;

                tasedTimer = tasedTime;
                BecomeAlarmed();
            }
        }

        // ------------ Knocked Out ------------ 
        else if(enemyMovement.status == EnemyMovement.Status.KnockedOut)
        {
            animator.SetTrigger("KO");
            animator.applyRootMotion = false;
        }

        // ----- If any other enemyMovement.status is detected, resets it to normal.
        else
            enemyMovement.status = EnemyMovement.Status.Normal;
    }
    

    public override void BecomeAlarmed()
    {
        if(enemyMovement.status != EnemyMovement.Status.KnockedOut && enemyMovement.status != EnemyMovement.Status.Tased)
        {
            base.BecomeAlarmed();

            enemyMovement.status = EnemyMovement.Status.Chasing;
            aggroTimer = aggroTime;
        }
    }
}
