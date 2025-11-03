using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMelee : MonoBehaviour
{
    [SerializeField] [Range(0f, 360f)] private float enemyBackDetectionAngle = 100f;
    [SerializeField] private float cooldownInSeconds = 1f;
    [SerializeField] private PlayAudio meleeAttackPlayer;
    [SerializeField] private AudioClip sucessfulAttackSound;

    private PlayerInput playerInput;
    private PlayerInteraction playerInteraction;
    private PlayerCarryInventory playerCarryInventory;
    [SerializeField] private Animator animator;

    private bool canAttack;
    
    private float cooldownTimer;
    
    private Enemy attackableEnemy;
    public event EventHandler OnAttackAvailable;
    public event EventHandler OnAttackNotAvailable;
    public event EventHandler OnKnockout;

    public event EventHandler OnAttackEnd;

    private void Start()
    {
        playerInput = FindAnyObjectByType<PlayerInput>();
        playerInteraction = FindAnyObjectByType<PlayerInteraction>();
        playerCarryInventory = FindAnyObjectByType<PlayerCarryInventory>();
        cooldownTimer = 0;
    }

    private void Update()
    {
        if(cooldownTimer > 0)
            cooldownTimer -= Time.deltaTime;
        
        GetAttackableNpc();
        CheckCanAttack();
        CheckForAttack();
    }

    private void CheckCanAttack()
    {
        
        if (cooldownTimer <= 0 && attackableEnemy != null && !playerCarryInventory.CarryingBody)
        {
            float angleDifference = Mathf.Abs(attackableEnemy.transform.eulerAngles.y - transform.eulerAngles.y);
            
            if ((angleDifference <= enemyBackDetectionAngle / 2 ||
                 angleDifference >= 360 - enemyBackDetectionAngle / 2)
                && attackableEnemy.EnemyStatus != Enemy.Status.KnockedOut)
            {
                canAttack = true;
                OnAttackAvailable?.Invoke(this, EventArgs.Empty);
                return;
            }
            
        }
        
        canAttack = false;
        OnAttackNotAvailable?.Invoke(this, EventArgs.Empty);
    }

    private void GetAttackableNpc()
    {
        if (playerInteraction.HitInteractables == null)
        {
            attackableEnemy = null;
            return;
        }
        
        for (int i = 0; i < playerInteraction.HitInteractables.Length; i++)
        {
            var hit = playerInteraction.HitInteractables[i];
            Enemy enemy;
            if (hit == null) continue;
            if ((enemy = hit.GetComponentInParent<Enemy>()) == null) return;
            if (enemy is EnemyCamera) return;
            if (!enemy.IsKnockedOut) attackableEnemy = hit.GetComponentInParent<Enemy>();
        }
    }

    private void CheckForAttack()
    {
        if(playerInput.actions["Attack"].WasPressedThisFrame() && cooldownTimer <= 0 &&
            (playerCarryInventory.CarryingBody || canAttack))
        {
            if(playerCarryInventory.CarryingBody)
            {
                Debug.Log("Player dropped a body");

                playerCarryInventory.DropCarriable();
            }

            else if(canAttack && attackableEnemy != null)
            {
                Debug.Log("Player used Melee attack");
                
                attackableEnemy.GetKnockedOut();
                OnKnockout?.Invoke(this, EventArgs.Empty);

                meleeAttackPlayer.PlayOneShot(sucessfulAttackSound);
                
                animator.SetTrigger("Melee");
                StartCoroutine(CheckDelay());
            }

            cooldownTimer = cooldownInSeconds;
            meleeAttackPlayer.Play();
            attackableEnemy = null;
        }
    }

    private IEnumerator CheckDelay()
    {
        yield return new WaitForSeconds(0.05f);
        StartCoroutine(OnCompleteAttackAnimation());
    }

    private IEnumerator OnCompleteAttackAnimation()
    {
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(1).normalizedTime < 1.0f);
        yield return new WaitForSeconds(0.25f);
        OnAttackEnd?.Invoke(this, EventArgs.Empty);
    }
}
