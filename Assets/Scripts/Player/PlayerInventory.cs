using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private Sprite emptySlotSprite;
    private UIManager uIManager;
    private List<Item> inventory = new List<Item>();
    
    public List<Item> Inventory
    {
        get { return inventory; }
        private set { inventory = value; }
    }

    private void Start()
    {
        uIManager = FindFirstObjectByType<UIManager>();
    }
    
    public bool HasItem(Item item) => Inventory.Contains(item);
    
    public void AddItem(Item newItem)
    {
        Inventory.Add(newItem);
        uIManager.UpdateInventoryIcon(newItem.Icon, Inventory.IndexOf(newItem));
    }

    public void RemoveItem(Item item)
    {
        uIManager.UpdateInventoryIcon(emptySlotSprite, Inventory.IndexOf(item));
        Inventory.Remove(item);
    }
}
