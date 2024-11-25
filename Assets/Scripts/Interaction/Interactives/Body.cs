using UnityEngine;

public class Body : InteractiveObject
{
    [SerializeField] private Player.Disguise disguise;

    private Player player;
    private EnemyMovement enemyMovement;
    private bool hasDisguise;

    private void Start()
    {
        player = FindAnyObjectByType<Player>();
        enemyMovement = GetComponent<EnemyMovement>();
        hasDisguise = true;
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
}
