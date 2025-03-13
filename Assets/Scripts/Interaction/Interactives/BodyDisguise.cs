using UnityEngine;

public class BodyDisguise : InteractiveObject
{
    [Header("Disguise Properties")]
    [SerializeField] private Player.Disguise disguise;

    private Player player;
    private EnemyMovement enemyMovement;
    private bool hasDisguise;
    public bool HasDisguise {get => hasDisguise;}

    private void Start()
    {
        player = FindAnyObjectByType<Player>();
        enemyMovement = GetComponentInParent<EnemyMovement>();
        hasDisguise = true;
        objectName = disguise + " Disguise";
        enabled = false;
    }

    public override void Interact()
    {
        if(disguise != player.disguise && hasDisguise &&
            enemyMovement.currentStatus == EnemyMovement.Status.KnockedOut)
        {
            base.Interact();

            hasDisguise = false;
            player.GainDisguise(disguise);
        }
    }

    public override string GetInteractionText(bool requirementsMet)
    {
        if (!requirementsMet) return "Requires " + GetRequirementNames();

        return "Steal " + objectName;
    }

    public void ResetNPC()
    {
        Start();
    }
}
