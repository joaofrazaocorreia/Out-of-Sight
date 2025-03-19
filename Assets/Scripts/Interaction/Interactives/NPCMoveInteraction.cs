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
        if(enemyMovement.currentStatus == EnemyMovement.Status.KnockedOut)
        {
            gameObject.SetActive(false);
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
