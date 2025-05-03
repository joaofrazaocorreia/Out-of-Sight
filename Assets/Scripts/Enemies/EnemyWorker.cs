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
            if(enemyItemInventory != null)
                enemyItemInventory.DropAllItems();
        }

        // ----- If any other enemyMovement.currentStatus is detected, resets it to normal.
        else
            EnemyStatus = Status.Normal;
    }
    

    /// <summary>
    /// Alarms this enemy if it's conscious and it begins to flee.
    /// </summary>
    public override void BecomeAlarmed()
    {
        if(IsConscious)
        {
            base.BecomeAlarmed();

            if(!ignoresAlarm)
                EnemyStatus = Status.Fleeing;
        }
    }
}

