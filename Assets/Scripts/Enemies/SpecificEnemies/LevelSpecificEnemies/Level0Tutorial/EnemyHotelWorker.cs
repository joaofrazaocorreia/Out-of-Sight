using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyTutorial : EnemyPassive
{
    public override void BecomeAlarmed()
    {
        base.BecomeAlarmed();
        
        alarmedTimer = 0f;
        enemyMovement.Halted = true;
    }
}
