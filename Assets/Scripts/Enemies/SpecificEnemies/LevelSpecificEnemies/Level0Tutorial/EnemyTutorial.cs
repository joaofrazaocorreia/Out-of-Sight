using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyTutorial : EnemyPassive
{
    public override void BecomeAlarmed()
    {
        if (IsConscious)
        {
            if (!IsAlarmed)
            {
                Debug.Log($"{name} was alarmed!");
                if (alarmAudioPlayer != null) alarmAudioPlayer.Play();
            }

            alarmedTimer = 1000000f;
            detection.DetectionMeter = detection.DetectionLimit;
            enemyMovement.Halted = true;
        }
    }
}
