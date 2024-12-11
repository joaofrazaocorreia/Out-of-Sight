using UnityEngine;

public class EnemyPolice : Enemy
{
    /// <summary>
    /// How close this enemy needs to be to the player while chasing to catch them
    /// </summary>
    [SerializeField] private float playerCatchDistance = 1.5f;

    
    protected override void Start()
    {
        base.Start();

        type = Type.Police;
    }


    // Checks this enemy's status and proceeds accordingly.
    protected override void Update()
    {
        // ------------ Chasing ------------ 
        if(enemyMovement.currentStatus == EnemyMovement.Status.Chasing)
        {
            if(detection.SeesPlayer)
            {
                BecomeAlarmed();
            }

            float distanceToPlayer = (Vector3.Scale(transform.position, new Vector3(1f, 0f, 1f))
                - Vector3.Scale(player.transform.position, new Vector3(1f, 0f, 1f))).magnitude;

            if(distanceToPlayer < playerCatchDistance)
            {
                uiManager.Lose();
            }
        }

        // ----- If any other enemyMovement.currentStatus is detected, resets it to chasing.
        else
            enemyMovement.currentStatus = EnemyMovement.Status.Chasing;
    }
    

    public override void BecomeAlarmed()
    {
        if(enemyMovement == null) Start();
        if(!IsKnockedOut && !IsTased)
        {
            base.BecomeAlarmed();

            enemyMovement.currentStatus = EnemyMovement.Status.Chasing;
        }
    }
}
