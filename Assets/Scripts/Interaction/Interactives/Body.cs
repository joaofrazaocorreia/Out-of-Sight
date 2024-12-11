using UnityEngine;

public class Body : InteractiveObject
{
    [SerializeField] private Player.Disguise disguise;

    private Player player;
    private EnemyMovement enemyMovement;
    private bool hasDisguise;
    public bool HasDisguise {get => hasDisguise;}
    private bool hasBeenDetected;
    public bool HasBeenDetected {get => hasBeenDetected; set{ if(!hasBeenDetected) hasBeenDetected = value;}}

    private void Start()
    {
        player = FindAnyObjectByType<Player>();
        enemyMovement = GetComponentInParent<EnemyMovement>();
        hasDisguise = true;
        hasBeenDetected = false;
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
