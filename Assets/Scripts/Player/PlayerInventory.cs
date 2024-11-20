using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private Dictionary<Item, int> inventory = new Dictionary<Item, int>();

    public int GetItemQuantity(Item item) => inventory[item];
    
    public bool HasItem(Item item) => inventory.ContainsKey(item);
    
    public void AddItem(Item item, int amount) => inventory.Add(item, amount);
    
    public void RemoveItem(Item item)
    {
        inventory[item]--;
        if(inventory[item] == 0) inventory.Remove(item);
        print(inventory);
    }
}
