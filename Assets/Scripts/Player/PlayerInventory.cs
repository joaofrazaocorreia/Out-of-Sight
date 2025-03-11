using System;
using System.Collections.Generic;
using Interaction;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private Sprite emptySlotSprite;
    private List<Item> inventory = new List<Item>();
    
    public List<Item> Inventory
    {
        get { return inventory; }
        private set { inventory = value; }
    }
    
    public Sprite NewIcon {get; private set;}
    public int UpdatedItemIndex {get; private set;}
    
    public event EventHandler OnInventoryUpdated;
    
    public bool HasItem(ItemType itemType)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if(itemType == inventory[i].ItemType) return true;
        }   
        return false;
    }
    
    public void AddItem(Item newItem)
    {
        Inventory.Add(newItem);
        NewIcon = newItem.Icon;
        UpdatedItemIndex = Inventory.IndexOf(newItem);
        OnInventoryUpdated?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveItem(ItemType itemType)
    {
        for (int i = 0; i < Inventory.Count; i++)
        {
            if (itemType == Inventory[i].ItemType)
            {
                NewIcon = Inventory[i].Icon;
                UpdatedItemIndex = Inventory.IndexOf(Inventory[i]);
                Inventory.RemoveAt(i);
                OnInventoryUpdated?.Invoke(this, EventArgs.Empty);
                break;
            }
        }
    }
}
