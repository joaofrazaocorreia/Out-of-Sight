using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMelee : MonoBehaviour
{
    [SerializeField] private float attackRange = 4f;
    [SerializeField] private float frontalAngle = 120f;
    [SerializeField] [Range(0f, 360f)] private float enemyBackDetectionAngle = 100f;
    [SerializeField] private float cooldownInSeconds = 1f;
    [SerializeField] private Transform enemiesParent;
    [SerializeField] private PlayAudio meleeAttackPlayer;

    private PlayerInput playerInput;
    private PlayerInteraction playerInteraction;
    private PlayerCarryInventory playerCarryInventory;
    /*private List<Transform> allEnemies;
    private List<Transform> enemiesInRange;
    private List<Detection> enemiesWatching;
    public List<Detection> EnemiesWatching {get => enemiesWatching;}
    private Transform closestEnemy;
    private float distanceToClosestEnemy;*/

    private bool canAttack;
    
    private float cooldownTimer;
    
    private EnemyMovement attackableEnemy;
    public event EventHandler OnAttackAvailable;
    public event EventHandler OnAttackNotAvailable;
    public event EventHandler OnKnockout;

    private void Start()
    {
        playerInput = FindAnyObjectByType<PlayerInput>();
        playerInteraction = FindAnyObjectByType<PlayerInteraction>();
        playerCarryInventory = FindAnyObjectByType<PlayerCarryInventory>();
        /*allEnemies = new List<Transform>();
        enemiesInRange = new List<Transform>();
        enemiesWatching = new List<Detection>();*/
        cooldownTimer = 0;

        //UpdateEnemiesList();
    }

    private void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * attackRange, Color.white);

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
            print("1: " + (angleDifference <= enemyBackDetectionAngle / 2) );
            print(angleDifference >= 360 - enemyBackDetectionAngle / 2);
            
            if ((angleDifference <= enemyBackDetectionAngle / 2 ||
                 angleDifference >= 360 - enemyBackDetectionAngle / 2)
                && attackableEnemy.currentStatus != EnemyMovement.Status.KnockedOut)
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
            if ((enemy = hit.GetComponentInParent<Enemy>()) != null && !enemy.IsKnockedOut) attackableEnemy = hit.GetComponentInParent<EnemyMovement>();
            return;
        }
    }

    private void CheckForAttack()
    {
        if(playerInput.actions["Attack"].WasPressedThisFrame() && cooldownTimer <= 0)
        {
            if(playerCarryInventory.CarryingBody)
            {
                Debug.Log("Player dropped a body");

                playerCarryInventory.DropCarriable();
            }

            else if(canAttack)
            {
                Debug.Log("Player used Melee attack");
                if(attackableEnemy is null) return;
                
                    attackableEnemy.GetKnockedOut();
                    OnKnockout?.Invoke(this, EventArgs.Empty);
                

                    /*distanceToClosestEnemy = float.MaxValue;
                    closestEnemy = null;

                    UpdateEnemiesList();

                    // Checks for every enemy within range of the player's melee range
                    foreach(Transform enemy in enemiesInRange)
                    {
                        Vector3 distanceToEnemy = enemy.position - transform.position;

                        // Checks if the enemy is within the player's field of view
                        if(Vector3.Angle(transform.TransformDirection(Vector3.forward),
                            Vector3.Scale(enemy.position, new Vector3(1, 0, 1))
                                - Vector3.Scale(transform.position, new Vector3(1, 0, 1))) <= frontalAngle ||
                                    (enemy.transform.position - transform.position).magnitude <= 1.5f)
                        {
                            // Sends a raycast towards the enemy
                            if (Physics.Raycast(transform.position, distanceToEnemy, out RaycastHit hit, attackRange*2))
                            {
                                // Checks if the raycast hit the enemy
                                if (hit.transform == enemy || hit.transform.parent == enemy)
                                {
                                    // Registers the hit enemy as the closest enemy
                                    if(distanceToClosestEnemy > distanceToEnemy.magnitude)
                                    {
                                        closestEnemy = enemy;
                                        distanceToClosestEnemy = distanceToEnemy.magnitude;
                                    }
                                }
                            }
                        }
                    }

                    // Checks if there is an enemy in range and in the player's field of view
                    if(closestEnemy != null)
                    {
                        EnemyMovement enemyMovement = closestEnemy.GetComponent<EnemyMovement>();
                        float angleDifference = Mathf.Abs(enemyMovement.transform.eulerAngles.y - transform.eulerAngles.y);

                        // Knocks out the enemy if their back is facing the player and they aren't KO already
                        if((angleDifference <= enemyBackDetectionAngle/2 || angleDifference >= 360 - enemyBackDetectionAngle/2)
                            && enemyMovement.currentStatus != EnemyMovement.Status.KnockedOut)
                        {
                            enemyMovement.GetKnockedOut();

                            // Immediately alerts any conscious enemy that sees the knockout
                            if(GetComponent<Player>().detectable)
                                AlarmNearbyEnemies();
                        }
                    }*/
            }

            cooldownTimer = cooldownInSeconds;
            meleeAttackPlayer.Play();
            attackableEnemy = null;
        }
    }
    /*
    public void AlarmNearbyEnemies()
    {
        if(GetComponent<Player>().detectable)
        {
            UpdateEnemiesList();

            // Immediately alerts any conscious enemy that sees the player currently
            foreach(Detection d in enemiesWatching)
            {
                Debug.Log($"{d.transform.parent.name} saw the melee attack!");
                d.DetectionMeter = d.DetectionLimit;
            }
        }
    }

    private void UpdateEnemiesList()
    {
        allEnemies = new List<Transform>();
        enemiesInRange = new List<Transform>();
        enemiesWatching = new List<Detection>();
        
        for(int i = 0; i < enemiesParent.childCount; i++)
        {
            if(enemiesParent.GetChild(i).GetComponent<EnemyMovement>().currentStatus != EnemyMovement.Status.KnockedOut)
                allEnemies.Add(enemiesParent.GetChild(i));
        }

        if(allEnemies.Count > 0)
        {
            enemiesInRange = allEnemies.Where(enemy => 
                (Vector3.Scale(enemy.position, new Vector3(1, 0, 1))
                    - Vector3.Scale(transform.position, new Vector3(1, 0, 1))).magnitude <= attackRange).ToList();
            
            foreach(Transform t in allEnemies)
            {
                Detection d = t.GetComponentInChildren<Detection>();
                EnemyMovement em = t.GetComponent<EnemyMovement>();

                if(d.SeesPlayer && em.IsConscious)
                    enemiesWatching.Add(d);
            }
        }
    }*/
    
}
