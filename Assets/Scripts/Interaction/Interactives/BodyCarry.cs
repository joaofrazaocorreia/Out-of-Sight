using UnityEngine;

public class BodyCarry : InteractiveObject
{
    private EnemyMovement enemyMovement;
    private PlayerBodyInventory playerBodyInventory;
    private GameObject thisBody;
    private bool hasBeenDetected;
    public bool HasBeenDetected {get => hasBeenDetected; set{ if(!hasBeenDetected) hasBeenDetected = value;}}

    private void Start()
    {
        enemyMovement = GetComponentInParent<EnemyMovement>();
        playerBodyInventory = FindAnyObjectByType<PlayerBodyInventory>();
        thisBody = transform.parent.gameObject;
        hasBeenDetected = false;
        enabled = false;
    }

    public override void Interact()
    {
        if(enemyMovement.currentStatus == EnemyMovement.Status.KnockedOut)
        {
            base.Interact();

            playerBodyInventory.PickUpBody(thisBody);
        }
    }

    public override string GetInteractionText(bool requirementsMet)
    {
        if (!requirementsMet) return "Requires " + GetRequirementNames();

        return "Carry Body";
    }

    public void ResetNPC()
    {
        Start();
    }
}
