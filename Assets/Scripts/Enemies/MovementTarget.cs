using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovementTarget : MonoBehaviour
{
    [SerializeField] private float minStayDuration = 10;
    [SerializeField] private List<Transform> targetPositions;
    private Dictionary<EnemyMovement, Transform> currentEnemies;
    public bool Occupied
    {
        get => currentEnemies.Count >= targetPositions.Count;
    }

    private void Start()
    {
        currentEnemies = new Dictionary<EnemyMovement, Transform>();

        if(targetPositions.Count <= 0)
            targetPositions.Add(Instantiate(new GameObject(), transform.position,
                transform.rotation, transform).transform);
    }

    public Transform Occupy(EnemyMovement enemy, bool canChooseLastPos = false, float moveTimeMultiplier = 1f)
    {
        Transform targetPos = GetAvailablePos();

        if(targetPos != null)
        {
            currentEnemies.Add(enemy, targetPos);
            enemy.MoveTo(targetPos.position, canChooseLastPos, moveTimeMultiplier);
            enemy.RotateTo(targetPos.eulerAngles.y);
        }

        return targetPos;
    }

    public void Deoccupy(EnemyMovement enemy)
    {
        currentEnemies.Remove(enemy);
    }

    private Transform GetAvailablePos()
    {
        foreach(Transform t in targetPositions)
        {
            if(!currentEnemies.Values.Contains(t))
                return t;
        }

        return null;
    }
}
