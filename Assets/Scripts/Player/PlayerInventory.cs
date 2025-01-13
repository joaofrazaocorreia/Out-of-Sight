using System;
using System.Collections.Generic;
using Interaction;
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
        uIManager.UpdateInventoryIcon(newItem.Icon, Inventory.IndexOf(newItem));
    }

    public void RemoveItem(ItemType itemType)
    {
        for (int i = 0; i < Inventory.Count; i++)
        {
            if (itemType == Inventory[i].ItemType)
            {
                uIManager.UpdateInventoryIcon(Inventory[i].Icon, Inventory.IndexOf(Inventory[i]));
                Inventory.RemoveAt(i);
                break;
            }
        }
    }
}
