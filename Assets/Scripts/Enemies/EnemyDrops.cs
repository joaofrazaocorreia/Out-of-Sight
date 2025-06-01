using System.Collections.Generic;
using UnityEngine;

public class EnemyDrops : MonoBehaviour
{
    [SerializeField] private List<GameObject> drops;
    [SerializeField][Range(0, 100)] private List<float> dropChances;
    [SerializeField] private Transform itemsParent;

    public void SetItemDrops()
    {
        GetComponentInChildren<EnemyItemInventory>().ItemsParent = itemsParent;
        GetComponentInChildren<EnemyItemInventory>().SetItemDrops(drops, dropChances);
    }
}
