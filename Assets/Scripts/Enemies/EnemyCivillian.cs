using UnityEngine;

public class EnemyCivillian : Enemy
{
    protected override void Start()
    {
        base.Start();

        type = Type.Civillian;
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
        }

        // ------------ Scared ------------ 
        else if(enemyMovement.status == EnemyMovement.Status.Scared)
        {
            // run away from player until it's distant enough or stops seeing them
        }

        // ------------ Fleeing ------------ 
        else if(enemyMovement.status == EnemyMovement.Status.Fleeing)
        {
            // run towards exit, becomes scared if sees player too close
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

        enemyMovement.status = EnemyMovement.Status.Fleeing;
    }
}
