using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovementTarget : MonoBehaviour
{
    // [SerializeField] private float minStayDuration = 10;
    [SerializeField] private List<Transform> targetPositions;
    private Dictionary<EnemyMovement, Transform> currentEnemies;
    public bool Occupied
    {
        get => currentEnemies.Count >= targetPositions.Count;
    }

    private void Start()
    {
        currentEnemies = new Dictionary<EnemyMovement, Transform>();

        if(targetPositions == null || targetPositions.Count <= 0)
        {
            GameObject newPosition = new();
            newPosition.transform.position = transform.position;
            newPosition.transform.rotation = transform.rotation;
            newPosition.transform.parent = transform;
            newPosition.name = "Count Sexulla III the Nickelmeyer";

            targetPositions = new List<Transform> { newPosition.transform };
        }
    }

    public Transform Occupy(EnemyMovement enemy, bool canChooseLastPos = false, float moveTimeMultiplier = 1f)
    {
        Transform targetPos = GetAvailablePos();

        if(targetPos != null)
        {
            enemy.DeoccupyCurrentTarget();

            currentEnemies.Add(enemy, targetPos);
            enemy.MoveTo(targetPos.position, canChooseLastPos, moveTimeMultiplier);
            enemy.RotateTo(targetPos.eulerAngles.y);
        }

        return targetPos;
    }

    public void Deoccupy(EnemyMovement enemy)
    {
        if(currentEnemies.Keys.Contains(enemy))
            currentEnemies.Remove(enemy);
    }

    private Transform GetAvailablePos()
    {
        Transform availablePos = null;

        foreach(Transform t in targetPositions)
        {
            if(!currentEnemies.Values.Contains(t))
                availablePos = t;
        }

        return availablePos;
    }

    /// <summary>
    /// Creates a new movement target at a given position with a given rotation and parent.
    /// </summary>
    /// <param name="position">The position to spawn the new movement target.</param>
    /// <param name="rotation">The rotation of the new movement target.</param>
    /// <param name="parent">The parent object of the movement targets.</param>
    /// <returns>The created movement target.</returns>
    public static MovementTarget CreateMovementTarget(Vector3 position,
        Quaternion rotation, Transform parent)
    {
        GameObject newTargetObject = new();
        newTargetObject.transform.position = position;
        newTargetObject.transform.rotation = rotation;
        newTargetObject.transform.parent = parent;

        newTargetObject.AddComponent<MovementTarget>();

        return newTargetObject.GetComponent<MovementTarget>();
    }
}
