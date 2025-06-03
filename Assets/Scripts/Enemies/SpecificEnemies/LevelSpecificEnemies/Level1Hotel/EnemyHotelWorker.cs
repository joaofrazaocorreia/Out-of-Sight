using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyHotelWorker : EnemyPassive
{
    [Header("Hotel Employee Variables")]
    [SerializeField] protected List<MovementTarget> jobTargets;
    [SerializeField] protected MovementTarget conciergeTarget;
    [SerializeField] protected MovementTarget startingTarget;

    private NPCMoveInteraction luggageInteractible;

    protected override void Start()
    {
        base.Start();

        if (startingTarget != null)
        {
            startingTarget.Occupy(enemyMovement, true);
        }
        else
            new WaitForEndOfFrame();

        luggageInteractible = GetComponentInChildren<NPCMoveInteraction>();
    }

    // Patrols through a limited number of random movement targets, then leaves the map
    protected override void NormalBehavior()
    {
        enemyMovement.SetMovementSpeed(enemyMovement.WalkSpeed);
        enemyMovement.Halted = false;

        List<MovementTarget> emptyMainTargets = jobTargets.Where(m => !m.Occupied).ToList();

        if (emptyMainTargets.Count > 0 && !jobTargets.Contains(enemyMovement.CurrentTarget))
        {
            enemyMovement.PickTarget(emptyMainTargets, false, true);
        }

        else if (enemyMovement.MoveTimer <= 0 && jobTargets.Contains(enemyMovement.CurrentTarget))
        {
            enemyMovement.CurrentTarget.Occupy(enemyMovement, true);
        }
            
        
        enemyMovement.Patrol();
        enemyMovement.RotateToCurrentTarget();

        luggageInteractible.enabled = enemyMovement.CurrentTarget == conciergeTarget && enemyMovement.IsAtDestination;
    }
}
