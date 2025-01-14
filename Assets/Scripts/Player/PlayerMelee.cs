using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMelee : MonoBehaviour
{
    [SerializeField] private float attackRange = 4f;
    [SerializeField] private float frontalAngle = 120f;
    [SerializeField] private float cooldownInSeconds = 1f;
    [SerializeField] private Transform enemiesParent;
    [SerializeField] private PlayAudio meleeAttackPlayer;

    private PlayerInput playerInput;
    private PlayerCarryInventory playerCarryInventory;
    private List<Transform> allEnemies;
    private List<Transform> enemiesInRange;
    private List<Detection> enemiesWatching;
    private Transform closestEnemy;
    private float distanceToClosestEnemy;
    private float cooldownTimer;

    private void Start()
    {
        playerInput = FindAnyObjectByType<PlayerInput>();
        playerCarryInventory = FindAnyObjectByType<PlayerCarryInventory>();
        allEnemies = new List<Transform>();
        enemiesInRange = new List<Transform>();
        enemiesWatching = new List<Detection>();
        cooldownTimer = 0;

        UpdateEnemiesList();
    }

    private void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * attackRange, Color.white);

        if(cooldownTimer > 0)
            cooldownTimer -= Time.deltaTime;

        CheckForAttack();
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

            else
            {
                Debug.Log("Player used Melee attack");

                distanceToClosestEnemy = float.MaxValue;
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
                    
                    // Knocks out the enemy if they aren't KO already
                    if(enemyMovement.currentStatus != EnemyMovement.Status.KnockedOut)
                    {
                        enemyMovement.currentStatus = EnemyMovement.Status.KnockedOut;
                        closestEnemy.GetComponentInChildren<Detection>().DetectionMeter = 0;
                        UpdateEnemiesList();
                
                        // Immediately alerts any living enemy that sees the player knocking out another NPC
                        foreach(Detection d in enemiesWatching)
                        {
                            Debug.Log($"{d.transform.parent.name} saw the melee attack!");
                            d.DetectionMeter = d.DetectionLimit;
                        }
                    }
                }
            }

            cooldownTimer = cooldownInSeconds;
            meleeAttackPlayer.Play();
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
    }
}
