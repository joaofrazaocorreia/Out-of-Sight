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
        if(EnemyStatus == Status.Normal)
        {
            if(detection.DetectionMeter >= detection.DetectionLimit)
            {
                BecomeAlarmed();
            }
        }


        // ------------ Fleeing ------------ 
        else if(EnemyStatus == Status.Fleeing)
        {
            // nothing yet
        }

        // ------------ Knocked out ------------ 
        else if(EnemyStatus == Status.KnockedOut)
        {
            // Drops all items this enemy is carrying
            enemyItemInventory.DropAllItems();
        }

        // ----- If any other enemyMovement.currentStatus is detected, resets it to normal.
        else
            EnemyStatus = Status.Normal;
    }
    

    /// <summary>
    /// Alarms this enemy and makes it flee.
    /// </summary>
    public override void BecomeAlarmed()
    {
        if(IsConscious)
        {
            base.BecomeAlarmed();

            EnemyStatus = Status.Fleeing;
        }
    }
}
