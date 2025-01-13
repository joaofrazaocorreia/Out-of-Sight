
using Interaction;
using UnityEngine;

public class Item : InteractiveObject
{
    [Header("Item Variables")]
    [SerializeField] private Sprite icon;
    [SerializeField] private ItemType _itemType;
    
    public Sprite Icon => icon;
    public ItemType ItemType => _itemType;
    
    public override void Interact()
    {
        Destroy(gameObject);
    }
}
