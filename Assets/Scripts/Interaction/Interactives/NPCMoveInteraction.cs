using System.Linq;
using UnityEngine;

public class NPCMoveInteraction : InteractiveObject
{
    [Header("Movement Target")]
    [SerializeField] private MovementTarget enemyPosition;
    [SerializeField] private EnemyMovement enemyMovement;
    [SerializeField] private MovementTarget targetLocation;
    [SerializeField] private string fetchTargetWithTag;
    [SerializeField] private float timeAtTargetLocation = 10f;

    private Enemy enemy;

    private void Start()
    {
        if(enemyMovement != null)
            enemy = enemyMovement.GetComponent<Enemy>();
    }   

    private void Update()
    {
        if (enemyPosition != null)
        {
            if (enemyPosition.CurrentEnemies.Count > 0)
            {
                enemyMovement = enemyPosition.CurrentEnemies.Keys.First();
                enemy = enemyMovement.GetComponent<Enemy>();
            }

            else
            {
                enemy = null;
                enemyMovement = null;
            }
        }

        foreach (InteractiveObject io in GetComponentsInChildren<InteractiveObject>())
            io.enabled = io == this || (enemy != null && enemy.EnemyStatus != Enemy.Status.KnockedOut);
    }

    public override void Interact()
    {
        base.Interact();

        if(fetchTargetWithTag != "")
            targetLocation = GameObject.FindGameObjectWithTag(fetchTargetWithTag).GetComponent<MovementTarget>();

        enemyMovement.DeoccupyCurrentTarget();
        enemyMovement.MovingToSetTarget = true;
        Debug.Log($"{enemyMovement.name} is moving to set target ({targetLocation}).");

        targetLocation.Occupy(enemyMovement);
        enemyMovement.MoveTimer = timeAtTargetLocation;
        enabled = false;
    }
}
