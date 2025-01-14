using UnityEngine;

public class EnemyItemInventory : MonoBehaviour
{
    [SerializeField] private Item itemDrop1;
    [SerializeField] [Range(0, 100)] private float drop1Chance; 
    [SerializeField] private GameObject drop1Model;
    [SerializeField] private Item itemDrop2;
    [SerializeField] [Range(0, 100)] private float drop2Chance; 
    [SerializeField] private GameObject drop2Model;
    [SerializeField] private Transform dropPosition;
    [SerializeField] private Transform itemsParent;

    public bool HasDrop1 {get => drop1Model!= null && drop1Model.activeSelf;}
    public bool HasDrop2 {get => drop2Model!= null && drop2Model.activeSelf;}

    private void Start()
    {
        if(itemDrop1 != null)
            drop1Model.SetActive(Random.Range(0, 101) <= drop1Chance);

        if(itemDrop2 != null)
            drop2Model.SetActive(Random.Range(0, 101) <= drop2Chance);
    }

    /// <summary>
    /// Forces a specific item to become active on this enemy.
    /// </summary>
    /// <param name="number">The index of the item to enable.</param>
    public void ForceEnableItemDrop(int number)
    {
        switch(Mathf.Clamp(number, 1, 2))
        {
            case 1:
                drop1Model.SetActive(true);
                break;
            case 2:
                drop2Model.SetActive(true);
                break;
        }
    }

    /// <summary>
    /// Causes a specific item to drop from this enemy.
    /// </summary>
    /// <param name="number">The index of the item to drop.</param>
    public void DropItem(int number)
    {
        switch(Mathf.Clamp(number, 1, 2))
        {
            case 1:
                Instantiate(itemDrop1.gameObject,
                    dropPosition.position, dropPosition.rotation, itemsParent);
                drop1Model.SetActive(false);
                break;
            case 2:
                Instantiate(itemDrop2.gameObject,
                    dropPosition.position, dropPosition.rotation, itemsParent);
                drop2Model.SetActive(false);
                break;
        }
    }

    /// <summary>
    /// Drops all items this enemy has.
    /// </summary>
    public void DropAllItems()
    {
        if(HasDrop1)
            DropItem(1);

        if(HasDrop2)
            DropItem(2);
    }
}
