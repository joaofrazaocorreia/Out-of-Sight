using UnityEngine;

public class Body : InteractiveObject
{
    [SerializeField] private Player.Disguise disguise;

    private Player player;
    private EnemyMovement enemyMovement;
    private bool hasDisguise;
    public bool HasDisguise {get => hasDisguise;}

    private void Start()
    {
        player = FindAnyObjectByType<Player>();
        enemyMovement = GetComponent<EnemyMovement>();
        hasDisguise = true;
        objectName = disguise + " Disguise";
    }

    public override void Interact()
    {
        if(disguise != player.disguise && hasDisguise &&
            enemyMovement.status == EnemyMovement.Status.KnockedOut)
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
}
