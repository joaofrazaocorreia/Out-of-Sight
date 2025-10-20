using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyTutorial : EnemyPassive
{
    protected override void NormalBehavior()
    {
        if (enemyMovement.WalkSpeed != 0)
        {
            base.NormalBehavior();
        }

        else
        {
            StopMoving();
        }
    }

    protected override void SuspectfulBehavior()
    {
        if (IsAlarmed)
            StopMoving();

        else if (enemyMovement.WalkSpeed > 0)
            base.SuspectfulBehavior();
    }

    public override void BecomeAlarmed()
    {
        if (IsConscious)
        {
            if (!IsAlarmed)
            {
                Debug.Log($"{name} was alarmed!");
                if (alarmAudioPlayer != null) alarmAudioPlayer.Play();
                onBecomeAlarmed?.Invoke();
            }

            alarmedTimer = 1000000f;
            detection.DetectionMeter = detection.DetectionLimit;
            StopMoving();
        }
    }

    private void StopMoving()
    {
        enemyMovement.SetMovementSpeed(0);
        enemyMovement.Halt();
        enemyMovement.Halted = true;
        enemyMovement.MoveTimer = 0.01f;
    }
}
