
using UnityEngine;

public class Item : InteractiveObject
{
    [SerializeField] private Sprite icon;

    [SerializeField] private string itemName;
    
    public Sprite Icon => icon;
    public string ItemName => itemName;
    
    public override void Interact()
    {
        Destroy(gameObject);
        return;
    }
}
