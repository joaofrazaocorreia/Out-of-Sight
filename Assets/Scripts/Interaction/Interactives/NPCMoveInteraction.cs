using UnityEngine;

public class NPCMoveInteraction : InteractiveObject
{
    [Header("Movement Target")]
    [SerializeField] private EnemyMovement enemyMovement;
    [SerializeField] private Transform targetLocation;
    [SerializeField] private string fetchTargetWithTag;
    [SerializeField] private float timeAtTargetLocation = 10f;

    private void Update()
    {
        if(enabled && enemyMovement.currentStatus == EnemyMovement.Status.KnockedOut)
        {
            foreach(InteractiveObject io in GetComponentsInChildren<InteractiveObject>())
                io.enabled = false;
        }
    }

    public override void Interact()
    {
        base.Interact();

        if(fetchTargetWithTag != "")
            targetLocation = GameObject.FindGameObjectWithTag(fetchTargetWithTag).transform;

        enemyMovement.MovingToSetTarget = true;
        enemyMovement.MoveTo(targetLocation.position);
        enemyMovement.RotateTo(targetLocation.eulerAngles.y);
        enemyMovement.MoveTimer = timeAtTargetLocation;
        enabled = false;
    }
}
