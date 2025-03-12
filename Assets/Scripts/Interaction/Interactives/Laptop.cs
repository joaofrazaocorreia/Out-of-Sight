using UnityEngine;

namespace Interaction
{
    public class Laptop : InteractiveObject
    {
        [SerializeField] private Item SecretFiles;
        public override void Interact()
        {
            base.Interact();
        
            FindFirstObjectByType<PlayerInventory>().AddItem(SecretFiles );
        }
    }
}