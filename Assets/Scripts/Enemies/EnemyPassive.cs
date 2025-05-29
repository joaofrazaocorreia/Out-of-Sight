using System.Collections.Generic;
using UnityEngine;

public class EnemyPassive : Enemy
{
    [Header("Passive Enemy Variables")]
    [SerializeField] protected List<MovementTarget> restingTargets;
    [SerializeField] [Min(0)] protected float startRestingTimer = 15f;
    [SerializeField] [Min(0)] protected float minRestingTimer = 30f;
    [SerializeField] [Min(0)] protected float maxRestingTimer = 180f;

    protected override void Start()
    {
        base.Start();

        AddNecessity(() =>
        {
            if (restingTargets.Count > 0 && !ignoresAlarm)
            {
                Debug.Log($"{name} is going to take a break!");
                enemyMovement.PickTarget(restingTargets, false, true);
            }
        },
            startRestingTimer, minRestingTimer, maxRestingTimer);
    }
    public override void BecomeAlarmed()
    {
        if (IsConscious)
        {
            base.BecomeAlarmed();

            if (!ignoresAlarm)
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
