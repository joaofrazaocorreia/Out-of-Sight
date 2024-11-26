using UnityEngine;

public class EnemyCivillian : Enemy
{
    protected override void Start()
    {
        base.Start();

        type = Type.Civillian;
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


        // ------------ Fleeing ------------ 
        else if(enemyMovement.status == EnemyMovement.Status.Fleeing)
        {
            // nothing yet
        }

        // ------------ Tased ------------ 
        else if(enemyMovement.status == EnemyMovement.Status.Tased)
        {
            if(tasedTimer > 0)
                tasedTimer -= Time.deltaTime;

            else BecomeAlarmed();
        }

        // ------------ Knocked Out ------------ 
        else if(enemyMovement.status == EnemyMovement.Status.KnockedOut)
        {
            // dead
        }

        // ----- If any other enemyMovement.status is detected, resets it to normal.
        else
            enemyMovement.status = EnemyMovement.Status.Normal;
    }
    

    public override void BecomeAlarmed()
    {
        base.BecomeAlarmed();

        enemyMovement.CheckNearestExit();
        enemyMovement.status = EnemyMovement.Status.Fleeing;
    }
}
