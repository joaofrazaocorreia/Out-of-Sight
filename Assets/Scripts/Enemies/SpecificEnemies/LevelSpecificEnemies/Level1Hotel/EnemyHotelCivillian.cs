using UnityEngine;

public class EnemyHotelCivillian : EnemyPassive
{
    [SerializeField] protected int minTargets = 1;
    [SerializeField] protected int maxTargets = 2;
    protected int targetNum;

    protected override void Start()
    {
        base.Start();

        targetNum = Random.Range(minTargets, maxTargets + 1) + 1;
        Debug.Log("num of targets: " + targetNum);
    }

    // Patrols through a limited number of random movement targets, then leaves the map
    protected override void NormalBehavior()
    {
        enemyMovement.SetMovementSpeed(enemyMovement.WalkSpeed);
        enemyMovement.Halted = false;

        if(targetNum > 0)
        {
            if(enemyMovement.MoveTimer <= Time.deltaTime && ((!enemyMovement.Halted &&
                !enemyMovement.IsStatic) || enemyMovement.MovingToSetTarget))
            {
                targetNum--;
                Debug.Log("targetNum decreased: "+targetNum);
            }

            enemyMovement.Patrol();
        }

        else
        {
            enemyMovement.ExitMap();
            Debug.Log("leaving map");
        }
    }
}
