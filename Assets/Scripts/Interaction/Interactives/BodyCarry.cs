using UnityEngine;

public class BodyCarry : InteractiveObject
{
    private EnemyMovement enemyMovement;
    private PlayerCarryInventory playerCarryInventory;
    private GameObject thisBody;
    private bool hasBeenDetected;
    public bool HasBeenDetected {get => hasBeenDetected; set{ if(!hasBeenDetected) hasBeenDetected = value;}}

    private void Start()
    {
        enemyMovement = GetComponentInParent<EnemyMovement>();
        playerCarryInventory = FindAnyObjectByType<PlayerCarryInventory>();
        thisBody = transform.parent.gameObject;
        hasBeenDetected = false;
        enabled = false;
    }

    public override void Interact()
    {
        if(enemyMovement.currentStatus == EnemyMovement.Status.KnockedOut && !playerCarryInventory.CarryingBody)
        {
            base.Interact();

            playerCarryInventory.PickUpBody(thisBody);
        }
    }

    public override string GetInteractionText(bool requirementsMet)
    {
        if (!requirementsMet) return "Requires " + GetRequirementNames();

        else if(playerCarryInventory.CarryingBody) return "Already carrying a body!";

        return "Carry Body";
    }

    public void ResetNPC()
    {
        Start();
    }
}
