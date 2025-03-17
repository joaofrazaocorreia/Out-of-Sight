using UnityEngine;

public class EnemyWorker : Enemy
{
    protected override void Start()
    {
        base.Start();

        type = Type.Worker;
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
            if(enemyItemInventory != null)
                enemyItemInventory.DropAllItems();
        }

        // ----- If any other enemyMovement.currentStatus is detected, resets it to normal.
        else
            enemyMovement.currentStatus = EnemyMovement.Status.Normal;
    }
    

    /// <summary>
    /// Alarms this enemy if it's conscious and it begins to flee.
    /// </summary>
    public override void BecomeAlarmed()
    {
        if(enemyMovement.IsConscious)
        {
            base.BecomeAlarmed();

            if(!ignoresAlarm)
                enemyMovement.currentStatus = EnemyMovement.Status.Fleeing;
        }
    }
}

