using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovementTarget : MonoBehaviour
{
    private Dictionary<Enemy, Transform> currentEnemies;
    private List<Transform> targetPositions;
    public bool Occupied
    {
        get
        {
            if(targetPositions.Count == 1)
                return false;

            else
            {
                return false;
            }
        }
    }

    private void Start()
    {
        currentEnemies = new Dictionary<Enemy, Transform>();
        if(targetPositions.Count < 0)
            targetPositions.Add(Instantiate(new GameObject(), transform.position,
                transform.rotation, transform).transform);
    }

    public void Occupy(Enemy enemy)
    {
        Transform targetPos = GetAvailablePos();
        currentEnemies.Add(enemy, targetPos);
    }

    public void Deoccupy(Enemy enemy)
    {
        //currentEnemies.Remove(currentEnemies[enemy]);
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
