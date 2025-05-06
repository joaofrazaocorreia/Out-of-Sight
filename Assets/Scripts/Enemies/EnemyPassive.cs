
public class EnemyPassive : Enemy
{
    public override void BecomeAlarmed()
    {
        if(IsConscious)
        {
            base.BecomeAlarmed();
            
            if(!ignoresAlarm)
            {
                EnemyStatus = Status.Fleeing;
            }
        }
    }

    protected override void Update()
    {
        if(detection.DetectionMeter >= detection.DetectionLimit)
        {
            BecomeAlarmed();
        }

        base.Update();
    }
}
