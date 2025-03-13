using UnityEngine;

public class NPCInteraction : InteractiveObject
{
    [Header("Movement Target")]
    [SerializeField] private EnemyMovement enemyMovement;
    [SerializeField] private Transform targetLocation;
    [SerializeField] private string fetchTargetWithTag;

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
        enabled = false;
    }
}
