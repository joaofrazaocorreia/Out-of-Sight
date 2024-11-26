using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private Sprite emptySlotSprite;
    private UIManager uIManager;
    private Dictionary<Item, int> inventory = new Dictionary<Item, int>();

    private void Start()
    {
        uIManager = FindFirstObjectByType<UIManager>();
    }

    public int GetItemQuantity(Item item) => inventory[item];
    
    public bool HasItem(Item item) => inventory.ContainsKey(item);
    
    public void AddItem(Item item, int amount)
    {
        inventory.Add(item, amount);
        uIManager.UpdateInventoryIcon(item.Icon, inventory[item]);
    }

    public void RemoveItem(Item item)
    {
        uIManager.UpdateInventoryIcon(emptySlotSprite, inventory[item]);
        inventory[item]--;
        if(inventory[item] == 0) inventory.Remove(item);
    }
}
