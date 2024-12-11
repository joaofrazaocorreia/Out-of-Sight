using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMelee : MonoBehaviour
{
    [SerializeField] private float attackRange = 4f;
    [SerializeField] private float frontalAngle = 50f;
    [SerializeField] private Transform enemiesParent;

    private PlayerInput playerInput;
    private List<Transform> allEnemies;
    private List<Transform> enemiesInRange;
    private Transform closestEnemy;
    private float distanceToClosestEnemy;

    private void Start()
    {
        playerInput = FindAnyObjectByType<PlayerInput>();
        allEnemies = new List<Transform>();
        enemiesInRange = new List<Transform>();

        UpdateEnemiesList();
    }

    private void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * attackRange, Color.white);

        CheckForAttack();
    }

    private void CheckForAttack()
    {
        if(playerInput.actions["Attack"].WasPressedThisFrame())
        {
            Debug.Log("Player used Melee attack");

            distanceToClosestEnemy = float.MaxValue;
            closestEnemy = null;

            if(enemiesParent.childCount != allEnemies.Count)
                UpdateEnemiesList();

            if(allEnemies.Count > 0)
            {
                enemiesInRange = allEnemies.Where(enemy => 
                    (Vector3.Scale(enemy.position, new Vector3(1, 0, 1))
                        - Vector3.Scale(transform.position, new Vector3(1, 0, 1))).magnitude <= attackRange).ToList();
            }

            // Checks for every enemy within range of the player's detection range
            foreach(Transform enemy in enemiesInRange)
            {
                Vector3 distanceToEnemy = enemy.position - transform.position;

                // Checks if the enemy is within the player's field of view
                if(Vector3.Angle(transform.TransformDirection(Vector3.forward),
                    Vector3.Scale(enemy.position, new Vector3(1, 0, 1))
                        - Vector3.Scale(transform.position, new Vector3(1, 0, 1))) <= frontalAngle)
                {
                    // Sends a raycast towards the enemy
                    if (Physics.Raycast(transform.position, distanceToEnemy, out RaycastHit hit, attackRange*2))
                    {
                        // Checks if the raycast hit the enemy
                        if (hit.transform == enemy || hit.transform.parent == enemy)
                        {
                            if(distanceToClosestEnemy > distanceToEnemy.magnitude)
                            {
                                closestEnemy = enemy;
                                distanceToClosestEnemy = distanceToEnemy.magnitude;
                            }
                        }
                    }
                }
            }

            // Knocks out the enemy if they're not already knocked out
            if(closestEnemy != null)
            {
                EnemyMovement enemyMovement = closestEnemy.GetComponent<EnemyMovement>();
                
                if(enemyMovement.currentStatus != EnemyMovement.Status.KnockedOut)
                    enemyMovement.currentStatus = EnemyMovement.Status.KnockedOut;

                // TEMPORARY - moves the body away from the level when using meelee on it, this will be replaced by the body-carrying mechanic later
                else
                {
                    enemyMovement.gameObject.SetActive(false);
                    enemyMovement.transform.position = new Vector3(1000, 1000, 1000);
                    enemyMovement.gameObject.SetActive(true);
                }
            }
        }
    }

    private void UpdateEnemiesList()
    {
        allEnemies = new List<Transform>();
        
        for(int i = 0; i < enemiesParent.childCount; i++)
        {
            if(enemiesParent.GetChild(i).GetComponent<EnemyMovement>().currentStatus != EnemyMovement.Status.KnockedOut)
                allEnemies.Add(enemiesParent.GetChild(i));
        }
    }
}
