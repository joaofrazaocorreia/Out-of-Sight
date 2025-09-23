using UnityEngine;
using Enums;

public class BodyDisguise : InteractiveObject
{
    [Header("Disguise Properties")]
    [SerializeField] private Disguise disguise;

    private Player player;
    private Enemy enemySelf;
    private bool hasDisguise;
    public bool HasDisguise {get => hasDisguise;}
    private bool forceDisable;
    public bool ForceDisable {get => forceDisable; set { forceDisable = value; }}

    private void Start()
    {
        player = FindAnyObjectByType<Player>();
        enemySelf = GetComponentInParent<Enemy>();
        hasDisguise = true;
        objectName = disguise + " Disguise";
        forceDisable = false;
        enabled = false;
    }

    public override void Interact()
    {
        if(disguise != player.disguise && hasDisguise &&
            enemySelf.EnemyStatus == Enemy.Status.KnockedOut)
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
