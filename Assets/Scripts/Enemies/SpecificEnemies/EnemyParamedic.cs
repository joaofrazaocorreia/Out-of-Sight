using UnityEngine;

public class EnemyParamedic : EnemyPassive
{
    [Header("Paramedic Variables")]
    [SerializeField] private float bodyGrabDistance = 5f;
    [SerializeField] private float bodyGrabTime = 3f;
    [SerializeField] private Transform bodyHoldPos;
    private Animator animator;
    private BodyCarry bodyTarget;
    public BodyCarry BodyTarget {get => bodyTarget; set => bodyTarget = value;}
    private float bodyGrabTimer;
    private bool hasReachedBody;

    protected override void Start()
    {
        base.Start();

        animator = GetComponentInChildren<Animator>();
        bodyGrabTimer = bodyGrabTime;
        hasReachedBody= false;
    }

    protected override void NormalBehavior()
    {
        base.NormalBehavior();

        if(bodyTarget != null)
        {
            if((bodyTarget.transform.position - transform.position)
                .magnitude <= bodyGrabDistance)
            {
                if(!hasReachedBody)
                {
                    animator.speed = 0.3f;
                    animator.SetTrigger("Interacting");
                    hasReachedBody = true;
                }

                if(bodyGrabTimer <= 0)
                {
                    Transform body = bodyTarget.GetComponentInParent<Enemy>().transform;
                    
                    animator.speed = 1f;
                    bodyGrabTimer = bodyGrabTime;
                    
                    body.parent = bodyHoldPos;
                    body.position = bodyHoldPos.position;
                    body.rotation = bodyHoldPos.rotation;

                    alarm.UnregisterEnemy(bodyTarget.GetComponent<Enemy>());
                    body.GetComponent<EnemyMovement>().ToggleRagdoll(false);
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
