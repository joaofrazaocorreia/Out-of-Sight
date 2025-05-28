using UnityEngine;

public class EnemyHotelCivillian : EnemyPassive
{
    [Header("Hotel Civillian Variables")]
    [SerializeField] protected int minTargets = 1;
    [SerializeField] protected int maxTargets = 2;
    protected int targetNum;

    protected override void Start()
    {
        base.Start();

        targetNum = Random.Range(minTargets, maxTargets + 1) + 1; // extra target because the NPC spawn point is a target

        enemyMovement.onChooseNewTarget.RemoveListener(DecreaseTargetNum); // makes sure it only has one of these listeners
        enemyMovement.onChooseNewTarget.AddListener(DecreaseTargetNum);
    }

    // Patrols through a limited number of random movement targets, then leaves the map
    protected override void NormalBehavior()
    {
        enemyMovement.SetMovementSpeed(enemyMovement.WalkSpeed);
        enemyMovement.Halted = false;

        if (targetNum > 0)
            enemyMovement.Patrol();

        else
            enemyMovement.ExitMap();
    }

    private void DecreaseTargetNum()
    {
        targetNum--;
    }
}
