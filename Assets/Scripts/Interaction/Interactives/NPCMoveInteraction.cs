using UnityEngine;

public class NPCMoveInteraction : InteractiveObject
{
    [Header("Movement Target")]
    [SerializeField] private EnemyMovement enemyMovement;
    [SerializeField] private MovementTarget targetLocation;
    [SerializeField] private string fetchTargetWithTag;
    [SerializeField] private float timeAtTargetLocation = 10f;

    private Enemy enemy;

    private void Start()
    {
        enemy = enemyMovement.GetComponent<Enemy>();   
    }

    private void Update()
    {
        if(enabled && enemy.EnemyStatus == Enemy.Status.KnockedOut)
        {
            foreach(InteractiveObject io in GetComponentsInChildren<InteractiveObject>())
                io.enabled = false;
        }
    }

    public override void Interact()
    {
        base.Interact();

        if(fetchTargetWithTag != "")
            targetLocation = GameObject.FindGameObjectWithTag(fetchTargetWithTag).GetComponent<MovementTarget>();

        enemyMovement.MovingToSetTarget = true;
        Debug.Log($"{enemyMovement.name} is moving to set target ({targetLocation}).");
        targetLocation.Occupy(enemyMovement);
        enemyMovement.MoveTimer = timeAtTargetLocation;
        enabled = false;
    }
}
