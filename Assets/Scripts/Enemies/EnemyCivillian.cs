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
        if(enemyMovement.currentStatus == EnemyMovement.Status.Normal)
        {
            if(detection.DetectionMeter >= detection.DetectionLimit)
            {
                BecomeAlarmed();
            }
        }


        // ------------ Fleeing ------------ 
        else if(enemyMovement.currentStatus == EnemyMovement.Status.Fleeing)
        {
            // nothing yet
        }

        // ------------ Tased ------------ 
        else if(enemyMovement.currentStatus == EnemyMovement.Status.Tased)
        {
            // nothing yet
        }

        // ------------ Knocked Out ------------ 
        else if(enemyMovement.currentStatus == EnemyMovement.Status.KnockedOut)
        {
            // nothing yet
        }

        // ----- If any other enemyMovement.currentStatus is detected, resets it to normal.
        else
            enemyMovement.currentStatus = EnemyMovement.Status.Normal;
    }
    

    public override void BecomeAlarmed()
    {
        if(!IsKnockedOut && !IsTased)
        {
            base.BecomeAlarmed();

            enemyMovement.CheckNearestExit();
            enemyMovement.currentStatus = EnemyMovement.Status.Fleeing;
        }
    }
}
