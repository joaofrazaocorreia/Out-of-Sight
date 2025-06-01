using System.Collections.Generic;
using UnityEngine;

public class EnemyItemInventory : MonoBehaviour
{
    [SerializeField] private List<Transform> dropHeldPositions;
    [SerializeField] private Transform hiddenHeldPos;

    private List<GameObject> itemsList;
    private Transform itemsParent;
    public Transform ItemsParent { get => itemsParent; set => itemsParent = value; }

    private void Start()
    {
        if (itemsList == null)
        {
            itemsList = new List<GameObject>();
        }
    }

    public void SetItemDrops(List<GameObject> drops, List<float> dropChances)
    {
        itemsList = new List<GameObject>(drops.Count);

        for (int i = 0; i < drops.Count; i++)
        {
            itemsList.Add(null);
            if (drops[i] == null) continue;

            float chance = i < dropChances.Count ? dropChances[i] : 0f;
            float roll = Random.Range(0f, 100f);

            GameObject createdItem = CreateVisualHeldItem(drops[i], i);

            if (roll > chance)
            {
                createdItem.SetActive(false);
            }
            
            else
            {
                itemsList[i] = createdItem;
            }
        }
    }

    private GameObject CreateVisualHeldItem(GameObject prefab, int index)
    {
        GameObject createdItem;

        if (index < dropHeldPositions.Count)
        {
            createdItem = Instantiate(prefab, dropHeldPositions[index].transform.position,
                dropHeldPositions[index].transform.rotation, dropHeldPositions[index].transform);
        }

        else
        {
            createdItem = Instantiate(prefab, hiddenHeldPos.transform.position,
                hiddenHeldPos.transform.rotation, hiddenHeldPos.transform);

            createdItem.SetActive(false);
        }

        createdItem.name = prefab.name;
        createdItem.GetComponent<Item>().enabled = false;

        return createdItem;
    }

    /// <summary>
    /// Causes a specific item to drop from this enemy.
    /// </summary>
    /// <param name="index">The index of the item to drop.</param>
    public void DropItem(int index)
    {
        if (itemsList[index] != null)
        {
            itemsList[index].SetActive(true);
            itemsList[index].transform.parent = ItemsParent;

            itemsList[index].GetComponent<Item>().enabled = true;
            itemsList[index].GetComponent<Collider>().enabled = true;
            itemsList[index].AddComponent<Rigidbody>();
        }
    }

    /// <summary>
    /// Drops all items this enemy has.
    /// </summary>
    public void DropAllItems()
    {
        for (int i = 0; i < itemsList.Count; i++)
        {
            if (itemsList[i] != null)
                DropItem(i);
        }
    }
}
