
using UnityEngine;

public class Item : InteractiveObject
{
    [SerializeField] private string itemName;
    
    public string ItemName => itemName;
    
    public override void Interact()
    {
        Destroy(gameObject);
        return;
    }
}
