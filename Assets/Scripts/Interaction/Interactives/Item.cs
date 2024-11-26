
using UnityEngine;

public class Item : InteractiveObject
{
    [SerializeField] private Sprite icon;
    
    public Sprite Icon => icon;
    
    public override void Interact()
    {
        Destroy(gameObject);
    }
}
