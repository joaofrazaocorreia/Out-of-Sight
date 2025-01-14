using UnityEngine;

public class NPCInteraction : InteractiveObject
{
    [Header("Movement Target")]
    [SerializeField] private Transform targetLocation;
    [SerializeField] private string fetchTargetWithTag;
    private EnemyMovement enemyMovement;

    private void Start()
    {
        enemyMovement = GetComponentInParent<EnemyMovement>();
    }

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
        gameObject.SetActive(false);
    }

    public override string GetInteractionText(bool requirementsMet)
    {
        if (!requirementsMet) return $"Maybe I can distract them...";

        return "Give \"lost\" luggage to clerk";
    }
}
