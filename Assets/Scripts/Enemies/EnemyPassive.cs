
using UnityEngine;

public class EnemyPassive : Enemy
{
    public override void BecomeAlarmed()
    {
        if(IsConscious)
        {
            base.BecomeAlarmed();

            EnemyStatus = Status.Fleeing;
        }
    }

    protected override void NormalBehavior()
    {
        if(detection.DetectionMeter >= detection.DetectionLimit)
        {
            BecomeAlarmed();
        }

        base.NormalBehavior();
    }
}
