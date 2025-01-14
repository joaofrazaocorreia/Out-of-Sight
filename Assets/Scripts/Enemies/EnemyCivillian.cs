using UnityEngine;

public class EnemyCivillian : Enemy
{
    protected override void Start()
    {
        base.Start();

        type = Type.Civillian;
    }


    protected override void Update()
    {
        CheckStatus();
    }

    // Checks this enemy's status and proceeds accordingly.
    private void CheckStatus()
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
    /// Alarms this enemy and makes it flee.
    /// </summary>
    public override void BecomeAlarmed()
    {
        if(enemyMovement.IsConscious)
        {
            base.BecomeAlarmed();

            enemyMovement.currentStatus = EnemyMovement.Status.Fleeing;
        }
    }
}
