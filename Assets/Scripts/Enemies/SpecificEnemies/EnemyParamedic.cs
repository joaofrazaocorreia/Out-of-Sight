using UnityEngine;

public class EnemyParamedic : EnemyPassive
{
    [SerializeField] private float bodyGrabDistance = 5f;
    [SerializeField] private float bodyGrabTime = 3f;
    [SerializeField] private Transform bodyHoldPos;
    private BodyCarry bodyTarget;
    public BodyCarry BodyTarget {get => bodyTarget; set => bodyTarget = value;}
    private float bodyGrabTimer;

    protected override void Start()
    {
        base.Start();

        bodyGrabTimer = bodyGrabTime;
    }

    protected override void NormalBehavior()
    {
        base.NormalBehavior();

        if(bodyTarget != null)
        {
            if((bodyTarget.transform.position - transform.position)
                .magnitude <= bodyGrabDistance)
            {
                if(bodyGrabTimer <= 0)
                {
                    bodyGrabTimer = bodyGrabTime;
                    
                    bodyTarget.transform.parent = bodyHoldPos;
                    bodyTarget.transform.position = bodyHoldPos.position;
                    bodyTarget.transform.rotation = bodyHoldPos.rotation;

                    alarm.UnregisterEnemy(bodyTarget.GetComponent<Enemy>());
                    bodyTarget.GetComponent<EnemyMovement>().ToggleRagdoll(false);
                    bodyTarget = null;
                    
                    enemyMovement.ExitMap();
                }

                else
                {
                    bodyGrabTimer -= Time.deltaTime;
                }
            }
        }

        else enemyMovement.ExitMap();
    }
}
